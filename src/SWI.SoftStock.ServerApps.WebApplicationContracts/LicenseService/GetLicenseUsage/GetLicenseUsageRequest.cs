using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetLicenseUsageList;
using System.Globalization;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetLicenseUsage
{
    using System;

    public class GetLicenseUsageRequest: IGetLicenseUsageRequest
    {
        // public int? ViewType { get; set; }

        public Guid LicenseId { get; set; }

        public int? Range { get; set; }
      
        public string From { get; set; }

        public DateTime? FromDate
        {
            get
            {
                if (DateTime.TryParseExact(From,
                                "dd-MM-yyyy",
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out var fromDt))
                {
                    return fromDt;
                }

                return null;

            }
            set => From = value?.ToString("dd-MM-yyyy");
        }

        public string To { get; set; }

        public DateTime? ToDate
        {
            get
            {
                if (DateTime.TryParseExact(To,
                    "dd-MM-yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var toDt))
                {
                    return toDt;
                }

                return null;

            }
            set => To = value?.ToString("dd-MM-yyyy");
        }

        public int? ViewType { get; set; }
    }
}