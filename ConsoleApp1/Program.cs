using ScannerKeyHunt.Utils;
using System.Text.Json;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filePath = "wallets.txt"; // Caminho do arquivo a ser lido

            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);

                List<string> parts = PuzzleParser.ReadPartsFromFile(lines);

                List<Puzzle> puzzles = parts
                    .Select(PuzzleParser.ParsePuzzleText)
                    .ToList();

                var data = JsonSerializer.Serialize(puzzles, new JsonSerializerOptions { WriteIndented = true });

            }
            else
            {
                Console.WriteLine($"O arquivo '{filePath}' não foi encontrado.");
            }
        }
    }
}
