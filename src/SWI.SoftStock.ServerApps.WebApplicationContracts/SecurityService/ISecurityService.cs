using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.ChangePassword;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.CreateUser;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.GetAccount;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.RegisterCompany;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.ValidateUser;
using System.Threading.Tasks;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService
{
    public interface ISecurityService
    {       
        Task<RegisterCompanyResponse> RegisterCompanyAsync(RegisterCompanyRequest request);

        Task<ValidateUserResponse> ValidateUser(ValidateUserRequest request);

        Task<GetAccountResponse> GetAccount(GetAccountRequest request);

        Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request);

        Task<User> GetUserAsync(string username);

        Task<bool> VerifyAsync(string userId, string code);

        Task<ChangePasswordResponse> ChangePassword(string userId, string currentPassword, string newPassword);
        bool IsValidUserCredentials(string requestUserName, string requestPassword);
    }
}