using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseRequestService.GetNewLicenseRequestCount;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using SWI.SoftStock.WebApi.Common;

namespace SWI.SoftStock.WebApi.Controllers.Management
{
    [ApiController]
    [Authorize(Roles = "User")]
    [Route("api/management/licenserequest")]
    public class ManagementLicenseRequestController : AuthorizedBaseController
    {
        private readonly ILicenseRequestService licenseRequestService;

        private readonly ILogger<ManagementLicenseRequestController> log;

        public ManagementLicenseRequestController(ILogger<ManagementLicenseRequestController> log, ILicenseRequestService licenseRequestService)
        {
            this.log = log;
            this.licenseRequestService = licenseRequestService;
        }

        [HttpGet]
        [Route("newcount")]
        public async Task<IActionResult> NewLicenseRequestCountAsync()
        {          
            var request = new GetNewLicenseRequestCountRequest { UserId = Guid.Parse(UserId) };
            var result = await this.licenseRequestService.GetNewLicenseRequestCount(request);
            return this.Ok(result.Count);
        }
    }
}
