namespace SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.CreateUser
{
    using System;
    using SWI.SoftStock.ServerApps.WebApplicationModel;

    public class CreateUserRequest
    {
        public UserModelWithPasswordEx UserModel { get; set; }

        public Guid StructureUnitId { get; set; }
    }
}
