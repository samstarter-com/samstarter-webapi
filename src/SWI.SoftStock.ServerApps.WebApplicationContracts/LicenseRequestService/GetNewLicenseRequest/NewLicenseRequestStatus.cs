namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseRequestService.GetNewLicenseRequest
{
    public enum NewLicenseRequestStatus
    {
        Success = 0,

        SoftwareOnMachineNotFound = 1,

        SoftwareNotFound = 2,

        MachineNotFound = 3,

        UserNotFound = 4
    }
}