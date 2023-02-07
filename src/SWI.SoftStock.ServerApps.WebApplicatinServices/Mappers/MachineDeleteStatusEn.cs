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
            string result = status switch
            {
                MachineDeleteStatus.NotExist => "Cannot delete. Machine not exist",
                _ => "Unknown error",
            };
            return result;
        }
    }
}