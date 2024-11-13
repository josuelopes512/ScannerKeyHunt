﻿using Microsoft.Extensions.Logging;
using ScannerKeyHunt.Data.Entities;
using ScannerKeyHunt.Domain.Interfaces;
using ScannerKeyHunt.Repository;
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

            //string startKeyHex = "0000000000000000000000000000000000000000000000040000000000000000";
            //string stopKeyHex = "000000000000000000000000000000000000000000000007ffffffffffffffff";

            PartitionCalculator calculator = new PartitionCalculator(puzzleWallet, _unitOfWork);
            List<Section> sections = calculator.GenerateSections();

            _unitOfWork.SectionRepository.AddRange(sections);
            _unitOfWork.Save();
        }

        public PuzzleWallet GeneratePuzzles()
        {
            PuzzleWallet puzzleWallet = _unitOfWork.PuzzleWalletCache.GetByExpressionBool(x => x.Address == "1BgGZ9tcN4rm9KBzDn7KprQz87SZ26SAMH");

            if (puzzleWallet == null)
            {
                puzzleWallet = new PuzzleWallet()
                {
                    StartKey = "0000000000000000000000000000000000000000000000000000000000000001",
                    EndKey = "0000000000000000000000000000000000000000000000000000000000000001",
                    Address = "1BgGZ9tcN4rm9KBzDn7KprQz87SZ26SAMH",
                    PuzzleId = "1",
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