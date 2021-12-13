using System;
using SWI.SoftStock.ServerApps.WebApplicationModel.Common;

namespace SWI.SoftStock.ServerApps.WebApplicationModel.Collections
{
	public class MachineLicenseCollection : BaseCollection<MachineLicenseModel>
	{
		public MachineLicenseCollection(OrderModel order):base(order.Order, order.Sort)
		{
			
		}
		public string MachineName { get; set; }

		public Guid? UserId { get; set; }
	}
}