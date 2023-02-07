using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;

namespace SWI.SoftStock.ServerApps.WebApplicationServices.Mappers
{

    /// <summary>
    /// English
    /// </summary>
    public static class ObservableDeleteStatusEn
    {
        public static string GetErrorMessage(ObservableDeleteStatus status)
        {
            string result = status switch
            {
                ObservableDeleteStatus.NotExist => "Cannot delete. License not exist",
                ObservableDeleteStatus.AppendedToMachine => "Cannot delete. Observable appended to machine",
                _ => "Unknown error",
            };
            return result;
        }
    }
}