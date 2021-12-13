using System;
using System.ComponentModel;
using SWI.SoftStock.ServerApps.WebApplicationModel.Attributes;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
	[Sortable]
	public class MachineLicenseModel : BaseModel<MachineLicenseModel>, ILicenseModel
	{
		[ReadOnly(true)]
		[Sort("thStatus")]
		public string Status { get; set; }

		public LicensedMachineFilterType LicensedMachineFilterType { get; set; }

		public Guid LicenseId { get; set; }

		[Sort("thName")]
		public string Name { get; set; }

		[Sort("thLicenseType")]
		public string LicenseTypeName { get; set; }

		[Sort("thLicenseCount")]
		public int Count { get; set; }

		[Sort("thAvailableLicenseCount")]
		public int AvailableCount { get; set; }

		[Sort("thStartDate")]
		public DateTime BeginDate { get; set; }

		[Sort("thExpirationDate")]
		public DateTime ExpirationDate { get; set; }

		public string Comments { get; set; }

		public SoftwareModel[] LinkedSoftwares { get; set; }
		public DocumentModel[] Documents { get; set; }

		public AlertModel[] Alerts { get; set; }

		[Sort("thStructureUnit")]
		public string StructureUnitName { get; set; }

		public Guid StructureUnitId { get; set; }
	}
}