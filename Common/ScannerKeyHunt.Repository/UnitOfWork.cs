using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using ScannerKeyHunt.Data.Context;
using ScannerKeyHunt.Data.Helpers;
using ScannerKeyHunt.Repository.Cache;
using ScannerKeyHunt.Repository.Interfaces;
using ScannerKeyHunt.Repository.Interfaces.Cache;
using ScannerKeyHunt.Repository.Interfaces.Repository;
using ScannerKeyHunt.RepositoryCache.Cache;
using ScannerKeyHunt.RepositoryCache.Cache.Interfaces;

namespace ScannerKeyHunt.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private bool disposed = false;
        private IDbContextTransaction _transaction;

        private BaseContext _context;
        private IUserStoreRepository _userStoreRepository;
        private ICacheService _cacheService;
        private IUserIdentityRepository _userIdentityRepository;
        private IUserRepositoryCache _userRepository;
        private ITokenAuthRepositoryCache _tokenAuthRepositoryCache;
        private IBlockRepositoryCache _blockRepositoryCache;
        private IAreaRepositoryCache _areaRepositoryCache;
        private ISectionRepositoryCache _sectionRepositoryCache;
        private IPuzzleWalletCache _puzzleWalletCache;
        private readonly ILogger<UnitOfWork> _logger;

        public UnitOfWork(
            BaseContext context,
            IUserIdentityRepository userIdentityRepository,
            IUserStoreRepository userStoreRepository,
            CacheServiceFactory cacheServiceFactory,
            ILogger<UnitOfWork> logger
        )
        {
            _userIdentityRepository = userIdentityRepository;
            _userStoreRepository = userStoreRepository;
            _context = context;
            _logger = logger;
            _cacheService = cacheServiceFactory.CreateCacheService(ConfigHelper.CacheType);
        }

        public IUserIdentityRepository UserIdentityRepository => _userIdentityRepository;

        public IUserStoreRepository UserStoreRepository => _userStoreRepository;

        public IUserRepositoryCache UserRepository
        {
            get
            {
                if (_userRepository == null)
                {
                    _userRepository = new UserRepositoryCache(
                        _userIdentityRepository,
                        _userStoreRepository,
                        _cacheService
                    );
                }

                return _userRepository;
            }
        }

        public ITokenAuthRepositoryCache TokenAuthRepository
        {
            get
            {
                if (_tokenAuthRepositoryCache == null)
                {
                    _tokenAuthRepositoryCache = new TokenAuthRepositoryCache(_context, _cacheService);
                }

                return _tokenAuthRepositoryCache;
            }
        }

        public IBlockRepositoryCache BlockRepository
        {
            get
            {
                if (_blockRepositoryCache == null)
                {
                    _blockRepositoryCache = new BlockRepositoryCache(_context, _cacheService);
                }

                return _blockRepositoryCache;
            }
        }

        public IAreaRepositoryCache AreaRepository
        {
            get
            {
                if (_areaRepositoryCache == null)
                {
                    _areaRepositoryCache = new AreaRepositoryCache(_context, _cacheService);
                }

                return _areaRepositoryCache;
            }
        }

        public ISectionRepositoryCache SectionRepository
        {
            get
            {
                if (_sectionRepositoryCache == null)
                {
                    _sectionRepositoryCache = new SectionRepositoryCache(_context, _cacheService);
                }

                return _sectionRepositoryCache;
            }
        }

        public IPuzzleWalletCache PuzzleWalletCache
        {
            get
            {
                if (_puzzleWalletCache == null)
                {
                    _puzzleWalletCache = new PuzzleWalletCache(_context, _cacheService);
                }

                return _puzzleWalletCache;
            }
        }

        public IDbContextTransaction BeginTransaction()
        {
            try
            {
                _transaction = _context.Database.BeginTransaction();
                return _transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to Begin Transaction");
                throw;
            }
        }

        public BaseContext GetBaseContext()
        {
            return _context;
        }

        public void Rollback() => _transaction.Rollback();

        public void Save() => _context.SaveChanges();

        public void Commit() => _transaction.Commit();

        public void Dispose()
        {
            try
            {
                Dispose(true);

                if (_userRepository != null)
                    _userRepository.Dispose();

                if (_userIdentityRepository != null)
                    _userIdentityRepository.Dispose();

                if (_userStoreRepository != null)
                    _userStoreRepository.Dispose();

                if (_tokenAuthRepositoryCache != null)
                    _tokenAuthRepositoryCache.Dispose();

                try { _context.ChangeTracker.Clear(); } catch (Exception ex) { _logger.LogError(ex, "Error to clear change tracker"); }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to dispose");
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();

                    if (_transaction != null)
                        _transaction.Dispose();
                }
            }
            disposed = true;
        }

        public void ExecuteInTransaction(Action<BaseContext> action)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    action(_context);
                    _context.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error to Execute In Transaction");

                    transaction.Rollback();

                    throw;
                }
            }
        }

        public void ExecuteSqlCommand(string query, params object[] parameters)
        {
            _context.Database.ExecuteSqlRaw(query, parameters);
        }
    }
}
