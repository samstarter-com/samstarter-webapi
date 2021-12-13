using System;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.LicenseSoftware
{
    public class LicenseSoftwareRequest
    {
        public Guid MachineId { get; set; }
        public Guid SoftwareId { get; set; }
        public Guid LicenseId { get; set; }
    }
}
