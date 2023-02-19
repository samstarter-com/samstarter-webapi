using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using System;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseRequestService.Add
{
    public class LicenseRequestAddResponse
    {
        public Guid? LicenseRequestId { get; set; }
        public SaveLicenseRequestStatus Status { get; set; }
    }
}
