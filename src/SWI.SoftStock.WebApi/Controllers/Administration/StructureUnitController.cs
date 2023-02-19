using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SWI.SoftStock.ServerApps.DataModel2.Identity;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationContracts.UserService.SetUsersRoles;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.WebApi.Common;
using SWI.SoftStock.WebApi.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWI.SoftStock.WebApi.Controllers.Administrations
{
    [ApiController]
    [Authorize(Policy = Constants.PolicyAdministrator)]
    [Route("api/administration/structureunits")]
    public class AdminStructureUnitController : AuthorizedBaseController
    {
        private readonly IStructureUnitService structureUnitService;

        private readonly IUserService userService;

        private readonly ILogger<AdminStructureUnitController> log;

        private readonly CustomRoleManager rolemanager;

        public AdminStructureUnitController(ILogger<AdminStructureUnitController> log, IStructureUnitService structureUnitService, IUserService userService, CustomRoleManager rolemanager)
        {
            this.structureUnitService = structureUnitService;
            this.userService = userService;
            this.log = log;
            this.rolemanager = rolemanager;
        }

        [Route("")]
        [HttpGet]
        public IActionResult StructureUnits([FromQuery] StructureUnitsRequestModel request)
        {
            const string role = "Admin";

            var parsed = Guid.TryParse(request?.SelectedStructureUnitId, out var uniqueId);           

            var tree = this.structureUnitService.GetStructureUnitModels(Guid.Parse(UserId),
                parsed
                    ? uniqueId
                    : null,
                new[] { role });
            return this.Ok(tree);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetDetails(string id)
        {          
            var userGuid = Guid.Parse(UserId);
            if (!Guid.TryParse(id, out var uniqueId))
            {
                uniqueId = (await this.structureUnitService.GetStructureUnitModels(userGuid,
                    null,
                    new[] { Constants.RoleAdministrator })).StructureUnits.First().UniqueId;
            }

            var adminRoleId = this.rolemanager.Roles.Single(r => r.Name == Constants.RoleAdministrator).Id;
            var detail = await this.structureUnitService.GetByUniqueId(uniqueId);
            detail.IsRootUnit = detail.ParentUniqueId == null || (await this.userService.GetUserRoles(detail.ParentUniqueId.Value, userGuid)).All(urm => urm.RoleId != adminRoleId);

            return this.Ok(new
            {
                Details = detail,
                UsersRoles = this.userService.GetStructureUnitUserRoles(uniqueId)
            });
        }

        [HttpPut]
        [Route("")]
        public async Task<IActionResult> Add(StructureUnitModel model)
        {
            if (this.ModelState.IsValid)
            {
                var res = await this.structureUnitService.CreateAndAdd(model, model.ParentUniqueId.Value);
                var uniqueId = res.StructureUnitId;
                var status = res.Status;
                switch (status)
                {
                    case StructureUnitCreationStatus.Success:
                        return this.Ok(
                            new { success = true, id = uniqueId.Value.ToString() });
                    case StructureUnitCreationStatus.ParentNotFound:
                        this.log.LogError("Cannot add. Parent is null. model:{0} userId:{1} parentId:{2}",
                            model,
                            UserId,
                            model.ParentUniqueId.Value);
                        return this.StatusCode(StatusCodes.Status500InternalServerError,
                            new FailedResponse() { Errors = new[] { "Cannot add" } });
                    case StructureUnitCreationStatus.NonUnique:
                        this.ModelState.AddModelError("Name",
                            $"Structure unit with name {model.Name} exists in your company. Structure unit must be unique.");
                        IEnumerable<string> errorsFromModelState = new string[] { }; // = GetErrorsFromModelState();
                        this.log.LogWarning("Cannot add. model:{0} userId:{1} parentId:{2} Errors:{3}",
                            model,
                            UserId,
                            model.ParentUniqueId.Value,
                            errorsFromModelState.ToArray());
                        return this.StatusCode(StatusCodes.Status500InternalServerError,
                            new FailedResponse() { Errors = this.ModelState.Values.SelectMany(v => v.Errors).Select(er => er.ErrorMessage).ToArray() });
                }

                this.log.LogWarning("Result is null. model:{0} userId:{1} parentId:{2}",
                    model,
                    UserId,
                    model.ParentUniqueId.Value);

                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }

            return this.StatusCode(StatusCodes.Status500InternalServerError,
                new FailedResponse() { Errors = this.ModelState.Values.SelectMany(v => v.Errors).Select(er => er.ErrorMessage).ToArray() });
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Update(StructureUnitModel model)
        {
            if (this.ModelState.IsValid)
            {
                StructureUnitUpdateStatus status = await this.structureUnitService.Update(model);
                switch (status)
                {
                    case StructureUnitUpdateStatus.Success:
                        return this.Ok(new { success = true, id = model.UniqueId.ToString() });
                    case StructureUnitUpdateStatus.NotExist:
                        this.log.LogWarning("Cannot update. Structure unit not found. model:{0}", model);
                        return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = new[] { "Cannot update" } });
                    case StructureUnitUpdateStatus.ParentStructureUnitIsSame:
                        this.log.LogWarning("Cannot update. Wrong parent unit. model:{0}", model);
                        return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = new[] { "Cannot update" } });

                    case StructureUnitUpdateStatus.NonUnique:
                        IEnumerable<string> errorsFromModelState = new string[] { };
                        this.log.LogWarning("Cannot update. model:{0} parentId:{1} Errors:{2}",
                                    model,
                                    UserId, errorsFromModelState);
                        return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = errorsFromModelState.ToArray() });

                }

                this.log.LogWarning("Result is null. model:{0} userId:{1}", model, UserId);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
            return this.StatusCode(StatusCodes.Status500InternalServerError, this.ModelState);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            Guid? parentUniqueId = await this.structureUnitService.GetParentUniqueId(id);
            StructureUnitDeleteStatus statuses = await this.structureUnitService.DeleteByUniqueId(id);
            if (statuses > (int)StructureUnitDeleteStatus.None)
            {
                var errors = new List<string>();
                this.log.LogWarning("Cannot delete. id:{0} userId:{1}", id, UserId);
                foreach (var status in ((StructureUnitDeleteStatus[])Enum.GetValues(typeof(StructureUnitDeleteStatus))).Where(s => s != StructureUnitDeleteStatus.None))
                {
                    if (statuses.HasFlag(status))
                    {
                        errors.Add(StructureUnitDeleteStatusMessage.GetErrorMessage(status));
                    }
                }
                return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = errors.ToArray() });
            }
            return this.Ok(new { success = true, id = parentUniqueId });
        }

        [HttpPost]
        [Route("{id}/role")]
        public async Task<IActionResult> GrantRole([FromBody] GrantRoleRequest data, Guid id)
        {
            var setUsersRolesRequest = new SetUsersRolesRequest
            {
                UserId = data.UserId,
                StructureUnitId = id,
                Roles = new[] { new RoleData { RoleId = data.RoleId, IsInRole = true } }
            };

            var res = await this.userService.SetUsersRolesAsync(setUsersRolesRequest);

            switch (res.Status)
            {
                case UserRoleUpdateStatus.Success:
                    return this.Ok(new { success = true });
                case UserRoleUpdateStatus.UserNotExist:
                    this.log.LogWarning("User not found. UserId:{0}", data.UserId);
                    return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = new[] { "User not found" } });
                case UserRoleUpdateStatus.StructureUnitNotExist:
                    this.log.LogWarning("Structure unit not found. StructureUnitIds:{0}", id);
                    return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = new[] { "Structure unit not found" } });
                case UserRoleUpdateStatus.IsLastAdmin:
                    this.log.LogWarning("Must be at least one administrator in company. UserId:{0}", data.UserId);
                    return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = new[] { "Must be at least one administrator in company" } });
                default:
                    this.log.LogWarning("Run time error during userService.SetUsersRoles. UserId:{0}", data.UserId);
                    return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = new[] { "Something go wrong" } });
            }
        }

        [HttpDelete]
        [Route("{id}/role")]
        public async Task<IActionResult> RemoveRole([FromBody] GrantRoleRequest data, Guid id)
        {
            var setUsersRolesRequest = new SetUsersRolesRequest
            {
                UserId = data.UserId,
                StructureUnitId = id,
                Roles = new RoleData[] { new RoleData { RoleId = data.RoleId, IsInRole = false } }
            };

            var res = await this.userService.SetUsersRolesAsync(setUsersRolesRequest);
            switch (res.Status)
            {
                case UserRoleUpdateStatus.Success:
                    return this.Ok(new { success = true });
                case UserRoleUpdateStatus.UserNotExist:
                    this.log.LogWarning("User not found. UserId:{0}", data.UserId);
                    return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = new[] { "User not found" } });
                case UserRoleUpdateStatus.StructureUnitNotExist:
                    this.log.LogWarning("Structure unit not found. StructureUnitIds:{0}", id);
                    return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = new[] { "Structure unit not found" } });
                case UserRoleUpdateStatus.IsLastAdmin:
                    this.log.LogWarning("Must be at least one administrator in company. UserId:{0}", data.UserId);
                    return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = new[] { "Must be at least one administrator in company" } });
                default:
                    this.log.LogWarning("Run time error during userService.SetUsersRoles. UserId:{0}", data.UserId);
                    return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = new[] { "Something go wrong" } });
            }
        }
    }

    public class GrantRoleRequest
    {
        public Guid UserId { get; set; }

        public Guid RoleId { get; set; }
    }
}
