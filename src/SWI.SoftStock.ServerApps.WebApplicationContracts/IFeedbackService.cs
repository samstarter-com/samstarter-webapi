namespace SWI.SoftStock.ServerApps.WebApplicationContracts
{
    using SWI.SoftStock.ServerApps.WebApplicationContracts.FeedbackService.Add;

    public interface IFeedbackService
    {
        FeedbackAddResponse Add(FeedbackAddRequest request);
    }
}
