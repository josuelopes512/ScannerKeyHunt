namespace ScannerKeyHunt.Repository.Interfaces
{
    public interface IBaseCache
    {
        T GetOrSet<T>(string key, Func<T> createItem, double cacheDurationMinutes);
        void ResetCache(string key);
        bool KeyExists(string key);
    }
}
