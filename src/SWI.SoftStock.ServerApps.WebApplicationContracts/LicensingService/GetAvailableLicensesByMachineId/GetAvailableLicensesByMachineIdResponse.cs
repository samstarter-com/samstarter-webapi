using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.GetAvailableLicensesByMachineId
{
	public class GetAvailableLicensesByMachineIdResponse
	{
		public GetAvailableLicensesByMachineIdStatus Status { get; set; }
		public MachineLicenseCollection Model { get; set; }
	}
}