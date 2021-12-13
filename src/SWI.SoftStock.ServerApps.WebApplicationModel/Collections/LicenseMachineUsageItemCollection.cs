using System;

namespace SWI.SoftStock.ServerApps.WebApplicationModel.Collections
{
	public class LicenseMachineUsageItemCollection : BaseCollection<LicenseUsageMachineModel>
    {
        public LicenseMachineUsageItemCollection(string order, string sort):base(order,sort)
        {
        
        }

        public int Range { get; set; }

        public DateTime From { get; set; }

        public DateTime To { get; set; }
    }
}