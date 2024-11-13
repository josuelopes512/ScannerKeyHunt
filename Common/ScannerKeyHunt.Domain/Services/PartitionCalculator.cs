using ScannerKeyHunt.Data.Entities;
using ScannerKeyHunt.Repository.Interfaces;
using System.Numerics;

namespace ScannerKeyHunt.Domain.Services
{
    public class PartitionCalculator
    {
        private static BigInteger HexToBigInt(string hex) => BigInteger.Parse(hex, System.Globalization.NumberStyles.HexNumber);

        private readonly BigInteger startKey;
        private readonly BigInteger stopKey;
        private readonly BigInteger intervalTotal;

        private int numSections = 1024;
        private int areasPerSection = 64;
        private int blocksPerArea = 16;

        private readonly PuzzleWallet _puzzleWallet;
        private readonly IUnitOfWork _unitOfWork;

        public PartitionCalculator(PuzzleWallet puzzleWallet, IUnitOfWork unitOfWork)
        {
            startKey = HexToBigInt(puzzleWallet.StartKey);
            stopKey = HexToBigInt(puzzleWallet.EndKey);
            intervalTotal = stopKey - startKey;
            _puzzleWallet = puzzleWallet;
            _unitOfWork = unitOfWork;

            AdjustPartitionSizes();
        }

        private void AdjustPartitionSizes()
        {
            // Se o intervalo total for menor que o esperado para o número de seções
            if (startKey == stopKey)
            {
                numSections = 1;
                areasPerSection = 1;
                blocksPerArea = 1;
                return;
            }
            
            if (intervalTotal < numSections)
            {
                numSections = (int)intervalTotal; // Ajustar para o número mínimo de seções
            }

            // Se o intervalo total for menor que o esperado para as áreas por seção
            BigInteger totalAreaInterval = intervalTotal / numSections;

            if (totalAreaInterval < areasPerSection)
            {
                areasPerSection = (int)totalAreaInterval; // Ajustar para o número mínimo de áreas
            }

            // Se o intervalo total for menor que o esperado para os blocos por área
            BigInteger totalBlockInterval = totalAreaInterval / areasPerSection;
            if (totalBlockInterval < blocksPerArea)
            {
                blocksPerArea = (int)totalBlockInterval; // Ajustar para o número mínimo de blocos
            }
        }

        private static string BigIntToHex(BigInteger bigInt)
        {
            // Converte para hexadecimal e garante que a string tenha pelo menos 64 caracteres
            string hex = bigInt.ToString("X");

            // Caso o número seja zero, o "ToString" retorna uma string vazia
            // Portanto, se for zero, retornamos uma string com 64 zeros
            if (bigInt == BigInteger.Zero)
            {
                return new string('0', 64);
            }

            // Garantir que a string tenha pelo menos 64 caracteres, adicionando zeros à esquerda se necessário
            return hex.PadLeft(64, '0');
        }

        public List<Section> GenerateSections()
        {
            List<Section> sections = new List<Section>();

            BigInteger intervalPerSection = intervalTotal / numSections;
            BigInteger intervalPerArea = intervalPerSection / areasPerSection;
            BigInteger intervalPerBlock = intervalPerArea / blocksPerArea;

            BigInteger currentKey = startKey;

            for (int s = 0; s < numSections; s++)
            {
                BigInteger sectionEndKey = currentKey + intervalPerSection - 1;

                Section section = new Section
                {
                    PuzzleWalletId = _puzzleWallet.Id,
                    StartKey = BigIntToHex(currentKey),
                    EndKey = BigIntToHex(sectionEndKey),
                    IsCompleted = false,
                    Disabled = false,
                    IsLocked = false,
                    Seed = Guid.NewGuid().ToString()
                };

                _unitOfWork.SectionRepository.Add(section);

                BigInteger areaKey = currentKey;

                for (int a = 0; a < areasPerSection; a++)
                {
                    BigInteger areaEndKey = areaKey + intervalPerArea - 1;

                    Area area = new Area
                    {
                        StartKey = BigIntToHex(areaKey),
                        EndKey = BigIntToHex(areaEndKey),
                        IsCompleted = false,
                        Disabled = false,
                        IsLocked = false,
                        Seed = Guid.NewGuid().ToString()
                    };

                    _unitOfWork.AreaRepository.Add(area);

                    BigInteger blockKey = areaKey;
                    for (int b = 0; b < blocksPerArea; b++)
                    {
                        BigInteger blockEndKey = blockKey + intervalPerBlock - 1;

                        Block block = new Block
                        {
                            StartKey = BigIntToHex(blockKey),
                            EndKey = BigIntToHex(blockEndKey),
                            IsCompleted = false,
                            Disabled = false,
                            IsLocked = false,
                            Seed = Guid.NewGuid().ToString()
                        };

                        _unitOfWork.BlockRepository.Add(block);

                        //area.Blocks.Add(block);
                        blockKey += intervalPerBlock;
                    }

                    //section.Areas.Add(area);
                    areaKey += intervalPerArea;
                }

                sections.Add(section);
                currentKey += intervalPerSection;
            }


            _unitOfWork.Save();

            return sections;
        }
    }
}
