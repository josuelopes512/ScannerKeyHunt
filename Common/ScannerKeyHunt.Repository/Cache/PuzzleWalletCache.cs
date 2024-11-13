using ScannerKeyHunt.Data.Context;
using ScannerKeyHunt.Data.Entities;
using ScannerKeyHunt.Repository.Cache.BaseCache;
using ScannerKeyHunt.Repository.Interfaces.Cache;
using ScannerKeyHunt.RepositoryCache.Cache.Interfaces;

namespace ScannerKeyHunt.Repository.Cache
{
    public class PuzzleWalletCache : BaseRepositoryCache<PuzzleWallet>, IPuzzleWalletCache
    {
        public PuzzleWalletCache(BaseContext context, ICacheService cacheService) : base(context, cacheService)
        {
        }
    }
}
