using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SWI.SoftStock.ServerApps.DataModel2.Identity;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.WebApi.Common;
using System;
using System.Threading.Tasks;

namespace SWI.SoftStock.WebApi.Controllers.Management
{
    [ApiController]
    [Authorize(Policy = Constants.PolicyManager)]
    [Route("api/management/structureunit")]
    public class StructureUnitController : AuthorizedBaseController
    {
        readonly IStructureUnitService structureUnitService;
        readonly IUserService userService;
        readonly ILogger<StructureUnitController> log;
        readonly CustomRoleManager rolemanager;

        public StructureUnitController(ILogger<StructureUnitController> log, IStructureUnitService structureUnitService, IUserService userService, CustomRoleManager rolemanager)
        {
            this.structureUnitService = structureUnitService;
            this.userService = userService;
            this.log = log;
            this.rolemanager = rolemanager;
        }

        [Authorize(Roles = "Admin, Manager")]
        [Route("")]
        [HttpGet]
        public async Task<IActionResult> StructureUnits([FromQuery] StructureUnitsRequestModel request)
        {
            const string role = "Manager";

            var parsed = Guid.TryParse(request.SelectedStructureUnitId, out var uniqueId);
                       

            var res = await this.structureUnitService.GetStructureUnitModels(Guid.Parse(UserId),
                parsed
                    ?
                        uniqueId
                    : null,
                new[] { role });
            return this.Ok(res.Item1);
        }
    }
}
