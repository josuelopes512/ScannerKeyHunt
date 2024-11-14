using System.Numerics;

namespace ConsoleApp1
{
    internal class Program
    {
        public static RangeDivisao[] DividirRange(string inicioHex, string fimHex, int maxSetores, int maxAreas, int maxBlocos)
        {
            // Converte as strings hexadecimais para BigInteger
            BigInteger inicioRange = HexToBigInteger(inicioHex);
            BigInteger fimRange = HexToBigInteger(fimHex);

            // Calcula o tamanho total do range
            BigInteger totalValores = fimRange - inicioRange + 1;

            // Ajusta os valores por bloco, área e setor para garantir ao menos 1 valor por bloco
            BigInteger valoresPorBloco = BigInteger.Max(totalValores / (maxSetores * maxAreas * maxBlocos), 1);
            BigInteger valoresPorArea = BigInteger.Max(valoresPorBloco * maxBlocos, 1);
            BigInteger valoresPorSetor = BigInteger.Max(valoresPorArea * maxAreas, 1);

            // Lista de divisões para armazenar os intervalos
            var divisaoRanges = new RangeDivisao[maxSetores * maxAreas * maxBlocos];
            int index = 0;

            // Divide o range em setores, áreas e blocos
            for (int s = 0; s < maxSetores; s++)
            {
                BigInteger setorInicio = inicioRange + (s * valoresPorSetor);
                BigInteger setorFim = BigInteger.Min(setorInicio + valoresPorSetor - 1, fimRange);

                for (int a = 0; a < maxAreas; a++)
                {
                    BigInteger areaInicio = setorInicio + (a * valoresPorArea);
                    BigInteger areaFim = BigInteger.Min(areaInicio + valoresPorArea - 1, fimRange);

                    for (int b = 0; b < maxBlocos; b++)
                    {
                        BigInteger blocoInicio = areaInicio + (b * valoresPorBloco);
                        BigInteger blocoFim = BigInteger.Min(blocoInicio + valoresPorBloco - 1, fimRange);

                        divisaoRanges[index++] = new RangeDivisao
                        {
                            SetorInicio = setorInicio,
                            SetorFim = setorFim,
                            AreaInicio = areaInicio,
                            AreaFim = areaFim,
                            BlocoInicio = blocoInicio,
                            BlocoFim = blocoFim
                        };

                        // Sai do loop se alcançarmos o fim do range
                        if (blocoFim >= fimRange) break;
                    }
                    if (areaFim >= fimRange) break;
                }
                if (setorFim >= fimRange) break;
            }

            // Retorna apenas os intervalos preenchidos
            Array.Resize(ref divisaoRanges, index);
            return divisaoRanges;
        }




        public static BigInteger HexToBigInteger(string hex)
        {
            if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                hex = hex.Substring(2);
            }

            return BigInteger.Parse("0" + hex, System.Globalization.NumberStyles.HexNumber);
        }

        static void Main(string[] args)
        {
            string startKeyHex = "0000000000000000000000000000000000000000000000000000000000000000";
            string stopKeyHex = "0000000000000000000000000000000000000000000000000000000000000001";
            //string stopKeyHex = "000000000000000000000000000000000000000000000007ffffffffffffffff";
            RangeDivisao[] divisaoRange = DividirRange(startKeyHex, stopKeyHex, 10, 5, 2);

            //PartitionCalculator calculator = new PartitionCalculator(startKeyHex, stopKeyHex);
            //List<Section> sections = calculator.GenerateSections();

            //// Exibir um exemplo do primeiro section, area, e bloco
            //Console.WriteLine("Section 0: \nStart = " + sections[0].StartKey + ", \nEnd = " + sections[0].EndKey);
            //Console.WriteLine("  Area 0: \nStart = " + sections[0].Areas[0].StartKey + ", \nEnd = " + sections[0].Areas[0].EndKey);
            //Console.WriteLine("    Block 0: \nStart = " + sections[0].Areas[0].Blocks[0].StartKey + ", \nEnd = " + sections[0].Areas[0].Blocks[0].EndKey);
        }
    }
}
