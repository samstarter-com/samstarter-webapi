using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.GetAccount;
using SWI.SoftStock.WebApi.Common;
using System;

namespace SWI.SoftStock.WebApi.Controllers.Administration
{
    [ApiController]
    [Authorize(Policy = Constants.PolicyAdministrator)]
    [Route("api/administration")]
    public class AdministrationController : AuthorizedBaseController
    {
        private readonly ISecurityService securityService;

        public AdministrationController(ISecurityService securityService)
        {
            this.securityService = securityService;
        }

        [HttpGet]
        [SwaggerOperation(OperationId = "getSummary")]
        [Route("summary")]
        public IActionResult Summary()
        {
            var request = new GetAccountRequest {UserId = Guid.Parse(UserId)};
            var account = this.securityService.GetAccount(request);
            var data = new { Details = account.AccountModel };
            return this.Ok(data);
        }
    }
}
