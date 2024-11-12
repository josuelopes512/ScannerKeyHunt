using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScannerKeyHunt.Repository.Interfaces;

namespace ScannerKeyHunt.Domain.Services
{
    public class BaseBackgroundService : IDisposable
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IServiceProvider _serviceProvider;
        protected bool _isActive = false;
        public readonly Guid _userId;
        private readonly ILogger<BaseBackgroundService> _logger;
        public CancellationToken _cancellationToken = CancellationToken.None;

        public BaseBackgroundService(IServiceProvider serviceProvider, Guid userId)
        {
            _userId = userId;
            _serviceProvider = serviceProvider;
            _unitOfWork = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();
            _logger = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ILogger<BaseBackgroundService>>();
        }

        protected void ActiveBackgroundService()
        {
            if (!_isActive)
            {
                _isActive = true;
            }
        }

        protected void DeactiveBackgroundService()
        {
            if (_isActive)
            {
                _isActive = false;
            }
        }

        protected void Start(Action action, int millisecondsTimeout = 3000)
        {
            Start();

            new Thread(() =>
            {
                while (_isActive)
                {
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        DeactiveBackgroundService();
                        break;
                    }

                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing action");
                    }
                    finally
                    {
                        Thread.Sleep(millisecondsTimeout);
                    }
                }
            }).Start();
        }

        public bool IsActive()
        {
            return _isActive;
        }

        public void Start()
        {
            UpdateCancellationToken(CancellationToken.None);
            ActiveBackgroundService();
        }

        public void Stop()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            source.Cancel();

            _cancellationToken = source.Token;

            DeactiveBackgroundService();
        }

        public void UpdateCancellationToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
