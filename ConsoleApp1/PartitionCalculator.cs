using System.Numerics;

namespace ConsoleApp1
{
    public class PartitionCalculator
    {
        private static BigInteger HexToBigInt(string hex) => BigInteger.Parse(hex, System.Globalization.NumberStyles.HexNumber);
        private static string BigIntToHex(BigInteger bigInt) => bigInt.ToString("X").PadLeft(64, '0'); // Pad to 64 characters for hex

        private readonly BigInteger startKey;
        private readonly BigInteger stopKey;
        private readonly BigInteger intervalTotal;

        private readonly int numSections = 1024;
        private readonly int areasPerSection = 64;
        private readonly int blocksPerArea = 16;

        public PartitionCalculator(string startHex, string stopHex)
        {
            startKey = HexToBigInt(startHex);
            stopKey = HexToBigInt(stopHex);
            intervalTotal = stopKey - startKey;
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
                    StartKey = BigIntToHex(currentKey),
                    EndKey = BigIntToHex(sectionEndKey)
                };

                BigInteger areaKey = currentKey;
                for (int a = 0; a < areasPerSection; a++)
                {
                    BigInteger areaEndKey = areaKey + intervalPerArea - 1;
                    Area area = new Area
                    {
                        StartKey = BigIntToHex(areaKey),
                        EndKey = BigIntToHex(areaEndKey)
                    };

                    BigInteger blockKey = areaKey;
                    for (int b = 0; b < blocksPerArea; b++)
                    {
                        BigInteger blockEndKey = blockKey + intervalPerBlock - 1;
                        area.Blocks.Add(new Block
                        {
                            StartKey = BigIntToHex(blockKey),
                            EndKey = BigIntToHex(blockEndKey)
                        });
                        blockKey += intervalPerBlock;
                    }

                    section.Areas.Add(area);
                    areaKey += intervalPerArea;
                }

                sections.Add(section);
                currentKey += intervalPerSection;
            }

            return sections;
        }
    }
}
