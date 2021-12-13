using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;

namespace SWI.SoftStock.ServerApps.WebApplicationServices.Mappers
{
    /// <summary>
    /// English
    /// </summary>
    public static class MachineDeleteStatusEn
    {
        public static string GetErrorMessage(MachineDeleteStatus status)
        {
            string result;
            switch (status)
            {
                case MachineDeleteStatus.NotExist:
                    result = "Cannot delete. Machine not exist";
                    break;
                default:
                    result = "Unknown error";
                    break;
            }
            return result;
        }
    }
}