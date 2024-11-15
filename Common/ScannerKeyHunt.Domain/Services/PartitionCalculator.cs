using Microsoft.Extensions.DependencyInjection;
using ScannerKeyHunt.Data.Entities;
using ScannerKeyHunt.Repository.Interfaces;
using System.Numerics;
using System.Security.Cryptography;

namespace ScannerKeyHunt.Domain.Services
{
    public class PartitionCalculator
    {
        private static BigInteger HexToBigInt(string hex) => BigInteger.Parse(hex, System.Globalization.NumberStyles.HexNumber);

        private readonly BigInteger startKey;
        private readonly BigInteger stopKey;
        private readonly BigInteger intervalTotal;

        private const long numSections = 1024;
        private const long areasPerSection = 64;
        private const long blocksPerArea = 16;
        private const long intervalBlocks = 50000;

        private readonly PuzzleWallet _puzzleWallet;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceProvider _serviceProvider;

        public PartitionCalculator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _unitOfWork = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();
            _puzzleWallet = GeneratePuzzles();
            startKey = HexToBigInt(_puzzleWallet.StartKey);
            stopKey = HexToBigInt(_puzzleWallet.EndKey);
            intervalTotal = stopKey - startKey;
        }

        public void GeneratePuzzle()
        {
            List<Section> sections = _unitOfWork.SectionRepository.GetAll(x => x.PuzzleWalletId == _puzzleWallet.Id).ToList();

            if (sections.Count == 0)
            {
                sections = DividirRange();
            }
        }

        private PuzzleWallet GeneratePuzzles()
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

        public List<Section> DividirRange()
        {
            List<Section> sections = new List<Section>();
            List<Area> areas = new List<Area>();

            BigInteger inicioRange = startKey; // HexToBigInteger(inicioHex);
            BigInteger fimRange = stopKey; // HexToBigInteger(fimHex);

            BigInteger totalValores = fimRange - inicioRange + 1;

            BigInteger valoresPorBloco = BigInteger.Max(totalValores / (numSections * areasPerSection * blocksPerArea), 1);
            BigInteger valoresPorArea = BigInteger.Max(valoresPorBloco * blocksPerArea, 1);
            BigInteger valoresPorSetor = BigInteger.Max(valoresPorArea * areasPerSection, 1);

            for (long s = 0; s < numSections; s++)
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

                if (setorFim >= fimRange) break;
            }

            _unitOfWork.SectionRepository.AddRange(sections);

            foreach (Section section in sections)
            {
                for (long a = 0; a < areasPerSection; a++)
                {
                    BigInteger areaInicio = HexToBigInteger(section.StartKey) + (a * valoresPorArea);
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

                    if (areaFim >= fimRange) break;
                }
            }

            _unitOfWork.AreaRepository.AddRange(areas);

            _unitOfWork.Save();

            return sections;
        }

        private void CreateArea(List<Area> areas)
        {
            List<Block> blocos = new List<Block>();

            BigInteger totalValores = stopKey - startKey + 1;
            BigInteger valoresPorBloco = BigInteger.Max(totalValores / (numSections * areasPerSection * blocksPerArea), 1);

            foreach (Area area in areas)
            {
                for (int b = 0; b < blocksPerArea; b++)
                {
                    BigInteger blocoInicio = HexToBigInteger(area.StartKey) + (b * valoresPorBloco);
                    BigInteger blocoFim = BigInteger.Min(blocoInicio + valoresPorBloco - 1, stopKey);

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

                    if (blocoFim >= stopKey) break;
                }
            }

            _unitOfWork.BlockRepository.AddRange(blocos);

            _unitOfWork.Save();
        }

        private static BigInteger GerarBigIntegerAleatorio(BigInteger min, BigInteger max)
        {
            if (min > max) throw new ArgumentException("O valor mínimo não pode ser maior que o valor máximo.");

            BigInteger intervalo = max - min + 1;
            int numeroBytes = intervalo.ToByteArray().Length;
            BigInteger resultado;

            // Gera um número aleatório dentro do intervalo até que seja menor do que o intervalo desejado
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] bytesAleatorios = new byte[numeroBytes];
                do
                {
                    rng.GetBytes(bytesAleatorios);
                    resultado = new BigInteger(bytesAleatorios);

                    // Ajusta o valor para que seja positivo
                    resultado = BigInteger.Abs(resultado);
                } while (resultado >= intervalo);
            }

            return resultado + min;
        }

        public Block GetOrCreateBlock()
        {
            IUnitOfWork unitOfWork = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();

            Block block = unitOfWork.BlockRepository.GetAll(x => !x.IsCompleted && !x.IsLocked && !x.Disabled ).ToList().FirstOrDefault();

            if (block == null)
            {
                block = SortedSectionArea();
            }

            return block;
        }

        public void ProccessBlock(Block block)
        {
            //for (BigInteger i = HexToBigInteger(block.StartKey); i < HexToBigInteger(block.EndKey); i++)
            //{
            //    //Console.WriteLine(i);
            //}

            block.IsCompleted = true;

            IUnitOfWork unitOfWork = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();

            unitOfWork.BlockRepository.Update(block);
            unitOfWork.Save();
        }

        public Block SortedSectionArea()
        {
            IUnitOfWork unitOfWork = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();

            Random rnd = new Random();

            Section section = unitOfWork.SectionRepository.GetAll(x => x.PuzzleWalletId == _puzzleWallet.Id).ToList().OrderBy(item => rnd.Next()).FirstOrDefault();

            Area area = unitOfWork.AreaRepository.GetAll(x => x.SectionId == section.Id).ToList().OrderBy(item => rnd.Next()).FirstOrDefault();

            List<BigInteger> oldBlocks = unitOfWork.BlockRepository.GetAll(x => x.AreaId == area.Id).ToList().Select(x => HexToBigInteger(x.StartKey)).ToList();

            BigInteger inicioNumeroAleatorio = GerarBigIntegerAleatorio(HexToBigInteger(area.StartKey), HexToBigInteger(area.EndKey));

            bool isOkvalidateBlock = false;

            while (!isOkvalidateBlock)
            {
                bool hasbreak = false;
                foreach (BigInteger initBlock in oldBlocks)
                {
                    BigInteger endBlock = (initBlock + 50000) > HexToBigInteger(area.EndKey) ? HexToBigInteger(area.EndKey) : (initBlock + 50000);
                    hasbreak = false;

                    while (inicioNumeroAleatorio >= initBlock && inicioNumeroAleatorio <= endBlock)
                    {
                        inicioNumeroAleatorio = GerarBigIntegerAleatorio(HexToBigInteger(area.StartKey), HexToBigInteger(area.EndKey));
                        hasbreak = true;
                        break;
                    }

                    if (hasbreak) break;
                }
                if (hasbreak) continue;
                isOkvalidateBlock = true;
            }

            BigInteger fimNumeroAleatorio = (inicioNumeroAleatorio + 50000) > HexToBigInteger(area.EndKey) ? HexToBigInteger(area.EndKey) : (inicioNumeroAleatorio + 50000);

            Block block = unitOfWork.BlockRepository.GetAll(x => x.AreaId == area.Id && x.StartKey == BigIntToHex(inicioNumeroAleatorio)).ToList().FirstOrDefault();

            if (block == null)
            {
                block = new Block
                {
                    AreaId = area.Id,
                    Area = area,
                    StartKey = BigIntToHex(inicioNumeroAleatorio),
                    EndKey = BigIntToHex(fimNumeroAleatorio),
                    IsCompleted = false,
                    Disabled = false,
                    IsLocked = false,
                    Seed = Guid.NewGuid().ToString()
                };

                unitOfWork.BlockRepository.Add(block);
                unitOfWork.Save();
            }

            return block;
        }
    }
}
