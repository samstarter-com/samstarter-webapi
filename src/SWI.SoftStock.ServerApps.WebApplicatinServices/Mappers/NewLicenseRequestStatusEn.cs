using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseRequestService.GetNewLicenseRequest;

namespace SWI.SoftStock.ServerApps.WebApplicationServices.Mappers
{
    /// <summary>
    ///     English
    /// </summary>
    public static class NewLicenseRequestStatusEn
    {
        public static string GetErrorMessage(NewLicenseRequestStatus status)
        {
            string result = status switch
            {
                NewLicenseRequestStatus.SoftwareOnMachineNotFound => "Software on machine not found",
                NewLicenseRequestStatus.SoftwareNotFound => "Software not found",
                NewLicenseRequestStatus.MachineNotFound => "Machine not found",
                NewLicenseRequestStatus.UserNotFound => "Machine must be linked to user. Please, link machine to user and try again",
                _ => "Unknown error",
            };
            return result;
        }
    }
}