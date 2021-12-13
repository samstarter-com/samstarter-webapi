using System;
using SWI.SoftStock.ServerApps.WebApplicationModel.Attributes;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
	[Sortable]
	public class ShortLicenseModel : BaseModel<ShortLicenseModel>
	{
		public Guid LicenseId { get; set; }

		[Sort("thName")]
		public string Name { get; set; }
	}
}