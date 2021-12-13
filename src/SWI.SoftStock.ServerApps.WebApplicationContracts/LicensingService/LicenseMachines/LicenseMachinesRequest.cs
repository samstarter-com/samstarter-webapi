using System;
using System.Collections.Generic;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.LicenseMachines
{
	public class LicenseMachinesRequest
	{
		public Guid LicenseId { get; set; }
		public IEnumerable<Guid> SuIds { get; set; }
	}
}