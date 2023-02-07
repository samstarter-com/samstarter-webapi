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
            string result = status switch
            {
                StructureUnitUpdateStatus.NotExist => "Structure unit not found",
                StructureUnitUpdateStatus.ParentStructureUnitIsSame => "Wrong parent unit",
                StructureUnitUpdateStatus.NonUnique => "Structure unit with name {0} exists in your company. Structure unit must be unique.",
                _ => "Unknown error",
            };
            return result;
        }
    }
}