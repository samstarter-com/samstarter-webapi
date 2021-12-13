using System;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    [Flags]
    public enum LicenseFilterType
    {
        None = 0, // 0

        Licensed = 1 << 0, //1

        Unlicensed = 1 << 1,  //2

        ExpiredLicensed = 1 << 2, //4

        All = Licensed | Unlicensed | ExpiredLicensed
    }
}