using System;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.UnLicenseSoftware
{
	public class UnLicenseSoftwareRequest
    {
        public Guid MachineId { get; set; }
        public Guid SoftwareId { get; set; }
    }
}