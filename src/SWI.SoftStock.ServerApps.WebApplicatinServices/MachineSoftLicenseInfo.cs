using SWI.SoftStock.ServerApps.DataModel2;

namespace SWI.SoftStock.ServerApps.WebApplicationServices
{
    using System;

    internal class MachineSoftLicenseInfo
    {
        public MachineSoftware MachineSoft { get; set; }

        public Software Software { get; set; }
        public Publisher Publisher { get; set; }
        public bool HasLicense { get; set; }
        public string LicenseName { get; set; }
        public Guid? LicenseId { get; set; }
    }
}