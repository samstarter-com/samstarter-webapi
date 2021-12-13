using System;
using System.Collections.Generic;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.LicenseLicense
{
	public class LicenseLicenseRequest
    {
        public Guid MachineId { get; set; }
		public IEnumerable<Guid> SuIds { get; set; }
    }
}