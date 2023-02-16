using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.MachineService.GetByObservableId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.ObservableService.GetAll;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.ServerApps.WebApplicationServices;
using SWI.SoftStock.ServerApps.WebApplicationServices.Mappers;
using SWI.SoftStock.WebApi.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SWI.SoftStock.WebApi.Controllers.Management
{
    [ApiController]
    [Authorize(Policy = Constants.PolicyManager)]
    [Route("api/management/observables")]
    public class ObservableController : AuthorizedBaseController
    {
        private readonly ILogger<ObservableController> log;
        private readonly IUserService userService;
        private readonly IObservableService observableService;
        private readonly IMachineService machineService;
        public ObservableController(ILogger<ObservableController> log, IUserService userService, IObservableService observableService, IMachineService machineService)
        {
            this.log = log;
            this.userService = userService;
            this.observableService = observableService;
            this.machineService = machineService;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Observables([FromQuery] PagingModel paging, [FromQuery] OrderingModel ordering,
            [FromQuery] string prname = null, [FromQuery] Guid? fsid = null)
        {          
            var companyId = this.userService.GetCompanyId(Guid.Parse(UserId));
            var request = new GetAllRequest
            {
                CompanyId = companyId,
                Prname = prname,
                FilterSoftwareId = fsid,
                Paging = MapperFromViewToModel.MapToPaging(paging),
                Ordering = MapperFromViewToModel.MapToOrdering(ordering)
            };

            GetAllResponse response = this.observableService.GetAll(request);
            var result = new
            {
                items = response.Model.Items,
                totalRecords = response.Model.TotalRecords,
                structureUnitId = companyId,
                pageIndex = paging.PageIndex,
                pageSize = paging.PageSize,
                sortData = response.Model.SortModels,
                sortedTableHeader = response.Model.SortedTableHeader,
                sortedProperty = ordering.Sort,
                order = ordering.Order,
                prname,
                fsid
            };
            return this.Ok(result);
        }

        [HttpGet]
        [Route("{observableId}")]
        public IActionResult GetById(Guid observableId)
        {
            var data = new { Details = this.observableService.GetObservableModelById(observableId) };
            return this.Ok(data);
        }

        [HttpGet]
        [Route("{observableId}/machines")]
        public async Task<IActionResult> Machines(Guid observableId, [FromQuery] PagingModel paging, [FromQuery] OrderingModel ordering)
        {
            GetByObservableIdRequest request = new GetByObservableIdRequest();
            request.ObservableId = observableId;
            request.Paging = MapperFromViewToModel.MapToPaging(paging);
            request.Ordering = MapperFromViewToModel.MapToOrdering(ordering);
            var response = await this.machineService.GetByObservableId(request);
            var result = new
            {
                items = response.Model.Items,
                totalRecords = response.Model.TotalRecords,
                observableId,
                pageIndex = paging.PageIndex,
                pageSize = paging.PageSize,
                sortData = response.Model.SortModels,
                sortedTableHeader = response.Model.SortedTableHeader,
                sortedProperty = ordering.Sort,
                order = ordering.Order
            };
            return this.Ok(result);
        }

        [HttpPost]
        [Route("{observableId}/machine/{machineId}")]
        public async Task<IActionResult> Append(Guid observableId, Guid machineId)
        {
            var res = await this.observableService.Append(observableId, machineId);
            var status = res.Item2;
            switch (status)
            {
                case ObservableAppendStatus.Success:
                    return this.Ok(
                        new
                        {
                            success = true
                        });

                default:
                    {
                        string message = ObservableAppendStatusEn.GetErrorMessage(status);
                        this.log.LogWarning(
                        "Cannot append observable process to machine. ObservableAppendStatus:{0}. error message:{1}. observableId:{2} machineId:{3}",
                        status,
                        message,
                        observableId,
                        machineId);
                        return this.StatusCode(StatusCodes.Status500InternalServerError, new { errors = new[] { message } });
                    }
            }
        }

        [HttpPost]
        [Route("{observableId}/remove/{machineId}")]
        public async Task<IActionResult> RemovePost(Guid machineId, Guid observableId)
        {
            var status = await this.observableService.Remove(machineId, observableId);

            switch (status)
            {
                case ObservableRemoveStatus.Success:
                    return this.Ok(
                        new
                        {
                            success = true
                        });
                default:
                    string message = ObservableRemoveStatusEn.GetErrorMessage(status);
                    this.log.LogWarning(
                        "Cannot remove observable process from machine. ObservableRemoveStatus:{0}. error message:{1}. machineId:{2} observableId:{3}",
                        status,
                        message,
                        machineId,
                        observableId);
                    return this.StatusCode(StatusCodes.Status500InternalServerError, new { errors = new[] { message } });
            }
        }

        [HttpPut]
        [Route("")]
        public async Task<IActionResult> Add(ObservableModelEx modelEx)
        {
            if (!this.ModelState.IsValid)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    new FailedResponse()
                    { Errors = this.ModelState.Values.SelectMany(v => v.Errors).Select(er => er.ErrorMessage).ToArray() });
            }
         
            var userId = Guid.Parse(UserId);
            var companyId = this.userService.GetCompanyId(userId);
            var res = await this.observableService.Add(modelEx, companyId, userId);
            Guid? observableId = res.Item1;
            var status = res.Item2;
            switch (status)
            {
                case ObservableCreationStatus.Success:
                    return this.Ok(
                        new
                        {
                            success = true,
                            id = observableId
                        });
                default:
                    var message = ObservableCreationStatusEn.GetErrorMessage(status);
                    this.log.LogWarning(
                        "Cannot add observable process. ObservableCreationStatus:{0}. error message:{1}. softwareId:{2} companyId:{3}",
                        status,
                        message,
                        modelEx.SoftwareId,
                        companyId);
                    return this.StatusCode(StatusCodes.Status500InternalServerError, new { errors = new[] { message } });
            }
        }

        [HttpDelete]
        [Route("{observableId}")]
        public async Task<IActionResult> Delete(Guid observableId)
        {
            ObservableDeleteStatus status = await this.observableService.Delete(observableId);
            if (status != ObservableDeleteStatus.Success)
            {
                this.log.LogWarning("Cannot delete observable. observableId:{0}", observableId);
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    new { errors = new[] { ObservableDeleteStatusEn.GetErrorMessage(status) } });
            }

            return this.Ok(
                new
                {
                    success = true
                });
        }
    }
}