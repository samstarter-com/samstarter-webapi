namespace SWI.SoftStock.ServerApps.WebApplicationContracts.MachineService.GetStructureUnitId
{
	public enum GetStructureUnitIdStatus
	{
		Success = 0,

		MachineNotFound = 1,

		MachineIsDeleted = 2,

		UnknownError = 100
	}
}