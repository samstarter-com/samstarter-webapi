using System;
using SWI.SoftStock.ServerApps.WebApplicationModel.Common;

namespace SWI.SoftStock.ServerApps.WebApplicationModel.Collections
{
	public class MachineSoftwareCollection : BaseCollection<InstalledSoftwareModel>
	{
		public MachineSoftwareCollection(OrderModel order)
			: base(order.Order, order.Sort)
		{
		}

		public string MachineName { get; set; }

		public Guid StructureUnitId { get; set; }
		public Guid? UserId { get; set; }
	}
}