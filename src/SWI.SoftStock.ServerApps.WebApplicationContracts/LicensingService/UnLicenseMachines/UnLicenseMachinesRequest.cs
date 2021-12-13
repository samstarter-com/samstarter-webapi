using System;
using System.Collections.Generic;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.UnLicenseMachines
{
	public class UnLicenseMachinesRequest
	{
		public Guid LicenseId { get; set; }
		public IEnumerable<Guid> SuIds { get; set; }
	}
}