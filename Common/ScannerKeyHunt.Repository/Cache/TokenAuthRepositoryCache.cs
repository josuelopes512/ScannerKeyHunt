using ScannerKeyHunt.Data.Context;
using ScannerKeyHunt.Data.Entities;
using ScannerKeyHunt.Data.Helpers;
using ScannerKeyHunt.Repository.Cache.BaseCache;
using ScannerKeyHunt.Repository.Interfaces.Cache;
using ScannerKeyHunt.Repository.Interfaces.Repository;
using ScannerKeyHunt.Repository.Repository;
using ScannerKeyHunt.RepositoryCache.Cache.Interfaces;

namespace ScannerKeyHunt.Repository.Cache
{
    public class TokenAuthRepositoryCache : BaseRepositoryCache<TokenAuth>, ITokenAuthRepositoryCache
    {
        private readonly ITokenAuthRepository _repository;
        private readonly ICacheService _cacheService;
        private double CacheDurationMinutes = ConfigHelper.CacheDurationMinutes;

        public TokenAuthRepositoryCache(BaseContext context, ICacheService cacheService) : base(context, cacheService)
        {
            _cacheService = cacheService;
            _repository = new TokenAuthRepository(context);
        }

        public void Add(TokenAuth entity)
        {
            _repository.Add(entity);
        }

        public void Update(TokenAuth entity)
        {
            try
            {
                _repository.Update(entity);
            }
            catch (Exception)
            {
                base.Update(entity);
            }
        }

        public void Delete(TokenAuth entity)
        {
            try
            {
                if (ConfigHelper.UseCache)
                    ResetCache(entity);

                _repository.Delete(entity);
            }
            catch (Exception)
            {
                base.Delete(entity);
            }
        }

        public TokenAuth GetTokenAuthByUserId(Guid userId, bool useCache = true)
        {
            Func<TokenAuth> function = () => _repository.GetTokenAuthByUserId(userId);

            if (useCache && ConfigHelper.UseCache)
            {
                string key = GenerateKey(userId.ToString());

                return _cacheService.GetOrSet(key, function, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return function();
        }
    }
}
