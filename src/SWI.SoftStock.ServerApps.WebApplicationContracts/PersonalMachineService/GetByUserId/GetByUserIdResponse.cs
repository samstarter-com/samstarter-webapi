using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.PersonalMachineService.GetByUserId
{
    public class GetByUserIdResponse
    {
        public GetByUserIdStatus Status { get; set; }
        public PersonalMachineCollection Model { get; set; }
    }
}

