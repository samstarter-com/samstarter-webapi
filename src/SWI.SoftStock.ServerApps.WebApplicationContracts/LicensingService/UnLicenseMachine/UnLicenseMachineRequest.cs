using System;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.UnLicenseMachine
{
	public class UnLicenseMachineRequest
    {
        public Guid MachineId { get; set; }
        public Guid LicenseId { get; set; }
    }
}