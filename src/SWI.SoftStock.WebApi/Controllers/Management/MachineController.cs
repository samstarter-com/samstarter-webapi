using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.GetAvailableLicensesByMachineId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.MachineService.GetByStructureUnitId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SoftwareService.GetSoftwaresByMachineId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;
using SWI.SoftStock.ServerApps.WebApplicationServices;
using SWI.SoftStock.ServerApps.WebApplicationServices.Mappers;
using SWI.SoftStock.WebApi.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SWI.SoftStock.WebApi.Controllers.Management
{
    [ApiController]
    [Authorize(Policy = Constants.PolicyManager)]
    [Route("api/management/machines")]
    public class MachineController : AuthorizedBaseController
    {
        private readonly ILogger<MachineController> log;
        private readonly IMachineService machineService;
        private readonly ISoftwareService softwareService;
        private readonly IStructureUnitService structureUnitService;
        private readonly ILicensingService licensingService;

        public MachineController(
            ILogger<MachineController> log,
            IMachineService machineService,
            ISoftwareService softwareService,
            IStructureUnitService structureUnitService,
            ILicensingService licensingService)
        {
            this.log = log;
            this.machineService = machineService;
            this.softwareService = softwareService;
            this.structureUnitService = structureUnitService;
            this.licensingService = licensingService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Machines(
            [FromQuery] string cid,
            [FromQuery] PagingModel paging,
            [FromQuery] OrderingModel ordering,
            [FromQuery] int machineType,
            [FromQuery] int? includeSubItems = 0)
        {
            GetByStructureUnitIdResponse response;
            var machineFilterType = (MachineFilterType)machineType;
            var isGuid = Guid.TryParse(cid, out var cidGuid);

            if (isGuid)
            {
                var request = new GetByStructureUnitIdRequest
                {
                    StructureUnitId = cidGuid,
                    MachineType = machineFilterType,
                    IncludeItemsOfSubUnits = includeSubItems == 1,
                    Paging = MapperFromViewToModel.MapToPaging(paging),
                    Ordering = MapperFromViewToModel.MapToOrdering(ordering)
                };

                response = await this.machineService.GetByStructureUnitIdAsync(request);
            }
            else
            {
                response = new GetByStructureUnitIdResponse
                {
                    Model = new MachineCollection(ordering.Order, ordering.Sort)
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
                machineType
            };
            return this.Ok(result);
        }

        [HttpGet]
        [Route("report")]
        public async Task<IActionResult> GetReport(
            [FromQuery] string cid,
            [FromQuery] OrderingModel ordering,
            [FromQuery] int machineType,
            [FromQuery] int includeSubItems = 0)
        {
            GetByStructureUnitIdResponse response;
            var machineFilterType = (MachineFilterType)machineType;
            var isGuid = Guid.TryParse(cid, out var cidGuid);

            if (isGuid)
            {
                var request = new GetByStructureUnitIdRequest
                {
                    StructureUnitId = cidGuid,
                    MachineType = machineFilterType,
                    IncludeItemsOfSubUnits = includeSubItems == 1,
                    Paging = new ServerApps.WebApplicationModel.Common.PagingModel
                    {
                        PageIndex = 0,
                        PageSize = int.MaxValue
                    },
                    Ordering = MapperFromViewToModel.MapToOrdering(ordering)
                };

                response = await this.machineService.GetByStructureUnitIdAsync(request);
            }
            else
            {
                response = new GetByStructureUnitIdResponse
                {
                    Model = new MachineCollection(ordering.Order, ordering.Sort)
                };
            }

            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);

            await writer.WriteAsync(
                $"Name;Current domain user;Current domain;Linked user;Structure unit;Last activity;Enabled;Total software;Licensed software;Unlicensed software;Operation system;{Environment.NewLine}");
            foreach (var machine in response.Model.Items)
            {
                await writer.WriteAsync(
                    $"{machine.Name};{machine.DomainUserName};{machine.DomainUserDomainName};{machine.LinkedUserName};{machine.StructureUnitName};{machine.LastActivity};{machine.Enabled};{machine.TotalSoftwareCount};{machine.LicensedSoftwareCount};{machine.UnLicensedSoftwareCount};{machine.OperationSystemName};{Environment.NewLine}");
            }

            await writer.FlushAsync();
            memoryStream.Position = 0;
            return this.File(memoryStream, "text/csv", "report.csv");
        }

        [HttpGet]
        [Route("{machineId}")]
        public async Task<IActionResult> GetById(Guid machineId)
        {
            var data = new { Details = await this.machineService.GetByIdAsync(machineId) };
            return this.Ok(data);
        }

        [HttpPost]
        [Route("{machineId}/structureunit/{structureUnitId}")]
        public async Task<IActionResult> ChangeStructureUnit(Guid machineId, Guid structureUnitId)
        {
            var status = await this.machineService.LinkToStructureUnitAsync(machineId, structureUnitId);
            switch (status)
            {
                case MachineLinkToStructureUnitStatus.Success:
                    return this.Ok(new { success = true });
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [HttpPost]
        [Route("{machineId}/user/{userId}")]
        public async Task<IActionResult> ChangeUser(Guid machineId, Guid? userId)
        {          
            if (!userId.HasValue)
            {
                return this.Ok(new { success = false, errors = new[] { "Choose user" } });
            }
            var status = await this.machineService.LinkToUserAsync(machineId, userId.Value);

            switch (status)
            {
                case MachineLinkToUserStatus.Success:
                    return this.Ok(new { success = true });
                default:
                    this.log.LogWarning("LinkToUserPost. Result is null. machineId:{0} userId:{1} userId:{2}",
                        machineId,
                        userId,
                        UserId);
                    throw new ArgumentOutOfRangeException();
            }
        }

        [HttpDelete]
        [Route("{machineId}")]
        public async Task<IActionResult> Delete(Guid machineId)
        {
            var status = await this.machineService.Delete(machineId);
            if (status != MachineDeleteStatus.Success)
            {
                this.log.LogWarning("Cannot delete machine. machineId:{0}", machineId);
                var message = MachineDeleteStatusEn.GetErrorMessage(status);
                return this.Ok(new { success = false, errors = new[] { message } });

            }

            return this.Ok();
        }

        [HttpPost]
        [Route("{machineId}/disable")]
        public async Task<IActionResult> Disable(Guid machineId)
        {
            var status = await this.machineService.Disable(machineId);
            if (status != MachineDisableStatus.Success)
            {
                this.log.LogWarning("Cannot disable machine. machineId:{0}", machineId);
                return this.Ok(new { success = false, errors = new[] { MachineDisableStatusEn.GetErrorMessage(status) } });
            }

            return this.Ok(new { success = true });
        }

        [HttpPost]
        [Route("{machineId}/enable")]
        public async Task<IActionResult> Enable(Guid machineId)
        {
            var status = await this.machineService.Enable(machineId);
            if (status != MachineEnableStatus.Success)
            {
                this.log.LogWarning("Cannot enable machine. machineId:{0}", machineId);
                return this.Ok(new { success = false, errors = new[] { MachineEnableStatusEn.GetErrorMessage(status) } });
            }
            return this.Ok(new { success = true });
        }

        [HttpGet]
        [Route("softwares")]
        public async Task<IActionResult> MachinesSoftwares([FromQuery] Guid machineId, [FromQuery] PagingModel paging,
            [FromQuery] OrderingModel ordering, int filterType,
            string filterName = null, string filterPublisherName = null, string filterVersion = null,
            string filterLicenseName = null)
        {
            var request = new GetSoftwaresByMachineIdRequest
            {
                MachineId = machineId,
                Paging = MapperFromViewToModel.MapToPaging(paging),
                Ordering = MapperFromViewToModel.MapToOrdering(ordering),
                FilterType = filterType,
                FilterItems = GetSoftwareFilterItems(filterName, filterPublisherName, filterVersion,
                    filterLicenseName)
            };

            var response = await this.softwareService.GetByMachineId(request);
            var message = GetSoftwaresByMachineIdStatusEn.GetErrorMessage(response.Status);

            if (response.Status == GetSoftwaresByMachineIdStatus.Success)
            {
                return this.Ok(
                    new
                    {
                        items = response.Model.Items,
                        machineId,
                        machineName = response.Model.MachineName,
                        structureUnitId = response.Model.StructureUnitId,
                        totalRecords = response.Model.TotalRecords,
                        pageIndex = paging.PageIndex,
                        pageSize = paging.PageSize,
                        filterType,
                        filterName,
                        filterPublisherName,
                        filterVersion,
                        filterLicenseName,
                        sortData = response.Model.SortModels,
                        sortedTableHeader = response.Model.SortedTableHeader,
                        sortedProperty = ordering.Sort,
                        order = ordering.Order,
                        userId = response.Model.UserId
                    });
            }

            this.log.LogWarning("Cannot get software. GetSoftwaresByMachineIdStatus:{0}. error message:{1}. machineId:{2}",
                response.Status, message, machineId);
            return this.Ok(new { success = false, errors = new[] { message } });
        }

        [HttpGet]
        [Route("{machineId}/licenses")]
        public async Task<IActionResult> MachineLicenses(Guid machineId, [FromQuery] PagingModel paging, [FromQuery] OrderingModel ordering,
            int status)
        {          
            var suGuids = await this.structureUnitService.GetStructureUnitsGuid(Guid.Parse(UserId), new[] { "Manager" });

            var request = new GetAvailableLicensesByMachineIdRequest();
            request.SuIds = suGuids;
            request.MachineId = machineId;
            request.Paging = MapperFromViewToModel.MapToPaging(paging);
            request.Ordering = MapperFromViewToModel.MapToOrdering(ordering);
            request.LicensedMachineFilterType = (LicensedMachineFilterType)status;
            var response = await this.licensingService.GetAvailableLicensesByMachineIdAsync(request);
            if (response.Status == GetAvailableLicensesByMachineIdStatus.Success)
            {
                return this.Ok(
                    new
                    {
                        items = response.Model.Items,
                        totalRecords = response.Model.TotalRecords,
                        machineId,
                        machineName = response.Model.MachineName,
                        status,
                        pageIndex = paging.PageIndex,
                        pageSize = paging.PageSize,
                        sortData = response.Model.SortModels,
                        sortedTableHeader = response.Model.SortedTableHeader,
                        sortedProperty = ordering.Sort,
                        order = ordering.Order
                    });
            }

            this.log.LogWarning("Cannot get licenses. GetAvailableLicensesByMachineIdStatus:{0}. machineId:{1}",
                response.Status, machineId);
            return this.Ok(new { success = false, errors = new[] { "Cannot get licenses" } });
        }

        private static Dictionary<string, string> GetSoftwareFilterItems(string filterName, string filterPublisherName,
            string filterVersion, string filterLicenseName)
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
            if (!string.IsNullOrEmpty(filterLicenseName))
            {
                result.Add("LicenseName", filterLicenseName.Trim());
            }
            return result;
        }
    }
}