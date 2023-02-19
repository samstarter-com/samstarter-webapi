namespace SWI.SoftStock.ServerApps.WebApplicationContracts
{
    using SWI.SoftStock.ServerApps.WebApplicationContracts.FeedbackService.Add;
    using System.Threading.Tasks;

    public interface IFeedbackService
    {
        Task<FeedbackAddResponse> Add(FeedbackAddRequest request);
    }
}
