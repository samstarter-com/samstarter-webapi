namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseRequestService.GetNewLicenseRequest
{
    using SWI.SoftStock.ServerApps.WebApplicationModel;

    public class NewLicenseRequestResponse
    {
        public NewLicenseRequestStatus Status { get; set; }
        public NewLicenseRequestModel Model { get; set; }
    }
}