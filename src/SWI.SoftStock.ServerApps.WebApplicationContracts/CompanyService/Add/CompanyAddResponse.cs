using System;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.CompanyService.Add
{
    public class CompanyAddResponse
    {
        public Guid CompanyUniqueId { get; set; }
        public int CompanyId { get; set; }
        public CompanyCreationStatus Status { get; set; }
    }
}
