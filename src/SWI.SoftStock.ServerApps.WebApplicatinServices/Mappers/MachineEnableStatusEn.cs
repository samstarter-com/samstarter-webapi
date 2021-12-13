using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;

namespace SWI.SoftStock.ServerApps.WebApplicationServices.Mappers
{
    /// <summary>
    /// English
    /// </summary>
    public static class MachineEnableStatusEn
    {
        public static string GetErrorMessage(MachineEnableStatus status)
        {
            string result;
            switch (status)
            {
                case MachineEnableStatus.NotExist:
                    result = "Cannot disable. Machine not exist";
                    break;
                default:
                    result = "Unknown error";
                    break;
            }
            return result;
        }
    }
}