using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;

namespace SWI.SoftStock.ServerApps.WebApplicationServices.Mappers
{
    /// <summary>
    ///     English
    /// </summary>
    public static class ObservableAppendStatusEn
    {
        public static string GetErrorMessage(ObservableAppendStatus status)
        {
            string result = status switch
            {
                ObservableAppendStatus.ObservableNotFound => "Observable process not found",
                ObservableAppendStatus.MachineNotFound => "Machine not found",
                ObservableAppendStatus.SoftwareNotInstalledOnMachine => "Software not installed on machine",
                ObservableAppendStatus.AlreadyAppended => "Software on machine is already observed",
                _ => "Unknown error",
            };
            return result;
        }
    }
}