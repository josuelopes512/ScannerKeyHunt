namespace ScannerKeyHunt.Repository.Interfaces.Cache
{
    public interface IBaseDataCache<TEntity> : IBaseRepository<TEntity>, IDisposable where TEntity : class
    {
        TEntity GetByGuid(Guid guid, bool useCache = false);
    }
}
