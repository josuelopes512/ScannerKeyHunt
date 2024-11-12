using ScannerKeyHunt.Data.Entities;

namespace ScannerKeyHunt.Repository.Interfaces.Repository
{
    public interface IUserStoreRepository : IDisposable
    {
        void SetEmailAndUserName(User user, string email, string username);
    }
}
