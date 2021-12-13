namespace SWI.SoftStock.ServerApps.WebApplicationContracts.FeedbackService.Add
{
    using SWI.SoftStock.ServerApps.WebApplicationModel;

    public class  FeedbackAddRequest
    {
        public FeedbackModel Feedback { get; set; }

        public string UserIp { get; set; }
    }
}