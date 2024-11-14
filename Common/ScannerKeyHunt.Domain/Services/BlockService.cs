using ScannerKeyHunt.Data.Entities;

namespace ScannerKeyHunt.Domain.Services
{
    public class BlockService
    {
        private static Random _random = new Random();

        private string GenerateHexSeed(int length)
        {
            byte[] buffer = new byte[length / 2];
            _random.NextBytes(buffer);
            return "0x" + BitConverter.ToString(buffer).Replace("-", "");
        }

        public Block CreateBlock(long areaId, string startKey, string endKey)
        {
            Block newBlock = new Block
            {
                AreaId = areaId,
                StartKey = startKey,
                EndKey = endKey,
                Seed = GenerateHexSeed(16),
                IsLocked = false,
                IsCompleted = false
            };

            SaveToDatabase(newBlock);
            return newBlock;
        }

        private void SaveToDatabase(Block block)
        {
            // Implementação para salvar no banco de dados
        }
    }
}
