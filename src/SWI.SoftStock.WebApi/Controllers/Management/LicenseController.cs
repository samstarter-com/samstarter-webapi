using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseRequestService.CreateLicense;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetByStructureUnitId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetLicenseUsage;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetLicenseUsageList;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.GetLicensedMachine;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;
using SWI.SoftStock.ServerApps.WebApplicationServices;
using SWI.SoftStock.ServerApps.WebApplicationServices.Mappers;
using SWI.SoftStock.WebApi.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SWI.SoftStock.WebApi.Controllers.Management
{
    [ApiController]
    [Authorize(Policy = Constants.PolicyManager)]
    [Route("api/management/licenses")]
    public class LicenseController : AuthorizedBaseController
    {
        private readonly ILogger<LicenseController> log;
        private readonly ILicenseService licenseService;
        private readonly IStructureUnitService structureUnitService;
        private readonly ILicensingService licensingService;
        private readonly ILicenseRequestService licenseRequestService;

        public LicenseController(
            ILogger<LicenseController> log,
            ILicenseService licenseService,
            IStructureUnitService structureUnitService,
            ILicensingService licensingService,
            ILicenseRequestService licenseRequestService)
        {
            this.log = log;
            this.licenseService = licenseService;
            this.structureUnitService = structureUnitService;
            this.licensingService = licensingService;
            this.licenseRequestService = licenseRequestService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Licenses([FromQuery] string cid, [FromQuery] PagingModel paging, [FromQuery] OrderingModel ordering, [FromQuery] int? includeSubItems = 0)
        {
            GetByStructureUnitIdResponse response;
            var isGuid = Guid.TryParse(cid, out var cidGuid);
            if (isGuid)
            {
                var request = new GetByStructureUnitIdRequest
                {
                    StructureUnitId = cidGuid,
                    IncludeItemsOfSubUnits = includeSubItems == 1,
                    Paging = MapperFromViewToModel.MapToPaging(paging),
                    Ordering = MapperFromViewToModel.MapToOrdering(ordering)
                };
                response = await this.licenseService.GetByStructureUnitIdAsync(request);
            }
            else
            {
                response = new GetByStructureUnitIdResponse
                {
                    Model = new LicenseCollection(ordering.Order, ordering.Sort)
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
                includeSubItems
            };
            return this.Ok(result);
        }


        [HttpGet]
        [Route("{licenseId}")]
        public IActionResult GetById(Guid licenseId)
        {
            var data = new { Details = this.licenseService.GetLicenseModelById(licenseId) };
            return this.Ok(data);
        }

        [HttpGet]
        [Route("detail/{licenseId}")]
        public IActionResult GetDetailById(Guid licenseId)
        {
            var data = new { Details = this.licenseService.GetLicenseModelExById(licenseId) };
            return this.Ok(data);
        }

        [HttpGet]
        [Route("licensetypes")]
        public IActionResult GetLicenseTypes()
        {
            var data = this.licenseService.GetLicenseTypes();
            return this.Ok(data);
        }

        [HttpDelete]
        [Route("{licenseId}/structureunit/{structureUnitId}")]
        public IActionResult Delete(Guid licenseId, Guid structureUnitId)
        {
            // TODO CHECKING THE RIGHTS TO ACT WITH A LICENSE
            var license = this.licenseService.GetLicenseModelExById(licenseId);

            if (license.StructureUnitId != structureUnitId)
            {
                this.log.LogWarning(
                    "Cannot delete license. licenses structureunitId not equal structureunitId. licenseId:{0} license.structureunitId:{1} structureunitId:{2}",
                    licenseId,
                    license.StructureUnitId,
                    structureUnitId);
                return this.Ok(new { success = false, errors = new[] { "Cannot delete license." } });
            }
            var status = this.licenseService.DeleteById(licenseId);

            if (status != LicenseDeleteStatus.Success)
            {
                this.log.LogWarning("Cannot delete license. licenseId:{0}", licenseId);
                var message = LicenseDeleteStatusEn.GetErrorMessage(status);
                return this.StatusCode(StatusCodes.Status500InternalServerError, new { success = false, errors = new[] { message } });
            }
            return this.Ok();
        }


        [HttpPost]
        [Route("{licenseId}/structureunit/{structureUnitId}")]
        public async Task<IActionResult> ChangeStructureUnit(Guid licenseId, Guid structureUnitId)
        {
            var status = await this.licenseService.LinkToStructureUnitAsync(licenseId, structureUnitId);
            switch (status)
            {
                case LicenseLinkToStructureUnitStatus.Success:
                    return this.Ok(new { success = true });
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [HttpGet]
        [Route("{fileid}/download")]
        public FileResult Download(Guid fileid)
        {
            var document = this.licenseService.GetDocumentById(fileid);
            return this.File(document.Content, "application/octet-stream", document.Name);
        }

        [HttpPut]
        [Route("")]
        public async Task<IActionResult> AddAsync(LicenseModelEx model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, this.ModelState);
                //return Json(new { errors = GetErrorsFromModelState() });
            }

            var licenseResponse = await this.licenseService.AddAsync(model);

            return this.Ok(new { success = true, id = licenseResponse.LicenseUniqueId });
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> UpdateAsync(LicenseModelEx model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, this.ModelState);
            }

            var licenseResponse = await this.licenseService.UpdateAsync(model);

            return this.Ok(new { success = true, id = model.LicenseId });
        }

        /// <summary>
        /// Create license by license request
        /// </summary>
        /// <param name="licenseRequestId">LicenseRequest identifier</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{licenseRequestId}")]
        public async Task<IActionResult> CreateByLicenseRequestAsync(Guid licenseRequestId)
        {
            var request = new CreateLicenseBasedOnLicenseRequestRequest() { LicenseRequestId = licenseRequestId };
            var response = await this.licenseRequestService.CreateLicenseAsync(request);

            switch (response.Status)
            {
                case CreateLicenseBasedOnLicenseRequestStatus.Success:
                    return this.Ok(new { success = true, id = response.LicenseId.Value });
                default:
                    var message = CreateLicenseBasedOnLicenseRequestStatusEn.GetErrorMessage(response.Status);
                    this.log.LogWarning(
                                "Cannot create license. CreateLicenseBasedOnLicenseRequestStatus:{0}. error message:{1} licenseRequestId:{2}",
                                response.Status,
                                message,
                                licenseRequestId);
                    return this.StatusCode(StatusCodes.Status500InternalServerError, new { success = false, errors = new[] { message } });
            }
        }

        [HttpGet]
        [Route("{licenseId}/usage")]
        public IActionResult LicenseUsage(
            Guid licenseId,
            [FromQuery] GetLicenseUsageRequest req,
            [FromQuery] PagingModel paging,
            [FromQuery] OrderingModel ordering,
            [FromQuery] int? includeSubItems = 0)
        {
            req.LicenseId = licenseId;
            var request = new GetLicenseUsageListRequest(req)
            {
                IncludeItemsOfSubUnits = includeSubItems == 1,
                Paging = MapperFromViewToModel.MapToPaging(paging),
                Ordering = MapperFromViewToModel.MapToOrdering(ordering),
                LicenseId = licenseId
            };
            var response = this.licenseService.GetLicenseUsageList(request);

            if (!req.ViewType.HasValue || req.ViewType.Value == 2)
            {
                var model = response.Model;
                return this.Ok(
                    new
                    {
                        items = model.Items,
                        licenseId,
                        licenseName = response.LicenseName,
                        totalRecords = model.TotalRecords,
                        pageIndex = paging.PageIndex,
                        pageSize = paging.PageSize,
                        sortData = model.SortModels,
                        sortedTableHeader = model.SortedTableHeader,
                        sortedProperty = ordering.Sort,
                        order = ordering.Order,
                        includeSubItems,
                        range = req.Range,
                        from = req.From,
                        to = req.To
                    });
            }

            var response1 = this.licenseService.GetLicenseUsage(req);
            var data = this.GetLicenseUsageModel(response1);
            return this.Ok(data);
        }

        [HttpGet]
        [Route("{licenseId}/machines")]
        public async Task<IActionResult> LicenseMachinesAsync(
            Guid licenseId,
            [FromQuery] PagingModel paging,
            [FromQuery] OrderingModel ordering,
            int status)
        {
           
            var suGuids = this.structureUnitService.GetStructureUnitsGuid(Guid.Parse(UserId), new[] { "Manager" });

            var request = new GetLicensedMachineRequest();
            request.Paging = MapperFromViewToModel.MapToPaging(paging);
            request.Ordering = MapperFromViewToModel.MapToOrdering(ordering);
            request.LicenseId = licenseId;
            request.SuIds = suGuids;
            request.LicensedMachineFilterType = (LicensedMachineFilterType)status;

            var response = await this.licensingService.GetLicensedMachineAsync(request);
            return this.Ok(new

            {
                items = response.Model.Items,
                licenseId,
                licenseName = response.Model.LicenseName,
                totalRecords = response.Model.TotalRecords,
                structureUnitId = Guid.Empty,
                status,
                pageIndex = paging.PageIndex,
                pageSize = paging.PageSize,
                sortData = response.Model.SortModels,
                sortedTableHeader = response.Model.SortedTableHeader,
                sortedProperty = ordering.Sort,
                order = ordering.Order
            });
        }

        [HttpGet]
        [Route("report")]
        public async Task<IActionResult> GetReport([FromQuery] string cid, [FromQuery] OrderingModel ordering, [FromQuery] int includeSubItems = 0)
        {
            LicenseCollection model = null;

            var isGuid = Guid.TryParse(cid, out var cidGuid);

            if (isGuid)
            {
                var request = new GetByStructureUnitIdRequest();
                request.StructureUnitId = cidGuid;
                request.IncludeItemsOfSubUnits = includeSubItems == 1;
                request.Paging = new ServerApps.WebApplicationModel.Common.PagingModel { PageIndex = 0, PageSize = int.MaxValue };
                request.Ordering = MapperFromViewToModel.MapToOrdering(ordering);
                var response = await this.licenseService.GetByStructureUnitIdAsync(request);
                model = response.Model;
            }
            else
            {
                model = new LicenseCollection(ordering.Order, ordering.Sort);
            }


            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);

            await writer.WriteAsync($"Name;License type;License count;Available license count;Structure unit;Start date;Expiration date;{Environment.NewLine}");
            foreach (var license in model.Items)
            {
                await writer.WriteAsync($"{license.Name};{license.LicenseTypeName};{license.Count};{license.AvailableCount};{license.StructureUnitName};{license.BeginDate};{license.ExpirationDate};{Environment.NewLine}");
            }

            await writer.FlushAsync();
            memoryStream.Position = 0;
            return this.File(memoryStream, "text/csv", "report.csv");
        }

        private object GetLicenseUsageModel(GetLicenseUsageResponse response)
        {
            return new
            {
                licenseId = response.LicenseId,
                licenseName = response.LicenseName,
                Total = response.Model.Items.Select(d => d.TotalCount).AsEnumerable(),
                Usage = response.Model.Items.Select(d => d.UsageCount).AsEnumerable(),
                Ticks = response.Model.Items.Select(d => d.TickText).AsEnumerable(),
                TotalLegendText = "Total count",
                UsageLegendText = "Used count",
                MaxTick = this.GetMaxTickl(response.Model.Items.Select(d => d.TotalCount).Max())
            };
        }

        private int GetMaxTickl(int maxCount)
        {
            if (maxCount < 10)
            {
                return 10;
            }
            if (maxCount < 100)
            {
                return 100;
            }
            if (maxCount < 1000)
            {
                return 1000;
            }

            return 10000;
        }
    }
}