namespace SWI.SoftStock.ServerApps.WebApplicationContracts
{
    using System.Collections.Generic;
    using SWI.SoftStock.ServerApps.WebApplicationModel.Common;

    public abstract class GetItemsRequest
    {
        protected GetItemsRequest()
        {
            FilterItems = new Dictionary<string, string>();
        }

        public PagingModel Paging { get; set; }
        public Dictionary<string, string> FilterItems { get; set; }
        public OrderModel Ordering { get; set; }
    }
}