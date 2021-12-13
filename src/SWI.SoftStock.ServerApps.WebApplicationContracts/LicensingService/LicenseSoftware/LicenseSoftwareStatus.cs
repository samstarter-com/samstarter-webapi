namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.LicenseSoftware
{
    public enum LicenseSoftwareStatus
    {
        Success = 0,

        SoftwareOnMachineNotFound = 1,

        SoftwareNotFound = 2,

        MachineNotFound = 3,

        SoftwareIsLinked = 4,

        LicenseNotFound = 5,

        LicenseNotForSoftware = 6,

        LicenseCountExceeded = 7
    }
}