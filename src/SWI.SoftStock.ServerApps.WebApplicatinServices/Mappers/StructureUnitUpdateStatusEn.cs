using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;

namespace SWI.SoftStock.ServerApps.WebApplicationServices.Mappers
{
    /// <summary>
    /// English
    /// </summary>
    public static class StructureUnitUpdateStatusEn
    {
        public static string GetErrorMessage(StructureUnitUpdateStatus status)
        {
            string result;
            switch (status)
            {
                case StructureUnitUpdateStatus.NotExist:
                    result = "Structure unit not found";
                    break;
                case StructureUnitUpdateStatus.ParentStructureUnitIsSame:
                    result = "Wrong parent unit";
                    break;
                case StructureUnitUpdateStatus.NonUnique:
                    result = "Structure unit with name {0} exists in your company. Structure unit must be unique.";
                    break;
                default:
                    result = "Unknown error";
                    break;
            }
            return result;
        }
    }
}