using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.ObservableService.GetAll
{
    public class GetAllResponse
    {
        public GetAllStatus Status { get; set; }
        public ObservableExCollection Model { get; set; }
    }
}