using System;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.StructureUnitService.GetCompanyIdByStructureUnitId
{
    public class GetCompanyIdByStructureUnitIdResponse
    {
        public int CompanyId { get; set; }
        public Guid CompanyUniqueId { get; set; }
    }
}
