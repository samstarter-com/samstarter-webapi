using System;
using SWI.SoftStock.ServerApps.WebApplicationModel.Common;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.PersonalMachineService.GetByUserId
{
    public class GetByUserIdRequest
    {
        public OrderModel Ordering { get; set; }
        public PagingModel Paging { get; set; }

        public Guid UserId { get; set; }
    }
}
