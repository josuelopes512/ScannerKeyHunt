using Microsoft.EntityFrameworkCore.Storage;

namespace ScannerKeyHunt.Application.Interfaces
{
    public interface IBaseServiceApp<TEntity, TModelDTO> : IDisposable where TModelDTO : class where TEntity : class
    {
        public List<TModelDTO> GetAll();
        public TModelDTO GetById(long id);
        public TModelDTO GetByGuid(Guid guid);
        public Guid? Add(TModelDTO modelDTO, object transaction = null);
        public List<Guid> AddRange(List<TModelDTO> modelDTOList, object transaction = null);
        public bool Update(long id, TModelDTO modelDTO, object transaction = null);
        public bool Update(Guid guid, TModelDTO modelDTO, object transaction = null);
        public bool UpdateDelete(long id, TModelDTO modelDTO, object transaction = null);
        public bool UpdateDelete(Guid guid, TModelDTO modelDTO, object transaction = null);
        public bool Delete(long id, object transaction = null);
        public bool Delete(Guid guid, object transaction = null);
        public bool Delete(long id, TModelDTO modelDTO, object transaction = null);
        public bool Delete(Guid guid, TModelDTO modelDTO, object transaction = null);
        public IDbContextTransaction BeginTransaction();
        public void Commit();
        public void Rollback();
        public void Dispose();
    }
}
