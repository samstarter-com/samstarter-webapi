using System;
using System.Collections.Generic;
using SWI.SoftStock.ServerApps.WebApplicationModel;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.GetLicensedMachine
{
	public class GetLicensedMachineRequest : GetItemsRequest
    {
        public Guid LicenseId { get; set; }

        public IEnumerable<Guid> SuIds { get; set; }

        public LicensedMachineFilterType LicensedMachineFilterType { get; set; }
    }
}