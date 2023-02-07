using System;
using System.ComponentModel;
using SWI.SoftStock.ServerApps.WebApplicationModel.Attributes;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
	[Sortable]
	public class LicenseMachineModel : BaseModel<LicenseMachineModel>, IMachineModel
	{
		[ReadOnly(true)]
		[Sort("thStatus")]
		public string Status { get; set; }

		public LicensedMachineFilterType LicensedMachineFilterType { get; set; }

		public Guid? UserId { get; set; }

		public Guid MachineId { get; set; }

		public Guid? StructureUnitId { get; set; }

		[Sort("thOperationSystemName")]
		public string OperationSystemName { get; set; }

		[Sort("thStructureUnitName")]
		public string StructureUnitName { get; set; }

		[Sort("thName")]
		public string Name { get; set; }

		[Sort("thDomainUserName")]
		public string DomainUserName { get; set; }

		[Sort("thDomainUserDomainName")]
		public string DomainUserDomainName { get; set; }

		[Sort("thLinkedUserName")]
		public string LinkedUserName { get; set; }

		[Sort("thCreatedOn")]
		public DateTime CreatedOn { get; set; }

		[Sort("thLastActivity")]
		public DateTime LastActivity { get; set; }

		[Sort("thTotalSoftwareCount")]
		public int TotalSoftwareCount { get; set; }

		[Sort("thLicensedSoftwareCount")]
		public int LicensedSoftwareCount { get; set; }

		[Sort("thUnLicensedSoftwareCount")]
		public int UnLicensedSoftwareCount { get; set; }

        [Sort("thExpiredLicensedSoftwareCount")]
        public int ExpiredLicensedSoftwareCount { get; set; }

		[Sort("thEnabled")]
		public bool Enabled { get; set; }
	}
}