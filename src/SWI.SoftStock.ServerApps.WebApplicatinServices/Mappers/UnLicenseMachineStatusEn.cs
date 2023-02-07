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
            string result = status switch
            {
                UnLicenseMachineStatus.MachineNotFound => "Machine not found",
                UnLicenseMachineStatus.LicenseNotFound => "License not found",
                UnLicenseMachineStatus.SoftwareIsNotLinked => "Software on machine not linked to license",
                _ => "Unknown error",
            };
            return result;
        }
    }
}