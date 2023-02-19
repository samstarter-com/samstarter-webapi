using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.MachineService.GetBySoftwareId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SoftwareService.GetById;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SoftwareService.GetByStructureUnitId;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;
using SWI.SoftStock.ServerApps.WebApplicationServices;
using SWI.SoftStock.WebApi.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace SWI.SoftStock.WebApi.Controllers.Management
{
    [ApiController]
    [Authorize(Policy = Constants.PolicyManager)]
    [Route("api/management/softwares")]
    public class SoftwareController : AuthorizedBaseController
    {
        private readonly ILogger<SoftwareController> log;
        private readonly ISoftwareService softwareService;
        private readonly IUserService userService;
        private readonly IStructureUnitService structureUnitService;
        private readonly IMachineService machineService;

        public SoftwareController(
            ILogger<SoftwareController> log,
            ISoftwareService softwareService,
            IUserService userService,
            IStructureUnitService structureUnitService,
            IMachineService machineService)
        {
            this.log = log;
            this.softwareService = softwareService;
            this.userService = userService;
            this.structureUnitService = structureUnitService;
            this.machineService = machineService;
        }


        [HttpGet]
        [Route("{softwareId}")]
        public async Task<IActionResult> GetById(Guid softwareId, string cid = null, int includeSubItems = 0)
        {            
            var userId = Guid.Parse(UserId);

            var cidParsed = Guid.TryParse(cid, out var uniqueId);
            var companyId = this.userService.GetCompanyId(userId);
            var userStructureUnitRoles = await this.userService.GetUserStructureUnitRolesAsync(userId);
            var userStructureUnitIds = userStructureUnitRoles
                .Where(usur => usur.RoleName.ToLower() == Constants.RoleManager.ToLower())
                .Select(usur => usur.StructureUnitId);

            var request = new GetByIdRequest
            {
                Id = softwareId,
                StructureUnitId = cidParsed ? uniqueId : null,
                IncludeItemsOfSubUnits = includeSubItems == 1,
                CompanyId = companyId,
                UserStructureUnitIds = userStructureUnitIds
            };
            var details = await this.softwareService.GetByIdAsync(request);
            var data = new { Details = details.Detail };
            return this.Ok(data);
        }


        [HttpGet]
        [Route("autocomplete")]
        public async Task<IActionResult> SoftwaresAutocomplete([FromQuery] string request, [FromQuery] string cid = null)
        {           
            var isGuid = Guid.TryParse(cid, out var cidGuid);
            if (!isGuid)
            {
                cidGuid = this.userService.GetCompanyId(Guid.Parse(UserId));
            }
            var model = await this.softwareService.GetForAutocomplete(cidGuid, request, true);
            return this.Ok(new { softwares = model.Items, totalRecords = model.TotalRecords, structureUnitId = cid });
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Softwares([FromQuery] string cid,
            [FromQuery] PagingModel paging,
            [FromQuery] OrderingModel ordering,
            [FromQuery] int? includeSubItems = 0,
            [FromQuery] string filterName = null,
            [FromQuery] string filterPublisherName = null,
            [FromQuery] string filterVersion = null)
        {
            var isGuid = Guid.TryParse(cid, out var cidGuid);
            var model = new SoftwareCollection(ordering.Order, ordering.Sort);
            var result = new
            {
                items = model.Items,
                structureUnitId = cid,
                totalRecords = model.TotalRecords,
                pageIndex = paging.PageIndex,
                pageSize = paging.PageSize,
                sortData = model.SortModels,
                sortedTableHeader = model.SortedTableHeader,
                sortedProperty = ordering.Sort,
                order = ordering.Order,
                includeSubItems,
                filterName,
                filterPublisherName,
                filterVersion
            };

            if (isGuid)
            {
                var getByStructureUnitIdRequest = new GetByStructureUnitIdRequest
                {
                    StructureUnitId = cidGuid,
                    IncludeItemsOfSubUnits = includeSubItems == 1,
                    Paging = MapperFromViewToModel.MapToPaging(paging),
                    Ordering = MapperFromViewToModel.MapToOrdering(ordering),
                    FilterItems = GetFilterItems(filterName, filterPublisherName, filterVersion)
                };

                var response = await this.softwareService.GetByStructureUnitIdAsync(getByStructureUnitIdRequest);
                return this.Ok(
                    new
                    {
                        items = response.Model.Items,
                        structureUnitId = cid,
                        totalRecords = response.Model.TotalRecords,
                        pageIndex = paging.PageIndex,
                        pageSize = paging.PageSize,
                        sortData = response.Model.SortModels,
                        sortedTableHeader = response.Model.SortedTableHeader,
                        sortedProperty = ordering.Sort,
                        order = ordering.Order,
                        includeSubItems,
                        filterName,
                        filterPublisherName,
                        filterVersion
                    });
            }

            return this.Ok(result);
        }

        [HttpGet]
        [Route("report")]
        public async Task<IActionResult> GetReport(
            [FromQuery] string cid,
            [FromQuery] OrderingModel ordering,
            int includeSubItems = 0,
            string filterName = null,
            string filterPublisherName = null,
            string filterVersion = null)
        {
            GetByStructureUnitIdResponse response;
            var isGuid = Guid.TryParse(cid, out var cidGuid);

            if (isGuid)
            {
                var getByStructureUnitIdRequest = new GetByStructureUnitIdRequest
                {
                    StructureUnitId = cidGuid,
                    IncludeItemsOfSubUnits = includeSubItems == 1,
                    Paging = new ServerApps.WebApplicationModel.Common.PagingModel { PageIndex = 0, PageSize = int.MaxValue },
                    Ordering = MapperFromViewToModel.MapToOrdering(ordering),
                    FilterItems = GetFilterItems(filterName, filterPublisherName, filterVersion)
                };

                response = await this.softwareService.GetByStructureUnitIdAsync(getByStructureUnitIdRequest);
            }
            else
            {
                response = new GetByStructureUnitIdResponse
                {
                    Model = new SoftwareCollection(ordering.Order, ordering.Sort)
                };
            }

            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);

            await writer.WriteAsync(
                $"Name;Publisher;Version;System component;Windows installer;Release type;Total installations;Licensed installations;Unlicensed installations;{Environment.NewLine}");
            foreach (var soft in response.Model.Items)
            {
                await writer.WriteAsync(
                    $"{soft.Name};{soft.PublisherName};{soft.Version};{soft.SystemComponent};{soft.WindowsInstaller};{soft.ReleaseType};{soft.TotalInstallationCount};{soft.LicensedInstallationCount};{soft.UnLicensedInstallationCount};{Environment.NewLine}");
            }

            await writer.FlushAsync();
            memoryStream.Position = 0;
            return this.File(memoryStream, "text/csv", "report.csv");
        }

        [HttpGet]
        [Route("machines")]
        public async Task<IActionResult> SoftwaresMachinesAsync([FromQuery] Guid softwareId, [FromQuery] PagingModel paging,
            [FromQuery] OrderingModel ordering, [FromQuery] int filterType)
        {          
            var userId = Guid.Parse(UserId);

            var suGuids = await this.structureUnitService.GetStructureUnitsGuid(userId, new[] { Constants.RoleManager });

            var request = new GetBySoftwareIdRequest();
            request.Paging = MapperFromViewToModel.MapToPaging(paging);
            request.Ordering = MapperFromViewToModel.MapToOrdering(ordering);
            request.SoftwareId = softwareId;
            request.SuIds = suGuids;
            request.FilterType = filterType;

            var response = await this.machineService.GetBySoftwareIdAsync(request);

            return this.Ok(
                new
                {
                    items = response.Model.Items,
                    softwareId,
                    softwareName = response.Model.SoftwareName,
                    totalRecords = response.Model.TotalRecords,
                    structureUnitId = Guid.Empty,
                    filterType,
                    pageIndex = paging.PageIndex,
                    pageSize = paging.PageSize,
                    sortData = response.Model.SortModels,
                    sortedTableHeader = response.Model.SortedTableHeader,
                    sortedProperty = ordering.Sort,
                    order = ordering.Order
                });
        }

        private static Dictionary<string, string> GetFilterItems(string filterName, string filterPublisherName,
            string filterVersion)
        {
            var result = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(filterName))
            {
                result.Add("Name", filterName.Trim());
            }
            if (!string.IsNullOrEmpty(filterPublisherName))
            {
                result.Add("PublisherName", filterPublisherName.Trim());
            }
            if (!string.IsNullOrEmpty(filterVersion))
            {
                result.Add("Version", filterVersion.Trim());
            }
            return result;
        }
    }
}