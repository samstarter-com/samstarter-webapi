namespace SWI.SoftStock.ServerApps.WebApplicationContracts.MachineService.GetByObservableId
{
    using SWI.SoftStock.ServerApps.WebApplicationModel;
    using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;

    public class GetByObservableIdResponse
    {
        public GetByObservableIdStatus Status { get; set; }
        public MachineCollection Model { get; set; }
    }
}