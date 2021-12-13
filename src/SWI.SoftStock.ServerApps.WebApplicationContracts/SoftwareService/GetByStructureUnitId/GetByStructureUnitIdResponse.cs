namespace SWI.SoftStock.ServerApps.WebApplicationContracts.SoftwareService.GetByStructureUnitId
{
    using SWI.SoftStock.ServerApps.WebApplicationModel;
    using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;

    public class GetByStructureUnitIdResponse
    {
        public GetByStructureUnitIdStatus Status { get; set; }

        public SoftwareCollection Model { get; set; }
    }
}