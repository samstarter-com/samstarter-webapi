namespace SWI.SoftStock.ServerApps.WebApplicationContracts.SoftwareService.GetByStructureUnitId
{
    using System;

    public class GetByStructureUnitIdRequest : GetItemsRequest
    {
        public Guid StructureUnitId { get; set; }

        public bool IncludeItemsOfSubUnits { get; set; }
    }
}