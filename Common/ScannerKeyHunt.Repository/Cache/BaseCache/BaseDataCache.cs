using Microsoft.Extensions.Caching.Memory;
using ScannerKeyHunt.Data.Helpers;
using ScannerKeyHunt.Repository.Interfaces.Cache;
using ScannerKeyHunt.Repository.Repository.BaseRepository;
using System.Linq.Expressions;

namespace ScannerKeyHunt.Repository.Cache.BaseCache
{
    public class BaseDataCache<TEntity> : BaseCache, IBaseDataCache<TEntity> where TEntity : class
    {
        private readonly BaseRepository<TEntity> _baseRepository;
        private readonly IMemoryCache _memoryCache;
        private string BaseKey = typeof(TEntity).Name;
        private double CacheDurationMinutes = 30;

        public BaseDataCache(IMemoryCache cache, BaseRepository<TEntity> baseRepository) : base(cache)
        {
            _memoryCache = cache;
            _baseRepository = baseRepository;
        }

        private string GenerateKey(string method, string key)
        {
            return string.Format("{0}_{1}_{2}", method, BaseKey, key);
        }

        public void Add(TEntity entity)
        {
            _baseRepository.Add(entity);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            _baseRepository.AddRange(entities);
        }

        public void Delete(Guid guid)
        {
            if (ConfigHelper.UseCache)
                ResetCache(GenerateKey("GetByGuid", guid.ToString()));

            _baseRepository.Delete(guid);
        }

        public void Delete(TEntity entity)
        {
            if (ConfigHelper.UseCache)
                ResetCache(GenerateKey("GetByGuid", entity.GetType().GetProperty("Id").GetValue(entity).ToString()));

            _baseRepository.Delete(entity);
        }

        public void Delete(Guid guid, TEntity entity)
        {
            if (ConfigHelper.UseCache)
                ResetCache(GenerateKey("GetByGuid", guid.ToString()));

            _baseRepository.Delete(guid, entity);
        }

        public List<TEntity> GetAll()
        {
            return _baseRepository.GetAll();
        }

        public List<TEntity> GetAll(Predicate<TEntity> predicate)
        {
            return _baseRepository.GetAll(predicate);
        }

        public List<TEntity> GetAllDeleted()
        {
            return _baseRepository.GetAllDeleted();
        }

        public TEntity GetByExpressionBool(Expression<Func<TEntity, bool>> predicate)
        {
            return _baseRepository.GetByExpressionBool(predicate);
        }

        public TEntity GetByGuid(Guid guid)
        {
            Func<TEntity> function = () => _baseRepository.GetByGuid(guid);

            if (ConfigHelper.UseCache)
            {
                string key = GenerateKey("GetByGuid", guid.ToString());

                return GetOrSet(key, function, CacheDurationMinutes);
            }

            return function();
        }

        public TEntity GetByGuid(Guid guid, bool useCache = false)
        {
            Func<TEntity> function = () => _baseRepository.GetByGuid(guid);

            if (useCache && ConfigHelper.UseCache)
            {
                string key = GenerateKey("GetByGuid", guid.ToString());

                return GetOrSet(key, function, CacheDurationMinutes);
            }

            return function();
        }

        public int GetCount()
        {
            return _baseRepository.GetCount();
        }

        public List<TEntity> GetPages<Tipo>(int numPage, int qtdRegs) where Tipo : class
        {
            return _baseRepository.GetPages<Tipo>(numPage, qtdRegs);
        }

        public List<TEntity> GetPages<Tipo>(Predicate<TEntity> predicate, int numPage, int qtdRegs) where Tipo : class
        {
            return _baseRepository.GetPages<Tipo>(predicate, numPage, qtdRegs);
        }

        public int GetCount<Tipo>() where Tipo : class
        {
            return _baseRepository.GetCount<Tipo>();
        }

        public int GetCount<Tipo>(Predicate<TEntity> predicate) where Tipo : class
        {
            return _baseRepository.GetCount<Tipo>(predicate);
        }

        public IEnumerable<TEntity> GetWithRawSql(string query, params object[] parameters)
        {
            return _baseRepository.GetWithRawSql(query, parameters);
        }

        public List<TEntity> ListEntities()
        {
            return _baseRepository.ListEntities();
        }

        public void Remove(TEntity entity)
        {
            if (ConfigHelper.UseCache)
                ResetCache(GenerateKey("GetByGuid", entity.GetType().GetProperty("Id").GetValue(entity).ToString()));

            _baseRepository.Remove(entity);
        }

        public void Update(TEntity entity)
        {
            if (ConfigHelper.UseCache)
                ResetCache(GenerateKey("GetByGuid", entity.GetType().GetProperty("Id").GetValue(entity).ToString()));

            _baseRepository.Update(entity);
        }

        public void Update(Guid guid, TEntity entity)
        {
            if (ConfigHelper.UseCache)
                ResetCache(GenerateKey("GetByGuid", guid.ToString()));

            _baseRepository.Update(guid, entity);
        }

        public bool Exists(Guid guid)
        {
            return _baseRepository.Exists(guid);
        }

        public List<TEntity> GetAll(Guid userId)
        {
            return _baseRepository.GetAll(userId);
        }

        public void UpdateData(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public void ExecuteSqlCommand(string query, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public void InsertData(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public bool Exists(Predicate<TEntity> predicate)
        {
            return _baseRepository.Exists(predicate);
        }

        public void BulkUpdate(List<TEntity> entities, bool isDeletion = false)
        {
            _baseRepository.BulkUpdate(entities, isDeletion);
        }

        public void Dispose()
        {
            _baseRepository.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
