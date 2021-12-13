using System;
using SWI.SoftStock.ServerApps.DataAccess2;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.FeedbackService.Add;

namespace SWI.SoftStock.ServerApps.WebApplicationServices
{

    public class FeedbackService : IFeedbackService
    {
        #region IFeedbackService Members

        private readonly MainDbContextFactory dbFactory;
        public FeedbackService(MainDbContextFactory dbFactory)
        {
            this.dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public FeedbackAddResponse Add(FeedbackAddRequest request)
        {
            var dbContext = dbFactory.Create();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var feedback = MapperFromViewToModel.MapToFeedback(request.Feedback, request.UserIp);
                unitOfWork.FeedbackRepository.Add(feedback);
                unitOfWork.Save();
            }
            return new FeedbackAddResponse() {Status = FeedbackAddStatus.Success};
        }

        #endregion
    }
}