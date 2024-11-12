using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ScannerKeyHunt.IoC
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("O Serviço está iniciando");

                while (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Rodando {time}", DateTime.UtcNow);

                    File.AppendAllText("c:\\logs\\log.txt", $"{Environment.NewLine} Rodando {DateTime.UtcNow}");

                    await Task.Delay(1000, stoppingToken);
                }

                _logger.LogInformation("O Serviço está parando.");
            }
            catch (TaskCanceledException)
            {
                // When the stopping token is canceled, for example, a call made from services.msc,
                // we shouldn't exit with a non-zero exit code. In other words, this is expected...
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
