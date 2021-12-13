namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseRequestService.GetByStructureUnitId
{
    using System;
    using SWI.SoftStock.ServerApps.WebApplicationModel;

    public class GetByStructureUnitIdRequest : GetItemsRequest
    {
        public Guid StructureUnitId { get; set; }

        public bool IncludeItemsOfSubUnits { get; set; }

        public ManagerLicenseRequestStatus Status { get; set; }
    }
}