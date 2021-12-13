using SWI.SoftStock.ServerApps.WebApplicationModel.Attributes;
using System;
using System.Collections.Generic;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    [Sortable]
    public class SoftwareModel : BaseModel<SoftwareModel>, ISoftwareModel
    {
        public Guid SoftwareId { get; set; }

        [Sort("thName")]
        public string Name { get; set; }

        [Sort("thPublisherName")]
        public string PublisherName { get; set; }

        [Sort("thVersion")]
        public string Version { get; set; }

        [Sort("thSystemComponent")]
        public string SystemComponent { get; set; }

        [Sort("thWindowsInstaller")]
        public string WindowsInstaller { get; set; }

        [Sort("thReleaseType")]
        public string ReleaseType { get; set; }

        [Sort("thTotalInstallationCount")]
        public int TotalInstallationCount { get; set; }

        [Sort("thLicensedInstallationCount")]
        public int LicensedInstallationCount { get; set; }

        [Sort("thUnLicensedInstallationCount")]
        public int UnLicensedInstallationCount { get; set; }

        public IEnumerable<ObservableModel> ObservableProcesses { get; set; }
        public IEnumerable<LicenseModel>Licenses { get; set; }
    }
}