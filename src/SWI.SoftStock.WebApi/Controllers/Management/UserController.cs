using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWI.SoftStock.WebApi.Common;
using System;
using System.Threading.Tasks;
using SWI.SoftStock.ServerApps.WebApplicationContracts;

namespace SWI.SoftStock.WebApi.Controllers.Management
{
    [ApiController]
    [Authorize(Policy = Constants.PolicyManager)]
    [Route("api/management/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet]
        [Route("autocomplete")]
        public async Task<IActionResult> UsersAutocomplete([FromQuery] string cid, [FromQuery] string request = "")
        {
            var isGuid = Guid.TryParse(cid, out var cidGuid);
            if (!isGuid)
            {
                return this.Ok();
            }

            var modelForAutocomplete = await this.userService.GetForAutocompleteAsync(cidGuid, request);
            var autocompleteresult = new { users = modelForAutocomplete, totalRecords = 0, structureUnitId = cid };
            return this.Ok(autocompleteresult);
        }
    }
}
