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
            string result;
            switch (status)
            {
                case LicenseMachineStatus.MachineNotFound:
                    result = "Machine not found";
                    break;
                case LicenseMachineStatus.LicenseNotFound:
                    result = "License not found";
                    break;
                case LicenseMachineStatus.LicenseCountExceeded:
                    result = "License count exceeded";
                    break;
                case LicenseMachineStatus.SoftwareOnMachineNotFound:
                    result = "Software on machine not found";
                    break;
                case LicenseMachineStatus.SoftwareIsLinked:
                    result = "Software on machine allready linked to license";
                    break;
                default:
                    result = "Unknown error";
                    break;
            }
            return result;
        }
    }
}