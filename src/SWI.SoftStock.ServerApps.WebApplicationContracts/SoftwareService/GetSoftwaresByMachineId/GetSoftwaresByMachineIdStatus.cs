namespace SWI.SoftStock.ServerApps.WebApplicationContracts.SoftwareService.GetSoftwaresByMachineId
{
    public enum GetSoftwaresByMachineIdStatus
    {
        Success = 0,

        MachineNotFound = 1,

        MachineIsDisabled = 2,

        UnknownError = 100
    }
}