namespace SWI.SoftStock.ServerApps.WebApplicationContracts.MachineService.GetByStructureUnitId
{
    using SWI.SoftStock.ServerApps.WebApplicationModel;
    using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;

    public class GetByStructureUnitIdResponse
    {
        public GetByStructureUnitIdStatus Status { get; set; }
        public MachineCollection Model { get; set; }
    }
}