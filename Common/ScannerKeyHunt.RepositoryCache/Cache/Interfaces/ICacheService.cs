namespace ScannerKeyHunt.RepositoryCache.Cache.Interfaces
{
    public interface ICacheService : IDisposable
    {
        dynamic Get<T>(string key);
        void Set<T>(string key, T value, TimeSpan expiration);
        T GetOrSet<T>(string key, Func<T> createItem, TimeSpan expiration);
        void Remove<T>(string key);
        bool Exists<T>(string key);
        string GenerateKey<T>(string key);
        void SetList<T>(string key, List<T> value, TimeSpan expiration);
        List<T>? GetList<T>(string key);
        List<T> GetOrSetList<T>(string key, Func<List<T>> createList, TimeSpan expiration);
        void RemoveList<T>(string key);
    }
}
