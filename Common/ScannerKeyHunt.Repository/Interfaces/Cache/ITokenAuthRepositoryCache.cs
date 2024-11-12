using ScannerKeyHunt.Data.Entities;

namespace ScannerKeyHunt.Repository.Interfaces.Cache
{
    public interface ITokenAuthRepositoryCache : IBaseRepositoryCache<TokenAuth>, IDisposable
    {
        TokenAuth GetTokenAuthByUserId(Guid userId, bool useCache = true);
    }
}
