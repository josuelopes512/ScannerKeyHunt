using NBitcoin;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using SHA256 = System.Security.Cryptography.SHA256;

namespace ScannerKeyHunt.Utils
{
    public static class Helpers
    {
        public static byte[] StringToByteArray(string Value)
        {
            return Enumerable.Range(0, Value.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(Value.Substring(x, 2), 16))
                         .ToArray();
        }

        public static string ConvertSHA256(string Value)
        {
            byte[] Bytes = StringToByteArray(Value);
            string hashString = string.Empty;
            byte[] encrypt = SHA256.Create().ComputeHash(Bytes);

            foreach (byte _Byte in encrypt)
            {
                hashString += _Byte.ToString("x2");
            }

            return hashString;
        }

        public static string HexToAddress(string Hex, bool IsCompressed = true)
        {
            string PrivateKey = "80" + Hex;
            string Hash1 = ConvertSHA256(PrivateKey);
            string Hash2 = ConvertSHA256(Hash1);
            string First4Bytes = Hash2.Substring(0, 8);
            string Checksum = (PrivateKey + First4Bytes);
            byte[] Data = StringToByteArray(Checksum);
            string WIF = NBitcoin.DataEncoders.Encoders.Base58.EncodeData(Data);

            if (IsCompressed)
            {
                var Secret = new BitcoinSecret(WIF, Network.Main);
                return Secret.PubKey.Compress().GetAddress(ScriptPubKeyType.Legacy, Network.Main).ToString();
            }
            else
            {
                var Secret = new BitcoinSecret(WIF, Network.Main);
                return Secret.GetAddress(ScriptPubKeyType.Legacy).ToString();
            }
        }

        private static IEnumerable<BigInteger> GenerateRange(BigInteger start, BigInteger count)
        {
            for (BigInteger i = 0; i < count; i++)
            {
                yield return start + i;
            }
        }

        public static List<AddressWallet> IntervalSeedHexList(BigInteger start, BigInteger end)
        {
            if (start > end)
                throw new ArgumentException("The start value cannot be greater than the end value.");

            BigInteger count = (start == end) ? start : (end - start + 1);

            if (count > 10)
            {
                count = 10;
            }

            ConcurrentBag<AddressWallet> keyList = new ConcurrentBag<AddressWallet>();

            Parallel.ForEach(GenerateRange(start, count), current =>
            {
                string hex = current.ToString("x64");

                keyList.Add(new AddressWallet
                {
                    Address = HexToAddress(hex),
                    PrivateKey = hex
                });
            });

            return keyList.ToList();
        }

        public static List<AddressWallet> InvervalHexList(BigInteger start, BigInteger end)
        {
            if (start > end)
                throw new ArgumentException("The start value cannot be greater than the end value.");

            BigInteger count = (start == end) ? start : (end - start + 1);

            ConcurrentBag<AddressWallet> keyList = new ConcurrentBag<AddressWallet>();

            Parallel.ForEach(GenerateRange(start, count), current =>
            {
                string hex = current.ToString("x64");

                keyList.Add(new AddressWallet
                {
                    Address = HexToAddress(hex),
                    PrivateKey = hex
                });
            });

            return keyList.ToList();
        }

        public static string GerarHashv1(List<string> enderecos)
        {
            enderecos.Sort();

            var stringBuilder = new StringBuilder();
            foreach (var endereco in enderecos)
            {
                stringBuilder.Append(endereco).Append(",");
            }

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
            return Convert.ToBase64String(hashBytes);
        }

        public static string GerarHashv2(List<string> enderecos)
        {
            enderecos.Sort();

            using var sha256 = SHA256.Create();
            using var memoryStream = new MemoryStream();
            using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal))
            {
                foreach (var endereco in enderecos)
                {
                    var bytes = Encoding.UTF8.GetBytes(endereco + ",");
                    gzipStream.Write(bytes, 0, bytes.Length);
                }
            }

            var compressedData = memoryStream.ToArray();
            var hashBytes = sha256.ComputeHash(compressedData);
            return Convert.ToBase64String(hashBytes);
        }

        public static string GerarHashv3(List<string> enderecos)
        {
            enderecos.Sort();

            using var sha256 = SHA256.Create();
            var combinedHash = new byte[sha256.HashSize / 8];

            foreach (var endereco in enderecos)
            {
                var itemHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(endereco));
                for (int i = 0; i < combinedHash.Length; i++)
                {
                    combinedHash[i] ^= itemHash[i]; // Combinação usando XOR
                }
            }

            return Convert.ToBase64String(combinedHash);
        }

        public static string GerarHashv4(List<string> enderecos)
        {
            enderecos.Sort();

            using var sha256 = SHA256.Create();
            using var memoryStream = new MemoryStream();
            using (var cryptoStream = new CryptoStream(memoryStream, sha256, CryptoStreamMode.Write))
            {
                foreach (var endereco in enderecos)
                {
                    var bytes = Encoding.UTF8.GetBytes(endereco + ","); // Inclua o delimitador manualmente
                    cryptoStream.Write(bytes, 0, bytes.Length);
                }
            }

            // O hash está disponível no SHA256 após o fechamento do CryptoStream
            var hashBytes = sha256.Hash!;
            return Convert.ToBase64String(hashBytes);
        }

        public static string BigIntToHex(BigInteger bigInt)
        {
            string hex = bigInt.ToString("X");

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

        public static bool ValidarHash(List<string> enderecos, string hash)
        {
            string result = GerarHashv4(enderecos);

            return result == hash;
        }
    }
}
