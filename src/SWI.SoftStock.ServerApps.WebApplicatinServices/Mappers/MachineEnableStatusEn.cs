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
            string result = status switch
            {
                MachineEnableStatus.NotExist => "Cannot disable. Machine not exist",
                _ => "Unknown error",
            };
            return result;
        }
    }
}