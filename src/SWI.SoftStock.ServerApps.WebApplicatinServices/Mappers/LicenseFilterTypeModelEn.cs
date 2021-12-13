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
            string result;
            switch (licenseFilterType)
            {
                case (Int32) (LicenseFilterType.Licensed | LicenseFilterType.Unlicensed):
                    result = "All";
                    break;
                case (Int32) LicenseFilterType.Licensed:
                    result = "Licensed";
                    break;
                case (Int32) LicenseFilterType.Unlicensed:
                    result = "Unlicensed";
                    break;
                default:
                    result = "Unknown filter";
                    break;
            }
            return result;
        }
    }
}