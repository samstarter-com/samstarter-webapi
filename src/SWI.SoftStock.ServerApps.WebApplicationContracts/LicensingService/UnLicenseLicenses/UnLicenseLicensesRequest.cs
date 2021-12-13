using System;
using System.Collections.Generic;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.UnLicenseLicenses
{
	public class UnLicenseLicensesRequest
	{
		public Guid MachineId { get; set; }
		public IEnumerable<Guid> SuIds { get; set; }
	}
}