using SWI.SoftStock.ServerApps.DataModel2;

namespace SWI.SoftStock.ServerApps.WebApplicationServices
{
	public class MachineSoftwareLicenseMachineSoftware
	{
		public MachineSoftware MachineSoftware { get; set; }

		public LicenseMachineSoftware LicenseMachineSoftware { get; set; }
	}

	public class LicenseSoftwareLicenseMachineSoftware
	{
		public LicenseSoftware LicenseSoftware { get; set; }

		public LicenseMachineSoftware LicenseMachineSoftware { get; set; }
	}
}