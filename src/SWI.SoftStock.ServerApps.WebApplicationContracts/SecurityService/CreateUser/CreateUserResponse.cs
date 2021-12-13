using System;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.CreateUser
{
    public class CreateUserResponse
    {
        public CreateUserStatus Status { get; set; }
        public Guid UserId { get; set; }
    }
}
