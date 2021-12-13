namespace SWI.SoftStock.ServerApps.WebApplicationContracts.UserService.GetByStructureUnitId
{
    using System;

    public class GetByStructureUnitIdRequest : GetItemsRequest
    {
        public Guid StructureUnitId { get; set; }

        public bool IncludeItemsOfSubUnits { get; set; }
    }
}