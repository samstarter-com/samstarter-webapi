using System;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.LicenseMachine
{
	public class LicenseMachineRequest
    {
        public Guid MachineId { get; set; }
        public Guid LicenseId { get; set; }
    }
}