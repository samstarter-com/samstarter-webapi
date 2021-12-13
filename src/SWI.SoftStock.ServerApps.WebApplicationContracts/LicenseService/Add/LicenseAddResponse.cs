using System;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.Add
{
    public class LicenseAddResponse
    {
        public Guid? LicenseUniqueId { get; set; }

        public LicenseCreationStatus Status { get; set; }
    }
}
