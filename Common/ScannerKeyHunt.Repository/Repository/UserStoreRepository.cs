using Microsoft.AspNetCore.Identity;
using ScannerKeyHunt.Data.Entities;
using ScannerKeyHunt.Repository.Interfaces.Repository;

namespace ScannerKeyHunt.Repository.Repository
{
    public class UserStoreRepository : IUserStoreRepository
    {
        private readonly IUserStore<User> _userStore;
        private readonly UserManager<User> _userManager;
        private readonly IUserEmailStore<User> _emailStore;

        public UserStoreRepository(
            IUserStore<User> userStore,
            UserManager<User> userManager
        )
        {
            _userStore = userStore;
            _userManager = userManager;
            _emailStore = GetEmailStore();
        }

        public void SetEmailAndUserName(User user, string email, string username)
        {
            _userStore.SetUserNameAsync(user, username, CancellationToken.None);
            _emailStore.SetEmailAsync(user, email, CancellationToken.None);
        }

        public void Dispose()
        {
            _userStore.Dispose();
            _emailStore.Dispose();
            GC.SuppressFinalize(this);
        }

        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<User>)_userStore;
        }
    }
}
