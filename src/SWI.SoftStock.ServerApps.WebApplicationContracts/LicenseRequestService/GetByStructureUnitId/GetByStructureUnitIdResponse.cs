namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseRequestService.GetByStructureUnitId
{
    using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;

    public class GetByStructureUnitIdResponse
    {
        public GetByStructureUnitIdStatus Status { get; set; }

        public LicenseRequestCollection Model { get; set; }
    }
}