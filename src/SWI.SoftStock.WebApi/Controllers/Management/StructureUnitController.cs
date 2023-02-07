﻿using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWI.SoftStock.ServerApps.DataModel2.Identity;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using System;
using SWI.SoftStock.WebApi.Common;

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
        public IActionResult StructureUnits([FromQuery] StructureUnitsRequestModel request)
        {
            const string role = "Manager";

            var parsed = Guid.TryParse(request.SelectedStructureUnitId, out var uniqueId);
                       

            var tree = this.structureUnitService.GetStructureUnitModels(Guid.Parse(UserId),
                parsed
                    ? (Guid?)
                        uniqueId
                    : null,
                new[] { role },
                out var selectedStructureUnit);
            return this.Ok(tree);
        }
    }
}
