using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.PersonalLicenseRequestService.GetNewLicenseRequestCount;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.ServerApps.WebApplicationServices;
using SWI.SoftStock.WebApi.Common;
using SWI.SoftStock.WebApi.Mapper;
using System;
using System.Threading.Tasks;

namespace SWI.SoftStock.WebApi.Controllers.Personal
{
    [ApiController]
    [Authorize(Policy = Constants.PolicyUser)]
    [Route("api/personal/licenserequests")]
    public class PersonalLicenseRequestController : AuthorizedBaseController
    {
        private readonly IPersonalLicenseRequestService personalLicenseRequestService;

        private readonly ILogger<PersonalLicenseRequestController> log;

        public PersonalLicenseRequestController(ILogger<PersonalLicenseRequestController> log, IPersonalLicenseRequestService personalLicenseRequestService)
        {
            this.log = log;
            this.personalLicenseRequestService = personalLicenseRequestService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> LicenseRequests([FromQuery] PagingModel paging, [FromQuery] OrderingModel ordering, int status)
        {           

            var statusFilter = (PersonalLicenseRequestStatus)status;

            var request = new SWI.SoftStock.ServerApps.WebApplicationContracts.PersonalLicenseRequestService.GetByUserId.GetByUserIdRequest
            {
                UserId = Guid.Parse(UserId),
                Status = statusFilter,
                Paging = MapperFromViewToModel.MapToPaging(paging),
                Ordering = MapperFromViewToModel.MapToOrdering(ordering)
            };
            var response = await this.personalLicenseRequestService.GetByUserIdAsync(request);

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
                status
            };

            return this.Ok(result);
        }

        [HttpGet]
        [Route("newcount")]
        public async Task<IActionResult> NewLicenseRequestCountAsync()
        {
            var request = new GetNewLicenseRequestCountRequest();
            request.UserId = Guid.Parse(UserId);
            var result = await this.personalLicenseRequestService.GetNewLicenseRequestCount(request);
            return this.Ok(result.Count);
        }

        [HttpPost]
        [Route("{id}/received")]
        public async Task Received(Guid id)
        {
            await this.personalLicenseRequestService.ReceivedAsync(id);
        }


        [HttpGet]
        [Route("{licenseRequestId}")]
        public async Task<IActionResult> LicenseRequest(Guid licenseRequestId)
        {
            var result = new { Details = await this.personalLicenseRequestService.GetLicenseRequestModelByIdAsync(licenseRequestId) };
            return this.Ok(result);
        }

        [HttpGet]
        [Route("{fileid}/download")]
        public async Task<FileResult> Download(Guid fileid)
        {
            var document = await this.personalLicenseRequestService.GetDocumentById(fileid);
            return this.File(document.Content, "application/octet-stream", document.Name);
        }

        [HttpPost]
        [Route("{id}/answer")]
        public async Task<IActionResult> Answer([FromBody] PersonalLicenseRequestAnswerModel model)
        {
            if (!this.ModelState.IsValid) return this.BadRequest(new { this.ModelState });

            var status = await this.personalLicenseRequestService.Answer(model);
            var message = AnswerPersonalLicenseRequestStatusMessage.GetErrorMessage(status);
            switch (status)
            {
                case AnswerPersonalLicenseRequestStatus.Success:
                    return this.Ok();
                default:
                    this.log.LogWarning(
                        "Cannot update license request. UpdateLicenseRequestStatus:{0}. error message:{1} licenseRequestId:{2}",
                        status,
                        message,
                        model.LicenseRequestId);
                    return this.BadRequest(message);
            }
        }
    }
}
