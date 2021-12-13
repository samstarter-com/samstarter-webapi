namespace SWI.SoftStock.ServerApps.WebApplicationContracts.SoftwareService.GetSoftwaresByMachineId
{
    using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;

    public class GetSoftwaresByMachineIdResponse
    {
        public GetSoftwaresByMachineIdStatus Status { get; set; }
        public MachineSoftwareCollection Model { get; set; }
    }
}