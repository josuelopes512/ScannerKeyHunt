using ScannerKeyHunt.Repository.Interfaces;
using System.Linq.Expressions;

namespace ScannerKeyHunt.Repository.Repository.BaseRepository
{
    public abstract class AbstractBaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        public AbstractBaseRepository() { }

        public void Add(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid guid)
        {
            throw new NotImplementedException();
        }

        public void Delete(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid guid, TEntity entity)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void ExecuteSqlCommand(string query, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public List<TEntity> GetAll()
        {
            throw new NotImplementedException();
        }

        public List<TEntity> GetAll(Predicate<TEntity> predicate)
        {
            throw new NotImplementedException();
        }

        public List<TEntity> GetAllDeleted()
        {
            throw new NotImplementedException();
        }

        public TEntity GetByExpressionBool(Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public TEntity GetByGuid(Guid guid)
        {
            throw new NotImplementedException();
        }

        public int GetCount()
        {
            throw new NotImplementedException();
        }

        public List<TEntity> GetPages<Tipo>(int numPage, int qtdRegs) where Tipo : class
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TEntity> GetWithRawSql(string query, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public void InsertData(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public List<TEntity> ListEntities()
        {
            throw new NotImplementedException();
        }

        public void Remove(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public void Update(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public void Update(Guid guid, TEntity entity)
        {
            throw new NotImplementedException();
        }

        public void UpdateData(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public bool Exists(Guid guid)
        {
            throw new NotImplementedException();
        }

        public List<TEntity> GetAll(Guid userId)
        {
            throw new NotImplementedException();
        }

        public bool Exists(Predicate<TEntity> predicate)
        {
            throw new NotImplementedException();
        }

        public void BulkUpdate(List<TEntity> entities, bool isDeletion = false)
        {
            throw new NotImplementedException();
        }

        public List<TEntity> GetPages<Tipo>(Predicate<TEntity> predicate, int numPage, int qtdRegs) where Tipo : class
        {
            throw new NotImplementedException();
        }

        public int GetCount<Tipo>() where Tipo : class
        {
            throw new NotImplementedException();
        }

        public int GetCount<Tipo>(Predicate<TEntity> predicate) where Tipo : class
        {
            throw new NotImplementedException();
        }
    }
}
