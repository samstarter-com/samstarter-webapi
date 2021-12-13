using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SWI.SoftStock.ServerApps.DataModel2;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService
{
    public class VerificationService : IVerificationService
    {
        private readonly IQueryableUserStore<User> userStore;
        protected virtual CancellationToken CancellationToken => CancellationToken.None;
        public VerificationService(IUserStore<User> userStore)
        {
            this.userStore = userStore as IQueryableUserStore<User>;
        }

        /// <inheritdoc />
        public async Task<User[]> GetUsersForVerificationAsync()
        {
            return await userStore.Users.Include(u => u.Company)
                .Where(u => u.SendStatus == SendStatus.None || u.SendStatus == SendStatus.Error).ToArrayAsync();
        }

        public async Task SetUserVerificationStatusAsync(User existingUser, SendStatus sending, byte? sendCount = default)
        {
            var user = userStore.Users.SingleOrDefault(u => u.Id == existingUser.Id);
            if (user != null)
            {
                user.SendStatus = sending;
                if (sendCount.HasValue)
                {
                    user.SendCount = sendCount.Value;
                }
                await userStore.UpdateAsync(user, CancellationToken);
            }
        }

        public SendStatus GetUserVerificationStatus(User existingUser)
        {
            var user = userStore.Users.SingleOrDefault(u => u.Id == existingUser.Id);
            return user?.SendStatus ?? SendStatus.None;
        }
    }
}