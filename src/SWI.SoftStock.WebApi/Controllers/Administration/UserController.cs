using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.CreateUser;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationContracts.UserService.GetByStructureUnitId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.UserService.SetUsersRoles;
using SWI.SoftStock.ServerApps.WebApplicationContracts.UserService.UserUnLock;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;
using SWI.SoftStock.ServerApps.WebApplicationServices;
using SWI.SoftStock.WebApi.Common;
using SWI.SoftStock.WebApi.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SWI.SoftStock.ServerApps.WebApplicationContracts;

namespace SWI.SoftStock.WebApi.Controllers.Administration
{
    [ApiController]
    [Authorize(Policy = Constants.PolicyAdministrator)]
    [Route("api/administration/users")]
    public class UserController : AuthorizedBaseController
    {
        private readonly ILogger<UserController> log;

        private readonly IUserService userService;

        private readonly ISecurityService securityService;

        public UserController(ILogger<UserController> log,
           IUserService userService,
           ISecurityService securityService)
        {
            this.log = log;
            this.userService = userService;
            this.securityService = securityService;
        }

        [HttpGet]
        [Route("{userId}")]
        public async Task<IActionResult> GetById(Guid userId)
        {
            var data = new
            {
                Details = this.userService.GetById(userId), //TODO: if lastactivitydate==null then show message "No date"
                Roles = await this.userService.GetUserStructureUnitRolesAsync(userId)
            };
            return this.Ok(data);
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Users([FromQuery] string cid, [FromQuery] PagingModel paging, [FromQuery] OrderingModel ordering, [FromQuery] int includeSubItems = 0)
        {
            var response = new GetByStructureUnitIdResponse();
            var isGuid = Guid.TryParse(cid, out var cidGuid);
            if (isGuid)
            {
                var getByStructureUnitIdRequest = new GetByStructureUnitIdRequest
                {
                    StructureUnitId = cidGuid,
                    IncludeItemsOfSubUnits = includeSubItems == 1,
                    Paging = MapperFromViewToModel.MapToPaging(paging),
                    Ordering = MapperFromViewToModel.MapToOrdering(ordering)
                };
                response = await this.userService.GetByStructureUnitId(getByStructureUnitIdRequest);
            }
            else
            {
                response.Model = new UserCollection(ordering.Order, ordering.Sort);
            }

            var result = new
            {
                items = response.Model.Items,
                totalRecords = response.Model.TotalRecords,
                structureUnitId = cid,
                pageIndex = paging.PageIndex,
                pageSize = paging.PageSize,
                sortData = response.Model.SortModels,
                sortedTableHeader = response.Model.SortedTableHeader,
                sortedProperty = ordering.Sort,
                order = ordering.Order,
                includeSubItems
            };
            return this.Ok(result);
        }

        [HttpGet]
        [Route("{userId}/roles")]
        public IActionResult GetUserRoles(Guid userId, [FromQuery] Guid structureUnitId)
        {
            var model = this.userService.GetUserRoles(structureUnitId, userId);
            return this.Ok(model);
        }

        [HttpPost]
        [Route("{userId}/roles")]
        public async Task<IActionResult> ChangeRole(Guid userId, [FromBody] IEnumerable<UserRoleModel> roles)
        {           

            var suIds = roles.Select(r => r.StructureUnitId).Distinct();
            if (suIds.Any(suId => !this.userService.IsUserInRole(Guid.Parse(UserId), Constants.RoleAdministrator, suId).Result))
            {
                throw new UnauthorizedAccessException();
            }

            var setUsersRolesRequests = roles.Select(r => new SetUsersRolesRequest
            {
                UserId = userId,
                Roles = new[] { new RoleData() { RoleId = r.RoleId, IsInRole = r.IsInRole } },
                StructureUnitId = r.StructureUnitId
            });

            SetUsersRolesResponse resp = null;
            foreach (var setUsersRolesRequest in setUsersRolesRequests)
            {
                resp = await this.userService.SetUsersRolesAsync(setUsersRolesRequest);
            }

            switch (resp.Status)
            {
                case UserRoleUpdateStatus.Success:
                    return this.Ok(new { success = true });
                case UserRoleUpdateStatus.UserNotExist:
                    this.log.LogWarning("User not found. UserId:{0}", userId);
                    return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = new[] { "User not found" } });
                case UserRoleUpdateStatus.StructureUnitNotExist:
                    this.log.LogWarning("Some of structure unit not found. StructureUnitIds:{0}",
                                       string.Join(" ;", roles.Select(r => r.StructureUnitId)));
                    return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = new[] { "Some of structure unit not found" } });
                case UserRoleUpdateStatus.IsLastAdmin:
                    this.log.LogWarning("Must be at least one administrator in company. UserId:{0}", userId);
                    return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = new[] { "Must be at least one administrator in company" } });
                case UserRoleUpdateStatus.RunTime:
                    throw new NotImplementedException();
                default:
                    this.log.LogWarning("Run time error during userService.SetUsersRoles. UserId:{0}", userId);
                    return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = new[] { "Run time error during userService.SetUsersRoles" } });
            }
        }

        [HttpPost]
        [Route("{userId}")]
        public async Task<IActionResult> Update(UserModelEx model)
        {          
            if (this.ModelState.IsValid)
            {
                var status = await this.userService.UpdateAsync(model);
                switch (status)
                {
                    case UserUpdateStatus.Success:
                        return this.Ok(new { success = true });
                    case UserUpdateStatus.NotExist:
                        this.log.LogWarning("Cannot update. User not found. model:{0}", model);
                        return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = new[] { "User not found" } });
                    case UserUpdateStatus.NonUnique:
                        var error = $"User with name {model.UserName} exists. User must be unique.";
                        this.log.LogWarning("Cannot update. model:{0} parentId:{1} Error:{2}", model, UserId, error);
                        return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = new[] { error } });
                    case UserUpdateStatus.EmailNonUnique:
                        error = $"User with email {model.Email} exists. User must have unique email.";
                        this.log.LogWarning("Cannot update. model:{0} parentId:{1} Error:{2}", model, UserId, error);
                        return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = new[] { error } });
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return this.BadRequest(new { this.ModelState });
        }

        [HttpPut]
        [Route("")]
        public async Task<IActionResult> Add(UserModelWithPasswordEx model)
        {
            if (this.ModelState.IsValid)
            {
                var request = new CreateUserRequest { UserModel = model, StructureUnitId = model.StructureUnitId };
                var result = await this.securityService.CreateUserAsync(request);
                if (result.Status == CreateUserStatus.Success)
                {
                    return this.Ok(new { success = true, id = result.UserId });
                }

                var message = CreateUserStatusMessage.GetErrorMessage(result.Status);
                return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = new[] { message } });
            }
            return this.BadRequest(new { this.ModelState });
        }

        [HttpDelete]
        [Route("{userId}")]
        public async Task<IActionResult> Delete(Guid userId)
        {           
            var currentUserId = Guid.Parse(UserId);

            if (userId == currentUserId)
            {
                this.log.LogWarning("Cannot delete user. user try to delete himself. userId:{0}", userId);
                return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = new[] { "You cannot delete yourself" } });
            }

            var status = await this.userService.DeleteById(userId);
            if (status != UserDeleteStatus.None)
            {
                this.log.LogWarning("Cannot delete user. userId:{0}", userId);
                var message = UserDeleteStatusMessage.GetErrorMessage(status);
                return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = new[] { message } });
            }
            return this.Ok();
        }

        [HttpPost]
        [Route("{userId}/unlock")]
        public IActionResult UnLockPost(Guid userId)
        {
            var response = this.userService.UnLock(new UserUnLockRequest { UserId = userId });
            if (response.Status != UserUnLockStatus.Success)
            {
                this.log.LogWarning("Cannot unlock user. userId:{0}", userId);
                var message = UserUnLockStatusMessage.GetErrorMessage(response.Status);
                return this.StatusCode(StatusCodes.Status500InternalServerError, new FailedResponse() { Errors = new[] { message } });
            }
            return this.Ok();
        }
    }
}
