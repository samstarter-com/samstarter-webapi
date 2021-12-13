namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.UnLicenseSoftware
{
    public enum UnLicenseSoftwareStatus
    {
        Success = 0,

        SoftwareOnMachineNotFound = 1,

        SoftwareNotFound = 2,

        MachineNotFound = 3,

        SoftwareIsNotLinked = 4,
    }
}
