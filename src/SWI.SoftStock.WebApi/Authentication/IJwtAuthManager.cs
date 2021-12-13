using SWI.SoftStock.ServerApps.DataModel2;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SWI.SoftStock.WebApi.Authentication
{
    public interface IJwtAuthManager
    { 
        Task<JwtAuthResult> GenerateTokens(User user, Claim[] claims, DateTime now, bool setRefreshToken);
    }
}