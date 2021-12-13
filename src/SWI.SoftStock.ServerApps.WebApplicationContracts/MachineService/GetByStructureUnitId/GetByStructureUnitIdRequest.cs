namespace SWI.SoftStock.ServerApps.WebApplicationContracts.MachineService.GetByStructureUnitId
{
    using System;
    using SWI.SoftStock.ServerApps.WebApplicationModel;

    public class GetByStructureUnitIdRequest : GetItemsRequest
    {
        public Guid StructureUnitId { get; set; }
        public MachineFilterType MachineType { get; set; }
        public bool IncludeItemsOfSubUnits { get; set; }
    }
}
