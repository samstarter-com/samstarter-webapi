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
            string result;
            switch (status)
            {
                case ObservableAppendStatus.ObservableNotFound:
                    result = "Observable process not found";
                    break;
                case ObservableAppendStatus.MachineNotFound:
                    result = "Machine not found";
                    break;
                case ObservableAppendStatus.SoftwareNotInstalledOnMachine:
                    result = "Software not installed on machine";
                    break;
                case ObservableAppendStatus.AlreadyAppended:
                    result = "Software on machine is already observed";
                    break;
                default:
                    result = "Unknown error";
                    break;
            }
            return result;
        }
    }
}