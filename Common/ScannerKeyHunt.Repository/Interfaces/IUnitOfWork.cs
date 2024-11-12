using Microsoft.EntityFrameworkCore.Storage;
using ScannerKeyHunt.Data.Context;
using ScannerKeyHunt.Repository.Interfaces.Cache;
using ScannerKeyHunt.Repository.Interfaces.Repository;

namespace ScannerKeyHunt.Repository.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserIdentityRepository UserIdentityRepository { get; }
        IUserStoreRepository UserStoreRepository { get; }
        IUserRepositoryCache UserRepository { get; }
        ITokenAuthRepositoryCache TokenAuthRepository { get; }
        BaseContext GetBaseContext();
        void Save();
        void Commit();
        void Dispose();
        void Rollback();
        IDbContextTransaction BeginTransaction();
        void ExecuteInTransaction(Action<BaseContext> action);
        void ExecuteSqlCommand(string query, params object[] parameters);
    }
}
