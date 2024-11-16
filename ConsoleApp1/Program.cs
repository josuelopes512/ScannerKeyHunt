using Newtonsoft.Json;
using ScannerKeyHunt.Utils;
using System.Data;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool isRunning = true;

            while (isRunning)
            {
                try
                {
                    var reqget = Util.GetDataAsync("https://localhost:7223", "/api/Block/1");

                    BlockResult data = JsonConvert.DeserializeObject<BlockResult>(reqget);

                    var wallet = PuzzleList.GetPuzzles().Where(X => X.Number == "3").FirstOrDefault();

                    List<AddressWallet> addressWallets = Helpers.InvervalHexList(Helpers.HexToBigInteger(data.StartKey), Helpers.HexToBigInteger(data.EndKey));

                    string seed = Helpers.GerarHashv4(addressWallets.Select(x => x.Address).ToList());
                    string privateKey = string.Empty;

                    if (addressWallets.Select(x => x.Address).Contains(wallet.Address))
                    {
                        isRunning = false;
                        privateKey = addressWallets.FirstOrDefault(x => x.Address == wallet.Address)?.PrivateKey;

                        Console.WriteLine($"Seed: {seed}");
                        Console.WriteLine($"PrivateKey: {privateKey}");
                    }

                    Util.RequestPost("https://localhost:7223", $"/api/Block", JsonConvert.SerializeObject(new { Id = data.Id, Seed = seed, PrivateKey = privateKey }));
                }
                catch (Exception)
                {

                }

                Task.Delay(10000).Wait();
            }


            //string filePath = "wallets.txt"; // Caminho do arquivo a ser lido

            //if (File.Exists(filePath))
            //{
            //    var lines = File.ReadAllLines(filePath);

            //    List<string> parts = PuzzleParser.ReadPartsFromFile(lines);

            //    List<Puzzle> puzzles = parts
            //        .Select(PuzzleParser.ParsePuzzleText)
            //        .ToList();

            //    var data = JsonSerializer.Serialize(puzzles, new JsonSerializerOptions { WriteIndented = true });

            //}
            //else
            //{
            //    Console.WriteLine($"O arquivo '{filePath}' não foi encontrado.");
            //}
        }
    }
}
