using ScannerKeyHunt.Data.Context;
using ScannerKeyHunt.Data.Entities;
using ScannerKeyHunt.Repository.Cache.BaseCache;
using ScannerKeyHunt.Repository.Interfaces.Cache;
using ScannerKeyHunt.RepositoryCache.Cache.Interfaces;

namespace ScannerKeyHunt.Repository.Cache
{
    public class SectionRepositoryCache : BaseRepositoryCache<Section>, ISectionRepositoryCache
    {
        public SectionRepositoryCache(BaseContext context, ICacheService cacheService) : base(context, cacheService)
        {
        }
    }
}
