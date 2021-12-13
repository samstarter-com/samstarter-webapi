using System;
using System.Collections.Generic;
using SWI.SoftStock.ServerApps.WebApplicationModel;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.GetAvailableLicensesByMachineId
{
	public class GetAvailableLicensesByMachineIdRequest : GetItemsRequest
	{
		public IEnumerable<Guid> SuIds { get; set; }

		public Guid MachineId { get; set; }

		public LicensedMachineFilterType LicensedMachineFilterType { get; set; }
	}
}