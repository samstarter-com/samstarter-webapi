using SWI.SoftStock.ServerApps.DataModel2;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.ValidateUser
{
    public class ValidateUserResponse
    {
        public ValidateUserStatus Status { get; set; }
        public User User { get; set; }
    }
}