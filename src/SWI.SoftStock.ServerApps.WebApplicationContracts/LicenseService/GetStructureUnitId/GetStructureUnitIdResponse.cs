using System;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetStructureUnitId
{
	public class GetStructureUnitIdResponse
	{
		public GetStructureUnitIdStatus Status { get; set; }
		public Guid StructureUnitId { get; set; }
	}
}