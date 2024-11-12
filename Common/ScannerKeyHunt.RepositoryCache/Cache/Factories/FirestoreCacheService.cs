using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using ScannerKeyHunt.RepositoryCache.Cache.Interfaces;

namespace ScannerKeyHunt.RepositoryCache.Cache.Factories
{
    public class FirestoreCacheService : ICacheService
    {
        private readonly IConfiguration _configuration;
        private static FirestoreDb _firestoreDb;

        public FirestoreCacheService()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", _configuration["GoogleFirestore:CredentialsPath"]);

            _firestoreDb = FirestoreDb.Create(_configuration["GoogleFirestore:ProjectId"].ToString());
        }

        public bool Exists<T>(string key)
        {
            var document = _firestoreDb.Collection(typeof(T).Name).Document(key).GetSnapshotAsync().GetAwaiter().GetResult();
            return document.Exists && !IsExpired(document);
        }

        public string GenerateKey<T>(string key)
        {
            return $"{typeof(T).Name}_{key}";
        }

        public dynamic Get<T>(string key)
        {
            var document = _firestoreDb.Collection(typeof(T).Name).Document(key).GetSnapshotAsync().GetAwaiter().GetResult();

            if (document.Exists && !IsExpired(document))
            {
                return document.GetValue<T>("value");
            }

            return default(T);
        }

        public T GetOrSet<T>(string key, Func<T> createItem, TimeSpan expiration)
        {
            if (Exists<T>(key))
            {
                return Get<T>(key);
            }

            var item = createItem();

            Set(key, item, expiration);

            return item;
        }

        public void Remove<T>(string key)
        {
            var docRef = _firestoreDb.Collection(typeof(T).Name).Document(key);
            docRef.DeleteAsync().GetAwaiter().GetResult();
        }

        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            var docRef = _firestoreDb.Collection(typeof(T).Name).Document(key);

            var data = new Dictionary<string, object>()
            {
                { "value", value },
                { "expiration", Timestamp.FromDateTime(DateTime.UtcNow.Add(expiration)) }
            };

            docRef.SetAsync(data).GetAwaiter().GetResult();
        }

        private bool IsExpired(DocumentSnapshot document)
        {
            var expirationTime = document.GetValue<Timestamp>("expiration").ToDateTime();

            return DateTime.UtcNow > expirationTime;
        }

        public void SetList<T>(string key, List<T> value, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public List<T>? GetList<T>(string key)
        {
            throw new NotImplementedException();
        }

        public List<T> GetOrSetList<T>(string key, Func<List<T>> createList, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public void RemoveList<T>(string key)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
