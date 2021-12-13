using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;

namespace SWI.SoftStock.ServerApps.WebApplicationServices.Mappers
{
    /// <summary>
    /// English
    /// </summary>
    public static class LicenseDeleteStatusEn
    {
        public static string GetErrorMessage(LicenseDeleteStatus status)
        {
            string result;
            switch (status)
            {
                case LicenseDeleteStatus.NotExist:
                    result = "Cannot delete. License not exist";
                    break;
                case LicenseDeleteStatus.LicenseAttachedToMachine:
                    result = "Cannot delete. License attached to machine";
                    break;
                default:
                    result = "Unknown error";
                    break;
            }
            return result;
        }
    }
}