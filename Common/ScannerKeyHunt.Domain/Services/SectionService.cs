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
            }
        }

        public void GenerateSections()
        {
            PartitionCalculator calculator = new PartitionCalculator(_serviceProvider);
            calculator.GeneratePuzzle();

            int numeroDeWorkers = 10000;
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < numeroDeWorkers; i++)
            {
                tasks.Add(Task.Run(() => ExecutarTarefa(calculator)));
            }

            Task.WhenAll(tasks).Wait();
        }
    }
}
