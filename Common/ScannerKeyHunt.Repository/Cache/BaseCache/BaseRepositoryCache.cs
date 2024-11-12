using ScannerKeyHunt.Data.Context;
using ScannerKeyHunt.Data.Helpers;
using ScannerKeyHunt.Repository.Interfaces;
using ScannerKeyHunt.Repository.Interfaces.Cache;
using ScannerKeyHunt.Repository.Repository.BaseRepository;
using ScannerKeyHunt.RepositoryCache.Cache.Interfaces;
using System.Linq.Expressions;

namespace ScannerKeyHunt.Repository.Cache.BaseCache
{
    public class BaseRepositoryCache<TEntity> : IBaseRepositoryCache<TEntity> where TEntity : class
    {
        private IBaseRepository<TEntity> _baseRepository;
        private readonly ICacheService _cacheService;
        private string BaseKey = typeof(TEntity).Name;
        private string EntityName = typeof(TEntity).GetType().GUID.ToString();
        private double CacheDurationMinutes = ConfigHelper.CacheDurationMinutes;

        public BaseRepositoryCache(BaseContext context, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _baseRepository = new BaseRepository<TEntity>(context);
        }

        public string GenerateKey(string key)
        {
            return string.Format("{0}_{1}_{2}", BaseKey, EntityName, key);
        }

        public void Add(TEntity entity)
        {
            _baseRepository.Add(entity);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            _baseRepository.AddRange(entities);
        }

        public List<TEntity> GetAll()
        {
            List<TEntity> entities = _baseRepository.GetAll();

            if (ConfigHelper.UseCache)
                CacheList(entities);

            return entities;
        }

        public List<TEntity> GetAll(Predicate<TEntity> predicate)
        {
            List<TEntity> entities = _baseRepository.GetAll(predicate);

            if (ConfigHelper.UseCache)
                CacheList(entities);

            return entities;
        }

        public TEntity GetByExpressionBool(Expression<Func<TEntity, bool>> predicate)
        {
            return _baseRepository.GetByExpressionBool(predicate);
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

        public List<TEntity> GetAllDeleted()
        {
            return _baseRepository.GetAllDeleted();
        }

        public List<TEntity> GetAll(Guid userId)
        {
            return _baseRepository.GetAll(userId);
        }

        public bool Exists(Guid guid, bool useCache = true)
        {
            Func<bool> function = () => _baseRepository.Exists(guid);

            if (useCache && ConfigHelper.UseCache)
            {
                string key = GenerateKey(guid.ToString());

                return _cacheService.GetOrSet(key, function, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return function();
        }

        public bool Exists(Predicate<TEntity> predicate, bool useCache = true)
        {
            return _baseRepository.Exists(predicate);
        }

        public TEntity GetByGuid(Guid guid, bool useCache = true)
        {
            Func<TEntity> function = () => _baseRepository.GetByGuid(guid);

            if (useCache && ConfigHelper.UseCache)
            {
                string key = GenerateKey(guid.ToString());

                return _cacheService.GetOrSet(key, function, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return function();
        }

        private void CacheList(List<TEntity> entities)
        {
            foreach (TEntity entity in entities)
            {
                string key = GenerateKey(entity.GetType().GetProperty("Id").GetValue(entity).ToString());

                if (!_cacheService.Exists<bool>(key))
                    _cacheService.GetOrSet(key, () => entity, TimeSpan.FromMinutes(CacheDurationMinutes));
            }
        }

        public void Update(TEntity entity)
        {
            if (ConfigHelper.UseCache)
                ResetCache(entity);

            _baseRepository.Update(entity);
        }

        public void Update(Guid guid, TEntity entity)
        {
            if (ConfigHelper.UseCache)
                ResetCache(entity);

            _baseRepository.Update(guid, entity);
        }

        public void Delete(Guid guid)
        {
            _baseRepository.Delete(guid);
        }

        public void Delete(TEntity entity)
        {
            if (ConfigHelper.UseCache)
                ResetCache(entity);

            _baseRepository.Delete(entity);
        }

        public void Delete(Guid guid, TEntity entity)
        {
            if (ConfigHelper.UseCache)
                ResetCache(entity);

            _baseRepository.Delete(guid, entity);
        }

        public void Remove(TEntity entity)
        {
            if (ConfigHelper.UseCache)
                ResetCache(entity);

            _baseRepository.Remove(entity);
        }

        public void ResetCache(TEntity entity)
        {
            Guid guid = (Guid)entity.GetType().GetProperty("Id").GetValue(entity);

            ResetByGuid<TEntity>(guid);
        }

        private void ResetByGuid<T>(Guid guid)
        {
            _cacheService.Remove<T>(GenerateKey(guid.ToString()));
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
