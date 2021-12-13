using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetByStructureUnitId
{
	public class GetByStructureUnitIdResponse
	{
		public GetByStructureUnitIdStatus Status { get; set; }
		public LicenseCollection Model { get; set; }
	}
}