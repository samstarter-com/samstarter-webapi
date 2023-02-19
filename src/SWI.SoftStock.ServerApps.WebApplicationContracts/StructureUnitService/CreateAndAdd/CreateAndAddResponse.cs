using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using System;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.StructureUnitService.CreateAndAdd
{
    public class CreateAndAddResponse
    {
        public Guid? StructureUnitId { get; set; }
        public StructureUnitCreationStatus Status { get; set; }
    }
}
