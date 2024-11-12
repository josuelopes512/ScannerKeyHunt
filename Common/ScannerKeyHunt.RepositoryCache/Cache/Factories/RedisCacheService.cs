using Microsoft.Extensions.Caching.Distributed;
using ScannerKeyHunt.RepositoryCache.Cache.Interfaces;
using System.Text.Json;

namespace ScannerKeyHunt.RepositoryCache.Cache.Factories
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;

        public RedisCacheService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public dynamic? Get<T>(string key)
        {
            try
            {
                var cacheKey = GenerateKey<T>(key);

                var cachedData = _distributedCache.GetString(cacheKey);

                if (string.IsNullOrEmpty(cachedData))
                {
                    return default;
                }

                return JsonSerializer.Deserialize<T>(cachedData);
            }
            catch (Exception)
            {

            }

            return default;
        }

        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            var cacheKey = GenerateKey<T>(key);

            var serializedData = JsonSerializer.Serialize(value);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            _distributedCache.SetString(cacheKey, serializedData, options);
        }

        public T GetOrSet<T>(string key, Func<T> createItem, TimeSpan expiration)
        {
            T? data = Get<T>(key);

            if (data == null)
            {
                data = createItem();

                Set(key, data, expiration);
            }

            return data;
        }

        public void Remove<T>(string key)
        {
            var cacheKey = GenerateKey<T>(key);
            _distributedCache.Remove(cacheKey);
        }

        public bool Exists<T>(string key)
        {
            var cacheKey = GenerateKey<T>(key);
            return _distributedCache.GetString(cacheKey) != null;
        }

        public string GenerateKey<T>(string key)
        {
            return $"{typeof(T).Name}:{key}";
        }

        public void SetList<T>(string key, List<T> value, TimeSpan expiration)
        {
            var cacheKey = GenerateKey<List<T>>(key);

            var serializedData = JsonSerializer.Serialize(value);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            _distributedCache.SetString(cacheKey, serializedData, options);
        }

        public List<T>? GetList<T>(string key)
        {
            try
            {
                var cacheKey = GenerateKey<List<T>>(key);

                var cachedData = _distributedCache.GetString(cacheKey);

                if (string.IsNullOrEmpty(cachedData))
                {
                    return default;
                }

                return JsonSerializer.Deserialize<List<T>>(cachedData);
            }
            catch (Exception)
            {

            }

            return default;
        }

        public List<T> GetOrSetList<T>(string key, Func<List<T>> createList, TimeSpan expiration)
        {
            List<T>? data = GetList<T>(key);

            if (data == null)
            {
                data = createList();

                SetList(key, data, expiration);
            }

            return data;
        }

        public void RemoveList<T>(string key)
        {
            var cacheKey = GenerateKey<List<T>>(key);
            _distributedCache.Remove(cacheKey);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
