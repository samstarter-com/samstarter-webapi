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
            string result;
            switch (status)
            {
                case MachineDisableStatus.NotExist:
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