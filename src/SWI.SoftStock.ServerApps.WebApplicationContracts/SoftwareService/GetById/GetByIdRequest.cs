using System.Collections.Generic;
using System;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.SoftwareService.GetById
{
    public class GetByIdRequest
    {
        public Guid Id { get; set; }

        public Guid? StructureUnitId { get; set; }

        public bool IncludeItemsOfSubUnits { get; set; }

        public Guid CompanyId { get; set; }

        public IEnumerable<Guid> UserStructureUnitIds { get; set; }
    }
}