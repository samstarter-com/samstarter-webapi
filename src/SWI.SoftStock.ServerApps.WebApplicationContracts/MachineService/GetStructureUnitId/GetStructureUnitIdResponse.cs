using System;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.MachineService.GetStructureUnitId
{
	public class GetStructureUnitIdResponse
	{
		public GetStructureUnitIdStatus Status { get; set; }
		public Guid StructureUnitId { get; set; }
	}
}