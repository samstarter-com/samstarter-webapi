using Microsoft.Extensions.Logging;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseRequestService.GetByStructureUnitId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseRequestService.GetNewLicenseRequest;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;
using SWI.SoftStock.ServerApps.WebApplicationServices;
using SWI.SoftStock.ServerApps.WebApplicationServices.Mappers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWI.SoftStock.WebApi.Common;

namespace SWI.SoftStock.WebApi.Controllers.Management
{
    [ApiController]
    [Authorize(Policy = Constants.PolicyManager)]
    [Route("api/management/licenserequests")]
    public class LicenseRequestController : AuthorizedBaseController
    {
        private readonly ILogger<LicenseRequestController> log;
        private readonly ILicenseRequestService licenseRequestService;

        public LicenseRequestController(ILogger<LicenseRequestController> log, ILicenseRequestService licenseRequestService)
        {
            this.log = log;
            this.licenseRequestService = licenseRequestService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> LicenseRequests([FromQuery] string cid, [FromQuery] PagingModel paging,
            [FromQuery] OrderingModel ordering, [FromQuery] int status,
            [FromQuery] int? includeSubItems = 0)
        {
            GetByStructureUnitIdResponse response;
            var statusFilter = (ManagerLicenseRequestStatus)status;
            var isGuid = Guid.TryParse(cid, out var cidGuid);

            if (isGuid)
            {
                var request = new GetByStructureUnitIdRequest
                {
                    StructureUnitId = cidGuid,
                    Status = statusFilter,
                    IncludeItemsOfSubUnits = includeSubItems == 1,
                    Paging = MapperFromViewToModel.MapToPaging(paging),
                    Ordering = MapperFromViewToModel.MapToOrdering(ordering)
                };
                response = await this.licenseRequestService.GetByStructureUnitId(request);
            }
            else
            {
                response = new GetByStructureUnitIdResponse
                {
                    Model = new LicenseRequestCollection(ordering.Order, ordering.Sort)
                };
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
                includeSubItems,
                status
            };
            return this.Ok(result);
        }

        [HttpGet]
        [Route("newlicense")]
        public async Task<IActionResult> GetNewLicenseRequest([FromQuery] Guid machineId, [FromQuery] Guid softwareId)
        {
            var newLicenseRequestResponse = await this.licenseRequestService.GetNewLicenseRequest(new NewLicenseRequestRequest
            {
                MachineId = machineId,
                SoftwareId = softwareId
            });

            var message = NewLicenseRequestStatusEn.GetErrorMessage(newLicenseRequestResponse.Status);
            switch (newLicenseRequestResponse.Status)
            {
                case NewLicenseRequestStatus.Success:
                    return this.Ok(
                        new { success = true, model = newLicenseRequestResponse.Model });
                default:
                    this.log.LogWarning(
                        "Cannot create license request. NewLicenseRequestStatus:{0}. error message:{1}. softwareId:{2} machineId:{3}",
                        newLicenseRequestResponse.Status,
                        message,
                        softwareId,
                        machineId);
                    return this.StatusCode(StatusCodes.Status500InternalServerError, new { errors = new[] { message } });
            }
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> SaveLicenseRequest(NewLicenseRequestModel model)
        {
            if (!this.ModelState.IsValid)
            {
                var errors = this.GetErrorsFromModelState();
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    new FailedResponse() { Errors = errors });
            }
                    
            var userId = Guid.Parse(UserId);

            var result = await this.licenseRequestService.Add(model,
                userId,
                model.Sending);
            var licenseRequestId = result.LicenseRequestId;
            var status = result.Status;
            var message = SaveLicenseRequestStatusEn.GetErrorMessage(status);
            switch (status)
            {
                case SaveLicenseRequestStatus.Success:
                    return this.Ok(
                        new { success = true, id = licenseRequestId });

                default:
                    this.log.LogWarning(
                        "Cannot add license request. SaveLicenseRequestStatus:{0}. error message:{1}. softwareId:{2} machineId:{3} userId:{4}",
                        status,
                        message,
                        model.SoftwareId,
                        model.MachineId,
                        model.UserId);
                    return this.StatusCode(StatusCodes.Status500InternalServerError,
                        new FailedResponse() { Errors = new[] { message } });
            }
        }

        [HttpGet]
        [Route("{licenseRequestId}")]
        public async Task<IActionResult> GetById(Guid licenseRequestId)
        {
            var data = new { success = true, Details = await this.licenseRequestService.GetLicenseRequestModelByIdAsync(licenseRequestId) };
            return this.Ok(data);
        }

        [HttpPost]
        [Route("archive/{licenseRequestId}")]
        public async Task<IActionResult> ArchivePost(Guid licenseRequestId)
        {
            var status = await this.licenseRequestService.Archive(licenseRequestId);
            var message = ArchiveLicenseRequestStatusEn.GetErrorMessage(status);
            switch (status)
            {
                case ArchiveLicenseRequestStatus.Success:
                    return this.Ok(
                             new
                             {
                                 success = true
                             });
                default:
                    this.log.LogWarning(
                        "Cannot archive license request. ArchiveLicenseRequestStatus:{0}. error message:{1} licenseRequestId:{2}",
                        status,
                        message,
                        licenseRequestId);
                    return this.StatusCode(StatusCodes.Status500InternalServerError, new { errors = new[] { message } });
            }
        }

        [HttpPost]
        [Route("send/{licenseRequestId}")]
        public async Task<IActionResult> SendPost(Guid licenseRequestId)
        {
            var status = await this.licenseRequestService.SendToUser(licenseRequestId);
            var message = SendLicenseRequestStatusEn.GetErrorMessage(status);
            switch (status)
            {
                case SendLicenseRequestStatus.Success:
                    return this.Ok(
                        new
                        {
                            success = true
                        });
                default:
                    this.log.LogWarning(
                        "Cannot send to user license request. SendLicenseRequestStatus:{0}. error message:{1} licenseRequestId:{2}",
                        status,
                        message,
                        licenseRequestId);
                    return this.StatusCode(StatusCodes.Status500InternalServerError, new { errors = new[] { message } });
            }
        }

        [HttpGet]
        [Route("{fileid}/download")]
        public async Task<FileResult> Download(Guid fileid)
        {
            var document = await this.licenseRequestService.GetDocumentById(fileid);
            return this.File(document.Content, "application/octet-stream", document.Name);
        }

        [HttpPost]
        [Route("received/{licenseRequestId}")]
        public async Task Received(Guid licenseRequestId)
        {
            await this.licenseRequestService.ReceivedAsync(licenseRequestId);
        }

        private string[] GetErrorsFromModelState()
        {
            return this.ModelState.Values.SelectMany(v => v.Errors).Select(er => er.ErrorMessage).ToArray();
        }
    }
}