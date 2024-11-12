using MongoDB.Driver;
using ScannerKeyHunt.RepositoryCache.Cache.Interfaces;
using System.Text.Json;

namespace ScannerKeyHunt.RepositoryCache.Cache.Factories
{
    public class MongoDBCacheService : ICacheService
    {
        private readonly IMongoDatabase _database;

        public MongoDBCacheService(IMongoDatabase database)
        {
            _database = database;
        }

        private void EnsureCollectionExists<T>()
        {
            var collectionName = typeof(T).Name;

            var collectionList = _database.ListCollectionNames().ToList();

            if (!collectionList.Contains(collectionName))
            {
                _database.CreateCollection(collectionName);

                var collection = _database.GetCollection<CacheItem>(collectionName);

                var indexOptions = new CreateIndexOptions
                {
                    ExpireAfter = TimeSpan.Zero
                };

                var indexKeys = Builders<CacheItem>.IndexKeys.Ascending(x => x.Expiration);
                collection.Indexes.CreateOne(new CreateIndexModel<CacheItem>(indexKeys, indexOptions));
            }
        }

        private IMongoCollection<CacheItem> GetCollection<T>()
        {
            EnsureCollectionExists<T>();
            return _database.GetCollection<CacheItem>(typeof(T).Name);
        }

        private dynamic GetValueOrDefault<T>()
        {
            if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
            {
                // Se T é um tipo valorável (primitivo) e não nullable, retornamos null
                return null;
            }

            // Para tipos nullable ou tipos de referência, apenas retornamos default(T?)
            return default(T?);
        }

        public dynamic Get<T>(string key)
        {
            var cacheKey = GenerateKey<T>(key);

            var _cacheCollection = GetCollection<T>();

            var cacheItem = _cacheCollection.Find(x => x.Key == cacheKey).FirstOrDefault();

            return cacheItem != null ? JsonSerializer.Deserialize<T>(cacheItem.Value) : GetValueOrDefault<T>();
        }

        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            var cacheKey = GenerateKey<T>(key);

            var cacheItem = new CacheItem
            {
                Key = cacheKey,
                Value = JsonSerializer.Serialize(value),
                Expiration = DateTime.UtcNow.Add(expiration)
            };

            var filter = Builders<CacheItem>.Filter.Eq(x => x.Key, cacheKey);
            var options = new ReplaceOptions { IsUpsert = true };

            var _cacheCollection = GetCollection<T>();

            _cacheCollection.ReplaceOne(filter, cacheItem, options);
        }

        public T GetOrSet<T>(string key, Func<T> createItem, TimeSpan expiration)
        {
            dynamic data = Get<T>(key);

            if (data == null)
            {
                data = createItem();

                if (data != null)
                    Set(key, data, expiration);
            }

            return data;
        }

        public void Remove<T>(string key)
        {
            var _cacheCollection = GetCollection<T>();

            var cacheKey = GenerateKey<T>(key);

            _cacheCollection.DeleteOne(x => x.Key == cacheKey);
        }

        public bool Exists<T>(string key)
        {
            var _cacheCollection = GetCollection<T>();

            var cacheKey = GenerateKey<T>(key);

            return _cacheCollection.Find(x => x.Key == cacheKey).Any();
        }

        public string GenerateKey<T>(string key)
        {
            return $"{typeof(T).Name}:{key}";
        }

        public void SetList<T>(string key, List<T> value, TimeSpan expiration)
        {
            var cacheKey = GenerateKey<List<T>>(key);

            var cacheItem = new CacheItem
            {
                Key = cacheKey,
                Value = JsonSerializer.Serialize(value),
                Expiration = DateTime.UtcNow.Add(expiration)
            };

            var filter = Builders<CacheItem>.Filter.Eq(x => x.Key, cacheKey);
            var options = new ReplaceOptions { IsUpsert = true };

            var _cacheCollection = GetCollection<List<T>>();

            _cacheCollection.ReplaceOne(filter, cacheItem, options);
        }

        public List<T>? GetList<T>(string key)
        {
            string cacheKey = GenerateKey<List<T>>(key);

            IMongoCollection<CacheItem> _cacheCollection = GetCollection<List<T>>();

            CacheItem? cacheItem = _cacheCollection.Find(x => x.Key == cacheKey).FirstOrDefault();

            return cacheItem != null ? JsonSerializer.Deserialize<List<T>>(cacheItem.Value) : GetValueOrDefault<List<T>>();
        }

        public List<T> GetOrSetList<T>(string key, Func<List<T>> createList, TimeSpan expiration)
        {
            dynamic data = GetList<T>(key);

            if (data == null)
            {
                data = createList();

                if (data != null && data.Count > 0)
                    SetList(key, data, expiration);
            }

            return data;
        }

        public void RemoveList<T>(string key)
        {
            var _cacheCollection = GetCollection<List<T>>();

            var cacheKey = GenerateKey<List<T>>(key);

            _cacheCollection.DeleteOne(x => x.Key == cacheKey);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
