using Microsoft.Extensions.DependencyInjection;
using ScannerKeyHunt.Data.Enums;
using ScannerKeyHunt.RepositoryCache.Cache.Factories;
using ScannerKeyHunt.RepositoryCache.Cache.Interfaces;

namespace ScannerKeyHunt.RepositoryCache.Cache
{
    public class CacheServiceFactory
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public CacheServiceFactory(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public ICacheService CreateCacheService(CacheType cacheType)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                return cacheType switch
                {
                    CacheType.MemoryCache => scope.ServiceProvider.GetRequiredService<MemoryCacheService>(),
                    CacheType.Redis => scope.ServiceProvider.GetRequiredService<RedisCacheService>(),
                    CacheType.MongoDB => scope.ServiceProvider.GetRequiredService<MongoDBCacheService>(),
                    CacheType.Firestore => scope.ServiceProvider.GetRequiredService<FirestoreCacheService>(),
                    _ => throw new ArgumentException("Invalid cache type", nameof(cacheType)),
                };
            }
        }
    }

}
