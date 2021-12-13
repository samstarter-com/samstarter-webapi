namespace SWI.SoftStock.ServerApps.WebApplicationContracts.UserService.GetByStructureUnitId
{
    using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;

    public class GetByStructureUnitIdResponse
    {
        public GetByStructureUnitIdStatus Status { get; set; }

        public UserCollection Model { get; set; }
    }
}