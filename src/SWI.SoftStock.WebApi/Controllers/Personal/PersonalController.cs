using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.PersonalMachineService.GetByUserId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SoftwareService.GetSoftwaresByMachineId;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;
using SWI.SoftStock.ServerApps.WebApplicationServices;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SWI.SoftStock.WebApi.Controllers.Personal
{
    [ApiController]
    [Authorize]
    [Route("api/personal")]
    public class PersonalController : ControllerBase
    {
        private readonly IPersonalMachineService machineService;
        private readonly ISoftwareService softwareService;
        private readonly IUserService userService;

        public PersonalController(IUserService userService, IPersonalMachineService machineService,
            ISoftwareService softwareService)
        {
            this.machineService = machineService;
            this.softwareService = softwareService;
            this.userService = userService;
        }

        [HttpGet]
        [Route("machines")]
        public async Task<IActionResult> Machines([FromQuery] PagingModel paging, [FromQuery] OrderingModel ordering)
        {
            if (paging == null || (paging.PageIndex == 0 && paging.PageSize == 0))
            {
                paging = new PagingModel() { PageIndex = 0, PageSize = int.MaxValue };
            }


            ordering ??= new OrderingModel();

            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var o = MapperFromViewToModel.MapToOrdering(ordering);
            var request = new GetByUserIdRequest() { UserId = Guid.Parse(userId) };
            request.Ordering = o;
            request.Paging = MapperFromViewToModel.MapToPaging(paging);
            var response = await this.machineService.GetByUserIdAsync(request);
            var result = new
            {
                items = response.Model.Items,
                totalRecords = response.Model.TotalRecords,
                pageIndex = paging.PageIndex,
                pageSize = paging.PageSize,
                sortData = response.Model.SortModels,
                sortedTableHeader = response.Model.SortedTableHeader,
                sortedProperty = ordering.Sort,
                order = ordering.Order,
            };

            return this.Ok(result);
        }

        [HttpGet]
        [Route("softwares")]
        public IActionResult Softwares([FromQuery] string machineId, [FromQuery] PagingModel paging,
            [FromQuery] OrderingModel ordering, int? filterType)
        {
            if (!filterType.HasValue)
            {
                filterType = 0;
            }

            var parsed = Guid.TryParse(machineId, out var uniqueId);
            var machineuniqueId = parsed ? (Guid?)uniqueId : null;
            var o = MapperFromViewToModel.MapToOrdering(ordering);
            var model = new MachineSoftwareCollection(o);
            var result = new
            {
                items = model.Items,
                machineId,
                totalRecords = model.TotalRecords,
                pageIndex = paging.PageIndex,
                pageSize = paging.PageSize,
                sortData = model.SortModels,
                sortedTableHeader = model.SortedTableHeader,
                sortedProperty = ordering.Sort,
                order = ordering.Order,
                filterType = filterType.Value
            };
            if (machineuniqueId.HasValue)
            {
                var request = new GetSoftwaresByMachineIdRequest
                {
                    MachineId = machineuniqueId.Value,
                    Paging = MapperFromViewToModel.MapToPaging(paging),
                    FilterType = filterType.Value,
                    Ordering = o
                };

                var response = this.softwareService.GetByMachineId(request);

                result = new
                {
                    items = response.Model.Items,
                    machineId,
                    totalRecords = response.Model.TotalRecords,
                    pageIndex = paging.PageIndex,
                    pageSize = paging.PageSize,
                    sortData = response.Model.SortModels,
                    sortedTableHeader = response.Model.SortedTableHeader,
                    sortedProperty = ordering.Sort,
                    order = ordering.Order,
                    filterType = filterType.Value
                };
            }

            return this.Ok(result);
        }

        [Route("roles")]
        [HttpGet]
        public IActionResult GetRoles()
        {
            var userIdentity = (ClaimsIdentity)this.User.Identity;
            var claims = userIdentity.Claims;
            var data = claims.Where(c => c.Type == ClaimTypes.Role).Select(r => r.Value).ToArray();
            return this.Ok(data);
        }

        [AllowAnonymous]
        [Route("everyroles")]
        [HttpGet]
        public IActionResult GetEveryRoles()
        {
            var data = this.userService.GetRoles();
            return this.Ok(data);
        }
    }
}
