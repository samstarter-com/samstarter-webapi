using Microsoft.IdentityModel.Tokens;
using SWI.SoftStock.ServerApps.DataModel2;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SWI.SoftStock.ServerApps.WebApplicationContracts;

namespace SWI.SoftStock.WebApi.Authentication
{
    public class JwtAuthManager : IJwtAuthManager
    {
        private readonly JwtTokenConfig _jwtTokenConfig;
        private readonly byte[] _secret;
        private readonly IUserService userService;

        public JwtAuthManager(JwtTokenConfig jwtTokenConfig, IUserService userService)
        {
            this._jwtTokenConfig = jwtTokenConfig;
            this._secret = Encoding.ASCII.GetBytes(jwtTokenConfig.Secret);
            this.userService = userService;
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
            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

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

        private static string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[32];
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}