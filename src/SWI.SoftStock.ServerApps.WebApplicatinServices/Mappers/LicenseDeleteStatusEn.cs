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
            string result = status switch
            {
                LicenseDeleteStatus.NotExist => "Cannot delete. License not exist",
                LicenseDeleteStatus.LicenseAttachedToMachine => "Cannot delete. License attached to machine",
                _ => "Unknown error",
            };
            return result;
        }
    }
}