using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.RegisterCompany;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.ValidateUser;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.WebApi.Authentication;
using SWI.SoftStock.WebApi.Mapper;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace SWI.SoftStock.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly ISecurityService securityService;
        private readonly IJwtAuthManager jwtAuthManager;
        private readonly IUserService userService;
        private readonly ExpiredTokenValidationParameters expiredTokenValidationParameters;

        public AccountController(ISecurityService securityService, IUserService userService,
            IJwtAuthManager jwtAuthManager, ExpiredTokenValidationParameters expiredTokenValidationParameters)
        {
            this.securityService = securityService;
            this.jwtAuthManager = jwtAuthManager;
            this.userService = userService;
            this.expiredTokenValidationParameters = expiredTokenValidationParameters;
        }

        [Route("ChangePassword")]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (this.ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                var result =
                    await this.securityService.ChangePassword(userId, model.OldPassword, model.NewPassword);
                if (result.Success)
                {
                    return this.Ok();
                }

                return this.BadRequest(string.Join(", ", result.Errors));
            }

            return this.BadRequest(new { this.ModelState });
        }

        [AllowAnonymous]
        [Route("Register")]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (this.ModelState.IsValid)
            {
                var request = new RegisterCompanyRequest { RegisterModel = model, AccountName = "Free" };
                var result = await this.securityService.RegisterCompanyAsync(request);
                if (result.Status == RegisterCompanyStatus.Success)
                {
                    return this.Ok();
                }

                var errorMessage = RegisterCompanyStatusMessage.GetErrorMessage(result.Status);
                switch (result.Status)
                {
                    case RegisterCompanyStatus.InvalidUserName:
                        break;
                    case RegisterCompanyStatus.InvalidPassword:
                        break;
                    case RegisterCompanyStatus.InvalidEmail:
                        this.ModelState.AddModelError(nameof(RegisterModel.Email), errorMessage);
                        break;
                    case RegisterCompanyStatus.DuplicateUserName:
                        this.ModelState.AddModelError(nameof(RegisterModel.UserName), errorMessage);
                        break;
                    case RegisterCompanyStatus.DuplicateEmail:
                        this.ModelState.AddModelError(nameof(RegisterModel.Email), errorMessage);
                        break;
                    case RegisterCompanyStatus.ProviderError:
                        break;
                    case RegisterCompanyStatus.DuplicateCompany:
                        this.ModelState.AddModelError(nameof(RegisterModel.CompanyName), errorMessage);
                        break;
                    case RegisterCompanyStatus.UnknownError:
                        break;
                }
            }

            return this.BadRequest(new { this.ModelState });
        }

        [AllowAnonymous]
        [Route("Verify")]
        [HttpPost]
        public async Task<IActionResult> Verify(VerifyModel model)
        {
            if (model.UserId == null)
            {
                return this.BadRequest("userId is missing");
            }

            if (model.Code == null)
            {
                return this.BadRequest("code is missing");
            }

            bool result;
            try
            {
                result = await this.securityService.VerifyAsync(model.UserId, model.Code);
            }
            catch (InvalidOperationException)
            {
                return this.BadRequest("userId is corrupt");
            }

            if (result)
            {
                return this.Ok();
            }

            return this.BadRequest("error");
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest();
            }

            var validationResult = await this.securityService.ValidateUser(
                new ValidateUserRequest()
                { Password = request.Password, UserName = request.UserName });
            switch (validationResult.Status)
            {
                case ValidateUserStatus.Fail:
                    return this.Unauthorized("Wrong name or password");
                case ValidateUserStatus.NotApproved:
                    return this.Unauthorized("Please, confirm your account");
            }

            if (validationResult.Status != ValidateUserStatus.Success)
            {
                return this.Unauthorized();
            }

            var jwtResult = await this.GenerateTokens(validationResult.User, true);

            return this.Ok(new LoginResult
            {
                UserName = request.UserName,
                AccessToken = jwtResult.AccessToken,
                RefreshToken = jwtResult.RefreshToken.TokenString
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("refresh")]
        public async Task<ActionResult> Refresh([FromBody] RefreshModel refreshModel)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(refreshModel.AccessToken,
                this.expiredTokenValidationParameters,
                out var securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature,
                    StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userName = principal.FindFirst(ClaimTypes.Name).Value;

            var savedRefreshToken =
                await this.userService.GetRefreshTokenAsync(
                    Guid.Parse(userId)); //retrieve the refresh token from a data store

            if (savedRefreshToken.TokenString != refreshModel.RefreshToken ||
                savedRefreshToken.ExpireAt < DateTime.UtcNow)
            {
                throw new SecurityTokenException("Invalid token");
            }

            var user = await this.securityService.GetUserAsync(userName);
            var jwtResult = await this.GenerateTokens(user, false);
            return this.Ok(new RefreshResult
            {
                AccessToken = jwtResult.AccessToken,
                RefreshToken = refreshModel.RefreshToken
            });
        }

        private async Task<JwtAuthResult> GenerateTokens(User user, bool setRefreshToken)
        {
            var allRoles = await this.userService.GetUserStructureUnitRolesAsync(user.Id);
            var uniqueRoles = allRoles.Select(role => role.RoleName).Distinct().OrderBy(role => role);

            var claims = new List<Claim>(uniqueRoles.Select(role => new Claim("role", role)))
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Company.UniqueId.ToString())
            };
            var jwtResult = await this.jwtAuthManager.GenerateTokens(user, claims.ToArray(), DateTime.UtcNow, setRefreshToken);
            return jwtResult;
        }
    }
}
