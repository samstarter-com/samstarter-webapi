using System;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.StructureUnitService.GetCompanyIdByName
{
    public class GetCompanyIdByNameResponse
    {
        public int CompanyId { get; set; }
        public Guid CompanyUniqueId { get; set; }
    }
}
