namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetAvailableLicensesBySoftwareId
{
    using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;

    public class GetAvailableLicensesBySoftwareIdResponse
    {
        public GetAvailableLicensesBySoftwareIdStatus Status { get; set; }
        public ShortLicenseCollection Model { get; set; }
    }
}