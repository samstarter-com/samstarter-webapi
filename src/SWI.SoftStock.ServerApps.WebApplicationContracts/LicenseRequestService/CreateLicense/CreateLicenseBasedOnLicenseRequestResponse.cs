using System;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseRequestService.CreateLicense
{
    public class CreateLicenseBasedOnLicenseRequestResponse
    {
        public Guid? LicenseId { get; set; }
        public CreateLicenseBasedOnLicenseRequestStatus Status { get; set; }
    }
}