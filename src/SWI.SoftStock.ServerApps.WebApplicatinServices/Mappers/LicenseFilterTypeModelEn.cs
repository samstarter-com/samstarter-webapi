using System;
using SWI.SoftStock.ServerApps.WebApplicationModel;

namespace SWI.SoftStock.WebApplications.Main.Mappers
{


    /// <summary>
    ///     English
    /// </summary>
    public static class LicenseFilterTypeModelEn
    {
        public static string GetDescription(int licenseFilterType)
        {
            string result = licenseFilterType switch
            {
                (Int32)(LicenseFilterType.Licensed | LicenseFilterType.Unlicensed) => "All",
                (Int32)LicenseFilterType.Licensed => "Licensed",
                (Int32)LicenseFilterType.Unlicensed => "Unlicensed",
                _ => "Unknown filter",
            };
            return result;
        }
    }
}