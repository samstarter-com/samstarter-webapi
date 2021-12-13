namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.LicenseMachine
{
    public enum LicenseMachineStatus
    {
        Success,

        MachineNotFound,

        LicenseNotFound,

        LicenseCountExceeded,

        SoftwareOnMachineNotFound,

        SoftwareIsLinked
    }
}