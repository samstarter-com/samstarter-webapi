using System;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetLicenseUsageList
{
    public class GetLicenseUsageListResponse
    {
        public LicenseMachineUsageItemCollection Model { get; set; }
       
        public string LicenseName { get; set; }
        public Guid LicenseId { get; set; }
        public GetLicenseUsageMachineListStatus Status { get; set; }
        public UsageFilterModel Filter { get; set; }
    }
}