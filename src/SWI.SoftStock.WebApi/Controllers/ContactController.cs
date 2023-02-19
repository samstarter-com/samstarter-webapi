using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.FeedbackService.Add;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using System.Threading.Tasks;

namespace SWI.SoftStock.WebApi.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/contact")]
    public class ContactController : ControllerBase
    {
        private readonly IFeedbackService feedbackService;

        public ContactController(IFeedbackService feedbackService)
        {
            this.feedbackService = feedbackService;
        }

        [HttpPost]
        [Route("feedback")]
        public async Task<IActionResult> Feedback(FeedbackModel model)
        {

            var userIp = this.HttpContext.Connection.RemoteIpAddress.ToString(); ;
            await this.feedbackService.Add(new FeedbackAddRequest { Feedback = model, UserIp = userIp });
            return this.Ok();
        }

    }
}
