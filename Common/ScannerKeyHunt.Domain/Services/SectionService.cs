using Microsoft.Extensions.Logging;
using ScannerKeyHunt.Data.Entities;
using ScannerKeyHunt.Domain.Interfaces;
using ScannerKeyHunt.Repository.Interfaces;

namespace ScannerKeyHunt.Domain.Services
{
    public class SectionService : ISectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SectionService> _logger;

        public SectionService(IUnitOfWork unitOfWork, ILogger<SectionService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public void GenerateSections()
        {
            PuzzleWallet puzzleWallet = GeneratePuzzles();

            List<Section> sections = _unitOfWork.SectionRepository.GetAll(x => x.PuzzleWalletId == puzzleWallet.Id).ToList();

            if (sections.Count == 0)
            {
                PartitionCalculator calculator = new PartitionCalculator(puzzleWallet, _unitOfWork);
                sections = calculator.DividirRange();

                return;
            }
        }

        public PuzzleWallet GeneratePuzzles()
        {
            PuzzleWallet puzzleWallet = _unitOfWork.PuzzleWalletCache.GetByExpressionBool(x => x.Address == "1BY8GQbnueYofwSuFAT3USAhGjPrkxDdW9");

            if (puzzleWallet == null)
            {
                puzzleWallet = new PuzzleWallet()
                {
                    StartKey = "0000000000000000000000000000000000000000000000040000000000000000",
                    EndKey = "000000000000000000000000000000000000000000000007ffffffffffffffff",
                    Address = "1BY8GQbnueYofwSuFAT3USAhGjPrkxDdW9",
                    PuzzleId = "67",
                    IsLocked = false,
                    Disabled = false,
                    IsCompleted = false
                };

                _unitOfWork.PuzzleWalletCache.Add(puzzleWallet);
                _unitOfWork.Save();
            }

            return puzzleWallet;
        }
    }
}
