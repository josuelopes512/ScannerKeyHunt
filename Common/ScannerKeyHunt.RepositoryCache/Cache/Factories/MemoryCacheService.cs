using Microsoft.Extensions.Caching.Memory;
using ScannerKeyHunt.RepositoryCache.Cache.Interfaces;

namespace ScannerKeyHunt.RepositoryCache.Cache.Factories
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public dynamic Get<T>(string key)
        {
            var cacheKey = GenerateKey<T>(key);
            _memoryCache.TryGetValue(cacheKey, out T? value);
            return value;
        }

        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            var cacheKey = GenerateKey<T>(key);

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            _memoryCache.Set(cacheKey, value, options);
        }

        public T GetOrSet<T>(string key, Func<T> createItem, TimeSpan expiration)
        {
            var cacheKey = GenerateKey<T>(key);

            if (_memoryCache.TryGetValue(cacheKey, out T cachedItem))
                return cachedItem;

            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            var newItem = createItem();

            _memoryCache.Set(cacheKey, newItem, cacheEntryOptions);

            return newItem;
        }

        public void Remove<T>(string key)
        {
            var cacheKey = GenerateKey<T>(key);
            _memoryCache.Remove(cacheKey);
        }

        public bool Exists<T>(string key)
        {
            var cacheKey = GenerateKey<T>(key);
            return _memoryCache.TryGetValue(cacheKey, out _);
        }

        public string GenerateKey<T>(string key)
        {
            return $"{typeof(T).Name}:{key}";
        }

        public void SetList<T>(string key, List<T> value, TimeSpan expiration)
        {
            var cacheKey = GenerateKey<List<T>>(key);

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            _memoryCache.Set(cacheKey, value, options);
        }

        public List<T>? GetList<T>(string key)
        {
            var cacheKey = GenerateKey<List<T>>(key);
            _memoryCache.TryGetValue(cacheKey, out List<T>? value);
            return value;
        }

        public List<T> GetOrSetList<T>(string key, Func<List<T>> createList, TimeSpan expiration)
        {
            var cacheKey = GenerateKey<List<T>>(key);

            if (_memoryCache.TryGetValue(cacheKey, out List<T>? cachedList))
                return cachedList;

            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            var newList = createList();
            _memoryCache.Set(cacheKey, newList, cacheEntryOptions);

            return newList;
        }

        public void RemoveList<T>(string key)
        {
            var cacheKey = GenerateKey<List<T>>(key);
            _memoryCache.Remove(cacheKey);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
