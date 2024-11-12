using Microsoft.Extensions.Caching.Memory;
using ScannerKeyHunt.Data.Entities;
using ScannerKeyHunt.Repository.Interfaces;
using System.Collections;
using System.Reflection;

namespace ScannerKeyHunt.Repository.Cache.BaseCache
{
    public class BaseCache : IBaseCache
    {
        private readonly IMemoryCache _memoryCache;

        public BaseCache(IMemoryCache cache)
        {
            _memoryCache = cache;
        }

        public T GetOrSet<T>(string key, Func<T> createItem, double cacheDurationMinutes)
        {
            if (_memoryCache.TryGetValue(key, out T cachedItem))
                return cachedItem;

            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheDurationMinutes)
            };

            var newItem = createItem();
            _memoryCache.Set(key, newItem, cacheEntryOptions);

            return newItem;
        }

        public bool KeyExists(string key)
        {
            return _memoryCache.TryGetValue(key, out _);
        }

        private void ResetDataCache(string key)
        {
            if (KeyExists(key))
                _memoryCache.Remove(key);
        }

        public void ResetCache(string key)
        {
            try
            {
                ICollection entriesCollection = GetMemoryCacheEntriesCollection();

                if (entriesCollection != null)
                {
                    List<string> data = entriesCollection
                        .Cast<object>()
                    .Select(item => item.GetType().GetProperty("Key").GetValue(item).ToString())
                    .Where(item => item.Contains(key) || ContainsUserWithId(item, entriesCollection, key))
                        .ToList();

                    data.ForEach(item => ResetDataCache(item));
                }
            }
            catch (Exception)
            {
                // Lida com exceções
            }
        }

        private ICollection GetMemoryCacheEntriesCollection()
        {
            return typeof(MemoryCache)
                .GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(_memoryCache) as ICollection;
        }

        private bool ContainsUserWithId(string key, ICollection entriesCollection, string value)
        {
            var user = entriesCollection
                .Cast<object>()
                .FirstOrDefault(item => item.GetType().GetProperty("Key").GetValue(item).ToString() == key);

            if (user != null)
            {
                var valuedata = user.GetType().GetProperty("Value").GetValue(user);
                var userValue = valuedata.GetType().GetProperty("Value").GetValue(valuedata);

                if (userValue is User)
                {
                    User datauservalue = (User)userValue;
                    return datauservalue.Id.Contains(value);
                }
            }

            return false;
        }
    }
}
