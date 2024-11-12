using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ScannerKeyHunt.Domain.Services
{
    public class WebhookProcessorService : BackgroundService
    {
        private readonly ILogger<WebhookProcessorService> _logger;
        private static readonly ConcurrentQueue<object> payloadQueue = new ConcurrentQueue<object>();

        public WebhookProcessorService(ILogger<WebhookProcessorService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                while (payloadQueue.TryDequeue(out object payload))
                {
                    try
                    {
                        _logger.LogInformation($"Processando evento: {payload}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Erro ao processar o payload: {ex.Message}");
                    }
                }

                await Task.Delay(1000, stoppingToken);
            }
        }

        public static void EnqueuePayload(object payload)
        {
            payloadQueue.Enqueue(payload);
        }
    }
}
