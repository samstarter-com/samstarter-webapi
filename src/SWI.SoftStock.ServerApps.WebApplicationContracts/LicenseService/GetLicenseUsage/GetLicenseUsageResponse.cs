namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetLicenseUsage
{
    using System;
    using SWI.SoftStock.ServerApps.WebApplicationModel;

    public class GetLicenseUsageResponse
    {
        public LicenseUsageItemCollection Model { get; set; }
        public string LicenseName { get; set; }
        public Guid LicenseId { get; set; }
        public GetLicenseUsageStatus Status { get; set; }
        public UsageFilterModel Filter { get; set; }
    }
}