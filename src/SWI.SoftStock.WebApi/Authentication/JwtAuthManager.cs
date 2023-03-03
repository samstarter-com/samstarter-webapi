using Microsoft.IdentityModel.Tokens;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SWI.SoftStock.WebApi.Authentication
{
    public class JwtAuthManager : IJwtAuthManager
    {
        private readonly JwtTokenConfig _jwtTokenConfig;
        private readonly byte[] _secret;
        private readonly IUserService userService;
        private readonly JwtSecurityTokenHandler jwtSecurityTokenHandler;
        private readonly ExpiredTokenValidationParameters expiredTokenValidationParameters;

        public JwtAuthManager(JwtTokenConfig jwtTokenConfig
            , IUserService userService
            , JwtSecurityTokenHandler jwtSecurityTokenHandler
            , ExpiredTokenValidationParameters expiredTokenValidationParameters)
        {
            this._jwtTokenConfig = jwtTokenConfig;
            this._secret = Encoding.ASCII.GetBytes(jwtTokenConfig.Secret);
            this.userService = userService;
            this.jwtSecurityTokenHandler = jwtSecurityTokenHandler;
            this.expiredTokenValidationParameters = expiredTokenValidationParameters;
        }

        public async Task<JwtAuthResult> GenerateTokens(User user, Claim[] claims, DateTime now, bool setRefreshToken)
        {
            var shouldAddAudienceClaim = string.IsNullOrWhiteSpace(claims?.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Aud)?.Value);
            var jwtToken = new JwtSecurityToken(
                this._jwtTokenConfig.Issuer,
                shouldAddAudienceClaim ? this._jwtTokenConfig.Audience : string.Empty,
                claims,
                expires: now.AddMinutes(this._jwtTokenConfig.AccessTokenExpiration),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(this._secret), SecurityAlgorithms.HmacSha256Signature));
            var accessToken = jwtSecurityTokenHandler.WriteToken(jwtToken);

            RefreshToken refreshToken = null;
            if (setRefreshToken)
            {
                refreshToken = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    TokenString = GenerateRefreshTokenString(),
                    ExpireAt = now.AddMinutes(this._jwtTokenConfig.RefreshTokenExpiration)
                };
                await this.userService.SetRefreshTokenAsync(user, refreshToken);
            }

            return new JwtAuthResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public ClaimsPrincipal ValidateToken(string accessToken, out SecurityToken validatedToken)
        {
            var result = jwtSecurityTokenHandler.ValidateToken(accessToken,
                this.expiredTokenValidationParameters,
                out var securityToken);           
                validatedToken = securityToken;
            return result;
        }

        private static string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[32];
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}