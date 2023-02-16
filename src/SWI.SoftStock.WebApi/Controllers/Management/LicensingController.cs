using Microsoft.Extensions.Logging;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.LicenseLicense;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.LicenseMachine;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.LicenseMachines;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.UnLicenseLicenses;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.UnLicenseMachine;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.UnLicenseMachines;
using SWI.SoftStock.WebApplications.Main.Mappers;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWI.SoftStock.ServerApps.WebApplicationServices.Mappers;
using SWI.SoftStock.WebApi.Common;

namespace SWI.SoftStock.WebApi.Controllers.Management
{
    [ApiController]
    [Authorize(Policy = Constants.PolicyManager)]
    [Route("api/management/licensing")]
    public class LicensingController : AuthorizedBaseController
    {
        private readonly ILogger<LicensingController> log;
        private readonly IStructureUnitService structureUnitService;
        private readonly ILicensingService licensingService;

        public LicensingController(ILogger<LicensingController> log,
            IStructureUnitService structureUnitService,
            ILicensingService licensingService)
        {
            this.log = log;
            this.structureUnitService = structureUnitService;
            this.licensingService = licensingService;
        }


        [HttpPost]
        [Route("{licenseId}/licensemachine/{machineId}")]
        public async Task<IActionResult> LicenseMachine(Guid licenseId, Guid machineId)
        {
            var request = new LicenseMachineRequest
            {
                MachineId = machineId,
                LicenseId = licenseId
            };
            var response = await this.licensingService.LicenseMachine(request);

            if (response.Status != LicenseMachineStatus.Success)
            {
                var message = LicenseMachineStatusEn.GetErrorMessage(response.Status);
                this.log.LogWarning(
                    "Cannot link software on machine to license. LicenseMachineStatus:{0}. error message:{1}. machineId:{2} licenseId:{3}",
                    response.Status, message, machineId, licenseId);
                return this.Ok(new { success = false, errors = new[] { message } });
            }

            return this.Ok(new { success = true });
        }

        [HttpPost]
        [Route("{licenseId}/unlicensemachine/{machineId}")]
        public async Task<IActionResult> UnLicenseMachine(Guid licenseId, Guid machineId)
        {
            var request = new UnLicenseMachineRequest
            {
                LicenseId = licenseId,
                MachineId = machineId
            };
            var response = await this.licensingService.UnLicenseMachine(request);

            if (response.Status != UnLicenseMachineStatus.Success)
            {
                var message = UnLicenseMachineStatusEn.GetErrorMessage(response.Status);
                this.log.LogWarning(
                    "Cannot unlicense software on machinine. UnLicenseMachineStatus:{0}. error message:{1}. machineId:{2} licenseId:{3}",
                    response.Status, message, machineId, licenseId);
                return this.Ok(new { success = false, errors = new[] { message } });
            }

            return this.Ok(new { success = true });
        }


        [HttpPost]
        [Route("{licenseId}/licensemachines")]
        public async Task<IActionResult> LicenseMachines(Guid licenseId)
        {           
            var suGuids = await this.structureUnitService.GetStructureUnitsGuid(Guid.Parse(UserId), new[] { "Manager" });

            var request = new LicenseMachinesRequest
            {
                LicenseId = licenseId,
                SuIds = suGuids
            };
            var response = await this.licensingService.LicenseMachinesAsync(request);

            if (response.Status != LicenseMachinesStatus.Success)
            {
                var message = LicenseMachinesStatusStatusEn.GetErrorMessage(response.Status);
                this.log.LogWarning(
                    "Cannot link license to all machines . LinkAllMachineToLicenseStatus:{0}. error message:{1}. licenseId:{2}",
                    response.Status, message, licenseId);
                return this.Ok(new { success = false, errors = new[] { message } });
            }

            return this.Ok(new { success = true });
        }

        [HttpPost]
        [Route("{licenseId}/unlicensemachines")]
        public async Task<IActionResult> UnLicenseMachines(Guid licenseId)
        {         
            var suGuids = await this.structureUnitService.GetStructureUnitsGuid(Guid.Parse(UserId), new[] { "Manager" });

            var request = new UnLicenseMachinesRequest
            {
                LicenseId = licenseId,
                SuIds = suGuids
            };
            var response = await this.licensingService.UnLicenseMachinesAsync(request);

            if (response.Status != UnLicenseMachinesStatus.Success)
            {
                var message = UnLicenseMachinesStatusEn.GetErrorMessage(response.Status);
                this.log.LogWarning(
                    "Cannot unlicense machines. UnLicenseMachinesStatus:{0}. error message:{1}. licenseId:{2}",
                    response.Status, message, licenseId);
                return this.Ok(new { success = false, errors = new[] { message } });
            }

            return this.Ok(new { success = true });
        }

        [HttpPost]
        [Route("{machineId}/licenselicenses")]
        public async Task<IActionResult> LicenseLicenses(Guid machineId)
        {            
            var suGuids = await this.structureUnitService.GetStructureUnitsGuid(Guid.Parse(UserId), new[] { "Manager" });

            var request = new LicenseLicenseRequest
            {
                MachineId = machineId,
                SuIds = suGuids
            };
            LicenseLicenseResponse response = await this.licensingService.LicenseLicenseAsync(request);
            if (response.Status != LicenseLicenseStatus.Success)
            {
                var message = LicenseLicenseStatusEn.GetErrorMessage(response.Status);
                this.log.LogWarning(
                    "Cannot link software on machine to licenses. LicenseLicenseStatus:{0}. error message:{1}. machineId:{2}",
                    response.Status, message, machineId);
                return this.Ok(new { success = false, errors = new[] { message } });
            }

            return this.Ok(new { success = true });
        }

        [HttpPost]
        [Route("{machineId}/unlicenselicenses")]
        public async Task<IActionResult> UnLicenseLicenses(Guid machineId)
        {            
            var suGuids = await this.structureUnitService.GetStructureUnitsGuid(Guid.Parse(UserId), new[] { "Manager" });

            var request = new UnLicenseLicensesRequest
            {
                SuIds = suGuids,
                MachineId = machineId
            };
            var response = await this.licensingService.UnLicenseLicensesAsync(request);
            if (response.Status != UnLicenseLicensesStatus.Success)
            {
                var message = UnLicenseLicensesStatusEn.GetErrorMessage(response.Status);
                this.log.LogWarning(
                    "Cannot unlicense license. UnLicenseLicensesStatus:{0}. error message:{1}. machineId:{2}",
                    response.Status, message, machineId);
                return this.Ok(new { success = false, errors = new[] { message } });
            }

            return this.Ok(new { success = true });
        }
    }
}