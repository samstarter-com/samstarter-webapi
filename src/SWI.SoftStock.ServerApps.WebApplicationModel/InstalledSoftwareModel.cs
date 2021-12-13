using System;
using System.Collections.Generic;
using SWI.SoftStock.ServerApps.WebApplicationModel.Attributes;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    [Sortable]
	public class InstalledSoftwareModel : BaseModel<InstalledSoftwareModel>, ISoftwareModel
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

		/// <summary>
		/// Количество инсталляций
		/// </summary>
		[Sort("thTotalInstallationCount")]
		public int TotalInstallationCount { get; set; }

		/// <summary>
		/// Количество лицензированных инсталляций
		/// </summary>
		[Sort("thLicensedInstallationCount")]
		public int LicensedInstallationCount { get; set; }

		/// <summary>
		/// Количество не лицензированных инсталляций
		/// </summary>
		[Sort("thUnLicensedInstallationCount")]
		public int UnLicensedInstallationCount { get; set; }

		public IEnumerable<ObservableModel> ObservableProcesses { get; set; }

        [Sort("thInstallDate")]
        public string InstallDate { get; set; }

        [Sort("thDiscoveryDate")]
        public DateTime DiscoveryDate { get; set; }

        /// <summary>
        /// Лицензирован ли софт, используется в отображение на странице софта на конкретной машине
        /// </summary>
        [Sort("thHasLicense")]
        public bool HasLicense { get; set; }

        [Sort("thLicenseName")]
        public string LicenseName { get; set; }
       
        public Guid? LicenseId { get; set; }
       
    }
}
