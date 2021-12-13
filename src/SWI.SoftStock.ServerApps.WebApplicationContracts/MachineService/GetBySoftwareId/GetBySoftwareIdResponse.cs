using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.MachineService.GetBySoftwareId
{
	public class GetBySoftwareIdResponse
	{
		public GetBySoftwareIdStatus Status { get; set; }
		public SoftwareMachineCollection Model { get; set; }
	}
}