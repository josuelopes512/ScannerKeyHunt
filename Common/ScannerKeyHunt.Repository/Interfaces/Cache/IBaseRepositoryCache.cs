using System.Linq.Expressions;

namespace ScannerKeyHunt.Repository.Interfaces.Cache
{
    public interface IBaseRepositoryCache<TEntity> : IDisposable where TEntity : class
    {
        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);
        List<TEntity> GetAll();
        List<TEntity> GetAll(Predicate<TEntity> predicate);
        List<TEntity> GetAllDeleted();
        TEntity GetByGuid(Guid guid, bool useCache = true);
        TEntity GetByExpressionBool(Expression<Func<TEntity, bool>> predicate);
        int GetCount();
        List<TEntity> GetPages<Tipo>(Predicate<TEntity> predicate, int numPage, int qtdRegs) where Tipo : class;
        List<TEntity> GetPages<Tipo>(int numPage, int qtdRegs) where Tipo : class;
        int GetCount<Tipo>() where Tipo : class;
        int GetCount<Tipo>(Predicate<TEntity> predicate) where Tipo : class;
        void Update(TEntity entity);
        void Update(Guid guid, TEntity entity);
        void Delete(Guid guid);
        void Delete(TEntity entity);
        void Delete(Guid guid, TEntity entity);
        void Remove(TEntity entity);
        IEnumerable<TEntity> GetWithRawSql(string query, params object[] parameters);
        bool Exists(Guid guid, bool useCache = true);
        bool Exists(Predicate<TEntity> predicate, bool useCache = true);
        void BulkUpdate(List<TEntity> entities, bool isDeletion = false);
        List<TEntity> GetAll(Guid userId);
    }
}
