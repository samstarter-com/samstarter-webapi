using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.UnLicenseMachine;

namespace SWI.SoftStock.ServerApps.WebApplicationServices.Mappers
{
    /// <summary>
    ///     English
    /// </summary>
    public static class UnLicenseMachineStatusEn
    {
        public static string GetErrorMessage(UnLicenseMachineStatus status)
        {
            string result;
            switch (status)
            {
                case UnLicenseMachineStatus.MachineNotFound:
                    result = "Machine not found";
                    break;
                case UnLicenseMachineStatus.LicenseNotFound:
                    result = "License not found";
                    break;
                case UnLicenseMachineStatus.SoftwareIsNotLinked:
                    result = "Software on machine not linked to license";
                    break;
                default:
                    result = "Unknown error";
                    break;
            }
            return result;
        }
    }
}