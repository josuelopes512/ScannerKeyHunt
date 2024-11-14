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

        private const int numSections = 1024;
        private const int areasPerSection = 64;
        private const int blocksPerArea = 16;

        private readonly PuzzleWallet _puzzleWallet;
        private readonly IUnitOfWork _unitOfWork;

        public PartitionCalculator(PuzzleWallet puzzleWallet, IUnitOfWork unitOfWork)
        {
            startKey = HexToBigInt(puzzleWallet.StartKey);
            stopKey = HexToBigInt(puzzleWallet.EndKey);
            intervalTotal = stopKey - startKey;
            _puzzleWallet = puzzleWallet;
            _unitOfWork = unitOfWork;

            //AdjustPartitionSizes();
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

        public static BigInteger HexToBigInteger(string hex)
        {
            if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                hex = hex.Substring(2);
            }

            return BigInteger.Parse("0" + hex, System.Globalization.NumberStyles.HexNumber);
        }

        public List<Section> DividirRange(int maxSetores = numSections, int maxAreas = areasPerSection, int maxBlocos = blocksPerArea)
        {
            List<Section> sections = new List<Section>();
            List<Area> areas = new List<Area>();
            List<Block> blocos = new List<Block>();

            BigInteger inicioRange = startKey; // HexToBigInteger(inicioHex);
            BigInteger fimRange = stopKey; // HexToBigInteger(fimHex);

            BigInteger totalValores = fimRange - inicioRange + 1;

            BigInteger valoresPorBloco = BigInteger.Max(totalValores / (maxSetores * maxAreas * maxBlocos), 1);
            BigInteger valoresPorArea = BigInteger.Max(valoresPorBloco * maxBlocos, 1);
            BigInteger valoresPorSetor = BigInteger.Max(valoresPorArea * maxAreas, 1);

            // Divide o range em setores, áreas e blocos
            for (int s = 0; s < maxSetores; s++)
            {
                BigInteger setorInicio = inicioRange + (s * valoresPorSetor);
                BigInteger setorFim = BigInteger.Min(setorInicio + valoresPorSetor - 1, fimRange);

                Section section = new Section
                {
                    PuzzleWalletId = _puzzleWallet.Id,
                    StartKey = BigIntToHex(setorInicio),
                    EndKey = BigIntToHex(setorFim),
                    IsCompleted = false,
                    Disabled = false,
                    IsLocked = false,
                    Seed = Guid.NewGuid().ToString()
                };

                sections.Add(section);

                //_unitOfWork.SectionRepository.Add(section);

                for (int a = 0; a < maxAreas; a++)
                {
                    BigInteger areaInicio = setorInicio + (a * valoresPorArea);
                    BigInteger areaFim = BigInteger.Min(areaInicio + valoresPorArea - 1, fimRange);

                    Area area = new Area
                    {
                        SectionId = section.Id,
                        Section = section,
                        StartKey = BigIntToHex(areaInicio),
                        EndKey = BigIntToHex(areaFim),
                        IsCompleted = false,
                        Disabled = false,
                        IsLocked = false,
                        Seed = Guid.NewGuid().ToString()
                    };

                    areas.Add(area);

                    for (int b = 0; b < maxBlocos; b++)
                    {
                        BigInteger blocoInicio = areaInicio + (b * valoresPorBloco);
                        BigInteger blocoFim = BigInteger.Min(blocoInicio + valoresPorBloco - 1, fimRange);

                        Block block = new Block
                        {
                            AreaId = area.Id,
                            Area = area,
                            StartKey = BigIntToHex(blocoInicio),
                            EndKey = BigIntToHex(blocoFim),
                            IsCompleted = false,
                            Disabled = false,
                            IsLocked = false,
                            Seed = Guid.NewGuid().ToString()
                        };

                        blocos.Add(block);

                        if (blocoFim >= fimRange) break;
                    }
                    if (areaFim >= fimRange) break;
                }

                if (setorFim >= fimRange) break;
            }

            _unitOfWork.SectionRepository.AddRange(sections);
            _unitOfWork.AreaRepository.AddRange(areas);
            _unitOfWork.BlockRepository.AddRange(blocos);

            _unitOfWork.Save();

            return sections;
        }
    }
}
