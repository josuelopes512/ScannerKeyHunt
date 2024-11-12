using MongoDB.Bson.Serialization.Attributes;

namespace ScannerKeyHunt.RepositoryCache.Cache
{
    public class CacheItem
    {
        [BsonId]
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTime Expiration { get; set; }
    }
}
