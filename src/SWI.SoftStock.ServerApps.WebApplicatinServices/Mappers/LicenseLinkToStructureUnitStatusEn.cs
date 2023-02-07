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
            string result = status switch
            {
                LicenseLinkToStructureUnitStatus.NotExist => "Cannot link. License not exist",
                LicenseLinkToStructureUnitStatus.StructureUnitNotExist => "Cannot link. Structure unit not exist",
                _ => "Unknown error",
            };
            return result;
        }
    }
}