using System.Collections.Generic;
using System.Linq;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.WebApplicationModel;

namespace SWI.SoftStock.ServerApps.WebApplicationServices
{
	internal static class MachineSoftwaresExtension
	{
		public static LicensedMachineFilterType GetLicensedMachineFilterType(
			this IEnumerable<MachineSoftware> machineSoftwares,
			int licenseId)
		{
			return machineSoftwares
				.All(ms => ms.LicenseMachineSoftwares.Any(lms => lms.LicenseSoftware.LicenseId == licenseId && !lms.IsDeleted))
				? LicensedMachineFilterType.Licensed
				: machineSoftwares
					.All(ms => ms.LicenseMachineSoftwares.All(lms => lms.LicenseSoftware.LicenseId != licenseId || lms.IsDeleted))
					? LicensedMachineFilterType.None
					: LicensedMachineFilterType.PartialLicensed;
		}
	}
}