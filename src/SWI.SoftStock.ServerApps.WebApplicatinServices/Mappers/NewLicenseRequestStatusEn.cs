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
            string result;
            switch (status)
            {
                case NewLicenseRequestStatus.SoftwareOnMachineNotFound:
                    result = "Software on machine not found";
                    break;
                case NewLicenseRequestStatus.SoftwareNotFound:
                    result = "Software not found";
                    break;
                case NewLicenseRequestStatus.MachineNotFound:
                    result = "Machine not found";
                    break;
                case NewLicenseRequestStatus.UserNotFound:
                    result = "Machine must be linked to user. Please, link machine to user and try again";
                    break;
                default:
                    result = "Unknown error";
                    break;
            }
            return result;
        }
    }
}