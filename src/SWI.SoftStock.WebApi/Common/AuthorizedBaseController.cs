using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SWI.SoftStock.WebApi.Common
{
    public abstract class AuthorizedBaseController : ControllerBase
    {
        internal string UserId => ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value;
    }
}
