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
            string result;
            switch (status)
            {
                case ObservableDeleteStatus.NotExist:
                    result = "Cannot delete. License not exist";
                    break;
                case ObservableDeleteStatus.AppendedToMachine:
                    result = "Cannot delete. Observable appended to machine";
                    break;
                default:
                    result = "Unknown error";
                    break;
            }
            return result;
        }
    }
}