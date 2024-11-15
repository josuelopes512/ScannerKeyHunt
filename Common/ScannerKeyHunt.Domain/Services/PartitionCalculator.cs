using Microsoft.Extensions.DependencyInjection;
using Pipelines.Sockets.Unofficial.Arenas;
using ScannerKeyHunt.Data.Entities;
using ScannerKeyHunt.Repository.Interfaces;
using ScannerKeyHunt.Utils;
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

        private readonly long _walletPuzzleNumber;
        private readonly Puzzle _puzzle;
        private readonly PuzzleWallet _puzzleWallet;
        private readonly IServiceProvider _serviceProvider;
        private List<Section> _sections;

        public PartitionCalculator(IServiceProvider serviceProvider, long walletPuzzleNumber = 2)
        {
            _serviceProvider = serviceProvider;

            _walletPuzzleNumber = walletPuzzleNumber;
            _puzzle = PuzzleList.GetPuzzles().FirstOrDefault(x => x.Number.Equals(_walletPuzzleNumber.ToString()));
            _puzzleWallet = GeneratePuzzles();
            startKey = HexToBigInt(_puzzleWallet.StartKey);
            stopKey = HexToBigInt(_puzzleWallet.EndKey);
            intervalTotal = stopKey - startKey;

            GeneratePuzzle();
        }

        public void ProcessarBloco()
        {
            try
            {
                Block block = GetOrCreateBlock();

                ProccessBlock(block);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Endereço encontrado"))
                    throw ex;
            }
        }

        private void GeneratePuzzle()
        {
            IUnitOfWork unitOfWork = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();
            _sections = unitOfWork.SectionRepository.GetAll(x => x.PuzzleWalletId == _puzzleWallet.Id).ToList();

            if (_sections.Count == 0)
            {
                _sections = DividirRange();
            }
        }

        private PuzzleWallet GeneratePuzzles()
        {
            IUnitOfWork unitOfWork = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();
            PuzzleWallet puzzleWallet = unitOfWork.PuzzleWalletCache.GetByExpressionBool(x => x.Address == _puzzle.Address && string.IsNullOrEmpty(x.PrivateKey) && !x.IsCompleted && !x.IsLocked && !x.Disabled);

            if (puzzleWallet == null)
            {
                puzzleWallet = new PuzzleWallet()
                {
                    StartKey = _puzzle.HexStart,
                    EndKey = _puzzle.HexStop,
                    Address = _puzzle.Address,
                    PuzzleId = _puzzle.Number,
                    IsLocked = false,
                    Disabled = false,
                    IsCompleted = false
                };

                unitOfWork.PuzzleWalletCache.Add(puzzleWallet);
                unitOfWork.Save();
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
            IUnitOfWork unitOfWork = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();

            BigInteger inicioRange = startKey;
            BigInteger fimRange = stopKey;

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

            unitOfWork.SectionRepository.AddRange(sections);

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

            unitOfWork.AreaRepository.AddRange(areas);

            unitOfWork.Save();

            return sections;
        }

        private void CreateArea(List<Area> areas)
        {
            List<Block> blocos = new List<Block>();
            IUnitOfWork unitOfWork = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();

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

            unitOfWork.BlockRepository.AddRange(blocos);

            unitOfWork.Save();
        }

        //private static BigInteger GerarBigIntegerAleatorio(BigInteger min, BigInteger max)
        //{
        //    if (min > max) throw new ArgumentException("O valor mínimo não pode ser maior que o valor máximo.");

        //    BigInteger intervalo = max - min + 1;
        //    int numeroBytes = intervalo.ToByteArray().Length;
        //    BigInteger resultado;

        //    // Gera um número aleatório dentro do intervalo até que seja menor do que o intervalo desejado
        //    using (var rng = RandomNumberGenerator.Create())
        //    {
        //        byte[] bytesAleatorios = new byte[numeroBytes];
        //        do
        //        {
        //            rng.GetBytes(bytesAleatorios);
        //            resultado = new BigInteger(bytesAleatorios);

        //            // Ajusta o valor para que seja positivo
        //            resultado = BigInteger.Abs(resultado);
        //        } while (resultado >= intervalo);
        //    }

        //    return resultado + min;
        //}

        private static BigInteger GerarBigIntegerAleatorio(BigInteger min, BigInteger max)
        {
            BigInteger intervalo = max - min + 1;
            byte[] bytes = intervalo.ToByteArray();

            BigInteger resultado;
            using (var rng = RandomNumberGenerator.Create())
            {
                do
                {
                    rng.GetBytes(bytes);
                    bytes[^1] &= 0x7F; // Garante que o número gerado seja positivo
                    resultado = new BigInteger(bytes);
                } while (resultado >= intervalo);
            }

            return resultado + min;
        }

        public Block GetOrCreateBlock()
        {
            IUnitOfWork unitOfWork = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();

            Random rnd = new Random();

            Section section = _sections
                .OrderBy(item => rnd.Next())
                .FirstOrDefault();

            Area area = unitOfWork.AreaRepository
                .GetAll(x => x.SectionId == section.Id)
                .OrderBy(item => rnd.Next())
                .FirstOrDefault();

            Block block = unitOfWork.BlockRepository.GetAll(x => !x.IsCompleted && !x.IsLocked && !x.Disabled && x.AreaId == area.Id).FirstOrDefault();

            return block ?? SortedSectionArea(unitOfWork, area);
        }

        public void ProccessBlock(Block block)
        {
            List<AddressWallet> addressWallets = Utils.Helpers.RandomHexList(HexToBigInteger(block.StartKey), HexToBigInteger(block.EndKey));

            IUnitOfWork unitOfWork = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();
            
            if (addressWallets.Select(x => x.Address).Contains(_puzzle.Address))
            {
                _puzzleWallet.PrivateKey = addressWallets.Where(x => x.Address == _puzzle.Address).First().PrivateKey;
                _puzzleWallet.IsLocked = true;
                _puzzleWallet.Disabled = true;
                _puzzleWallet.IsCompleted = true;

                unitOfWork.PuzzleWalletCache.Update(_puzzleWallet);
            }
                //throw new Exception($"Endereço encontrado: {addressWallets.Where(x => x.Address == _puzzle.Address).First().ToString()}");

            block.IsCompleted = true;

            unitOfWork.BlockRepository.Update(block);
            unitOfWork.Save();
        }

        private BigInteger ValidateHasBlockProcessed(Area area)
        {
            IUnitOfWork unitOfWork = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();

            BigInteger randomNumber = GerarBigIntegerAleatorio(
                HexToBigInteger(area.StartKey),
                HexToBigInteger(area.EndKey));

            List<BigInteger> oldBlocks = unitOfWork.BlockRepository
                .GetAll(x => x.AreaId == area.Id)
                .Select(x => HexToBigInteger(x.StartKey))
                .ToList();

            bool isOkvalidateBlock = false;

            while (!isOkvalidateBlock)
            {
                bool hasbreak = false;

                foreach (BigInteger initBlock in oldBlocks)
                {
                    BigInteger endBlock = (initBlock + 50000) > HexToBigInteger(area.EndKey) ? HexToBigInteger(area.EndKey) : (initBlock + 50000);
                    hasbreak = false;

                    while (randomNumber >= initBlock && randomNumber <= endBlock && initBlock != endBlock)
                    {
                        randomNumber = GerarBigIntegerAleatorio(HexToBigInteger(area.StartKey), HexToBigInteger(area.EndKey));
                        hasbreak = true;
                        break;
                    }

                    if (hasbreak) break;
                }

                if (hasbreak) continue;
                isOkvalidateBlock = true;
            }

            return randomNumber;
        }

        public Block CreateBlock(IUnitOfWork unitOfWork, Area area, BigInteger startRandomNumber, BigInteger endRandomNumber)
        {
            Block block = new Block
            {
                AreaId = area.Id,
                Area = area,
                StartKey = BigIntToHex(startRandomNumber),
                EndKey = BigIntToHex(endRandomNumber),
                IsCompleted = false,
                Disabled = false,
                IsLocked = false,
                Seed = Guid.NewGuid().ToString()
            };

            unitOfWork.BlockRepository.Add(block);
            unitOfWork.Save();

            return block;
        }

        private Block SortedSectionArea(IUnitOfWork unitOfWork, Area area)
        {
            BigInteger startRandomNumber = ValidateHasBlockProcessed(area);

            BigInteger endRandomNumber = (startRandomNumber + 50000) > HexToBigInteger(area.EndKey) ? HexToBigInteger(area.EndKey) : (startRandomNumber + 50000);

            Block block = unitOfWork.BlockRepository
                .GetAll(x => x.AreaId == area.Id && x.StartKey == BigIntToHex(startRandomNumber))
                .FirstOrDefault();

            return block ?? CreateBlock(unitOfWork, area, startRandomNumber, endRandomNumber);
        }
    }
}
