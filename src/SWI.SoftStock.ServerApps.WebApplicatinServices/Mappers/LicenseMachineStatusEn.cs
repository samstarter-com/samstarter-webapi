using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.LicenseMachine;

namespace SWI.SoftStock.WebApplications.Main.Mappers
{
    /// <summary>
    ///     English
    /// </summary>
    public static class LicenseMachineStatusEn
    {
        public static string GetErrorMessage(LicenseMachineStatus status)
        {
            string result = status switch
            {
                LicenseMachineStatus.MachineNotFound => "Machine not found",
                LicenseMachineStatus.LicenseNotFound => "License not found",
                LicenseMachineStatus.LicenseCountExceeded => "License count exceeded",
                LicenseMachineStatus.SoftwareOnMachineNotFound => "Software on machine not found",
                LicenseMachineStatus.SoftwareIsLinked => "Software on machine allready linked to license",
                _ => "Unknown error",
            };
            return result;
        }
    }
}