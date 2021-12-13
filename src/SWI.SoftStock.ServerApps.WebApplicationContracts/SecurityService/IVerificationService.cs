using SWI.SoftStock.ServerApps.DataModel2;
using System.Threading.Tasks;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService
{
    public interface IVerificationService
    {
        Task<User[]> GetUsersForVerificationAsync();

        Task SetUserVerificationStatusAsync(User existingUser, SendStatus sending, byte? sendCount = null);

        SendStatus GetUserVerificationStatus(User existingUser);
    }
}