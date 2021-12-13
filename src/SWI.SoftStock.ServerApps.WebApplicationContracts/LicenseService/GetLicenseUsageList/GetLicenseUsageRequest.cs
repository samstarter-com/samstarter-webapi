using System;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetLicenseUsage;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetLicenseUsageList
{
    public class GetLicenseUsageListRequest : GetItemsRequest, IGetLicenseUsageRequest
    {
        public GetLicenseUsageListRequest(GetLicenseUsageRequest request)
        {
            // ViewType = request.ViewType;
            LicenseId = request.LicenseId;
            Range = request.Range;
            FromDate = request.FromDate;
            ToDate = request.ToDate;
        }

        // public int? ViewType { get; set; }

        public Guid LicenseId { get; set; }

        public int? Range { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public bool IncludeItemsOfSubUnits { get; set; }
    }

    public interface IGetLicenseUsageRequest
    {
        // int? ViewType { get; set; }

        Guid LicenseId { get; set; }

        int? Range { get; set; }

        DateTime? FromDate { get; set; }

        DateTime? ToDate { get; set; }
    }
}