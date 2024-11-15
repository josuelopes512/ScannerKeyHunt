using Microsoft.Extensions.Logging;
using ScannerKeyHunt.Data.Entities;
using ScannerKeyHunt.Domain.Interfaces;

namespace ScannerKeyHunt.Domain.Services
{
    public class SectionService : ISectionService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SectionService> _logger;

        public SectionService(IServiceProvider serviceProvider, ILogger<SectionService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public void ExecutarTarefa(PartitionCalculator calculator)
        {
            try
            {
                Block block = calculator.GetOrCreateBlock();

                calculator.ProccessBlock(block);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Thread was being aborted."))
                    _logger.LogError(ex, $"Endereço encontrado: {ex.Message}");
            }
        }

        public void GenerateSections()
        {
            PartitionCalculator calculator = new PartitionCalculator(_serviceProvider, 67);

            //Parallel.For(0, 5, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            //i =>
            //{
            //    //_logger.LogCritical($"Processando bloco {i}");

            //    try
            //    {
            //        calculator.ProcessarBloco();
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex, $"Endereço encontrado: {ex.Message}");
            //    }

            //    _logger.LogCritical($"Bloco Processado {i}");
            //});

            //for (int i = 1; i <= 160; i++)
            //{
            //    int numeroDeWorkers = 10000;
            //    List<Task> tasks = new List<Task>();

            //    for (int j = 0; j < numeroDeWorkers; j++)
            //    {
            //        tasks.Add(new Task(() =>
            //        {
            //            try
            //            {
            //                calculator.ProcessarBloco();
            //            }
            //            catch (Exception ex)
            //            {
            //                _logger.LogError(ex, $"Endereço encontrado: {ex.Message}");
            //            }
            //        }));
            //    }

            //    Task.WhenAll(tasks).Wait();
            //}
        }
    }
}
