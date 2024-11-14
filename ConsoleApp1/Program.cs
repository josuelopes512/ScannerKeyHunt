using System.Diagnostics;
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

        public static double CalculateTime(long endValue)
        {
            long range = endValue + 1;

            // Criando um Stopwatch para medir o tempo de execução
            Stopwatch stopwatch = new Stopwatch();

            // Iniciando o cronômetro
            stopwatch.Start();

            // Ajuste da prioridade da thread
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            // Número de threads desejadas para paralelismo
            int threadCount = Environment.ProcessorCount;

            // Dividindo o intervalo de iterações em partes iguais para cada thread
            Parallel.For(0, threadCount, (threadIndex) =>
            {
                long start = threadIndex * (range / threadCount);
                long stop = (threadIndex + 1) * (range / threadCount);

                // Garantir que a última thread cubra o restante do intervalo
                if (threadIndex == threadCount - 1) stop = range;

                // Loop otimizado usando Vector (SIMD)
                for (long i = start; i < stop; i += Vector<int>.Count)
                {
                    // Criando um vetor com valores simulados (poderia ser uma operação aqui)
                    Vector<int> vector = new Vector<int>(new int[Vector<int>.Count]);

                    for (int j = 0; j < Vector<int>.Count; j++)
                    {
                        if (i + j < range)
                        {
                            //vector[j] = 1; // Substitua por operação matemática real se necessário
                        }
                    }
                    // Realizar operações com o vetor (sem operação pesada dentro do loop)
                }
            });

            // Parando o cronômetro
            stopwatch.Stop();

            // Exibindo o tempo de execução
            //Console.WriteLine($"Tempo de execução otimizado: {stopwatch.Elapsed.TotalSeconds} segundos");

            return stopwatch.Elapsed.TotalSeconds;
        }

        static void Main(string[] args)
        {
            // Definindo o número de iterações
            //long endValue = 70368744177663;

            //long endValue = 999999; // 0,0638477
            //long endValue = 9999999; // 0,1209405
            //long endValue = 99999999; // 0,2048091
            //long endValue = 999999999; // 0,6706422
            //long endValue = 9999999999; // 4,3406157

            // Valor inicial
            long number = 9;

            // Número de iterações (você pode ajustar isso conforme necessário)
            int iterations = 100;

            // Loop que adiciona 9 à casa decimal
            for (int i = 0; i < iterations; i++)
            {
                // Multiplica o número por 10 e adiciona 9
                number = number * 10 + 9;

                // Imprime o número a cada iteração
                Console.WriteLine($"Iteração {i + 1}: {number}, Time: {CalculateTime(number)}");
            }

            

            //string startKeyHex = "0000000000000000000000000000000000000000000000000000000000000000";
            //string stopKeyHex = "0000000000000000000000000000000000000000000000000000000000000001";
            //string stopKeyHex = "000000000000000000000000000000000000000000000007ffffffffffffffff";
            //RangeDivisao[] divisaoRange = DividirRange(startKeyHex, stopKeyHex, 10, 5, 2);

            //PartitionCalculator calculator = new PartitionCalculator(startKeyHex, stopKeyHex);
            //List<Section> sections = calculator.GenerateSections();

            //// Exibir um exemplo do primeiro section, area, e bloco
            //Console.WriteLine("Section 0: \nStart = " + sections[0].StartKey + ", \nEnd = " + sections[0].EndKey);
            //Console.WriteLine("  Area 0: \nStart = " + sections[0].Areas[0].StartKey + ", \nEnd = " + sections[0].Areas[0].EndKey);
            //Console.WriteLine("    Block 0: \nStart = " + sections[0].Areas[0].Blocks[0].StartKey + ", \nEnd = " + sections[0].Areas[0].Blocks[0].EndKey);
        }
    }
}
