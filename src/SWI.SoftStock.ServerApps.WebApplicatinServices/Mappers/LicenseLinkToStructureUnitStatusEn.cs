using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;

namespace SWI.SoftStock.ServerApps.WebApplicationServices.Mappers
{
    /// <summary>
    /// English
    /// </summary>
    public static class LicenseLinkToStructureUnitStatusEn
    {
        public static string GetErrorMessage(LicenseLinkToStructureUnitStatus status)
        {
            string result;
            switch (status)
            {
                case LicenseLinkToStructureUnitStatus.NotExist:
                    result = "Cannot link. License not exist";
                    break;
                case LicenseLinkToStructureUnitStatus.StructureUnitNotExist:
                    result = "Cannot link. Structure unit not exist";
                    break;
                default:
                    result = "Unknown error";
                    break;
            }
            return result;
        }
    }
}