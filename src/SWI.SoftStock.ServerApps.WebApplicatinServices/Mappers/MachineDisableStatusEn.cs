using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;

namespace SWI.SoftStock.ServerApps.WebApplicationServices.Mappers
{
    /// <summary>
    /// English
    /// </summary>
    public static class MachineDisableStatusEn
    {
        public static string GetErrorMessage(MachineDisableStatus status)
        {
            string result = status switch
            {
                MachineDisableStatus.NotExist => "Cannot disable. Machine not exist",
                _ => "Unknown error",
            };
            return result;
        }
    }
}