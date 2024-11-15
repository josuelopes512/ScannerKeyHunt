using System.Text.RegularExpressions;

namespace ScannerKeyHunt.Utils
{
    public static class PuzzleParser
    {
        public static List<string> ReadPartsFromFile(string[] lines)
        {
            var parts = new List<string>();

            string currentPart = string.Empty;

            foreach (var line in lines)
            {
                if (line.StartsWith("HEX"))
                {
                    if (!string.IsNullOrEmpty(currentPart))
                    {
                        parts.Add(currentPart);
                    }
                    currentPart = line; // Start a new part
                }
                else
                {
                    currentPart += "\n" + line; // Append line to the current part
                }
            }

            if (!string.IsNullOrEmpty(currentPart))
            {
                parts.Add(currentPart); // Add the last part
            }

            return parts;
        }

        public static Puzzle ParsePuzzleText(string text)
        {
            // Expressões regulares para capturar os dados
            var hexPattern = @"HEX\s(.*?)"; // @"HEX\s([0-9a-fA-F]+)";
            var startKeyPattern = @"Start key\s([0-9a-fA-F]+)";
            var stopKeyPattern = @"Stop key\s([0-9a-fA-F]+)";
            var addressPattern = @"P2PKH\(c\)\s([1A-Za-z0-9]+)";
            var numberPattern = @"Random Bitcoin Puzzle #(\d+)";

            // Usar Regex para capturar os dados
            var hexMatch = Regex.Match(text, hexPattern);
            var startKeyMatch = Regex.Match(text, startKeyPattern);
            var stopKeyMatch = Regex.Match(text, stopKeyPattern);
            var addressMatch = Regex.Match(text, addressPattern);
            var numberMatch = Regex.Match(text, numberPattern);

            // Verificar se as correspondências foram bem-sucedidas
            if (hexMatch.Success && startKeyMatch.Success && stopKeyMatch.Success && addressMatch.Success && numberMatch.Success)
            {
                // Criar o objeto Puzzle com os dados capturados
                return new Puzzle
                {
                    Number = numberMatch.Groups[1].Value,
                    HexStart = startKeyMatch.Groups[1].Value,
                    HexStop = stopKeyMatch.Groups[1].Value,
                    Address = addressMatch.Groups[1].Value,
                    AddressType = AddressType.Compressed // Assume que é "compressed" a partir do exemplo fornecido
                };
            }

            throw new InvalidOperationException("Texto inválido ou não correspondido.");
        }
    }
}
