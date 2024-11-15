using NBitcoin;
using System.Collections.Concurrent;
using System.Numerics;
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

        public static List<AddressWallet> RandomHexList(BigInteger start, BigInteger end)
        {
            int count = (int)(end - start);

            ConcurrentBag<AddressWallet> keyList = new ConcurrentBag<AddressWallet>();

            Parallel.For(0, count, i =>
            {
                string hex = (start + i).ToString("x64");

                keyList.Add(new AddressWallet
                {
                    Address = HexToAddress(hex),
                    PrivateKey = hex
                });
            });

            return keyList.ToList();
        }

        //public static List<AddressWallet> RandomHexList(BigInteger start, BigInteger end)
        //{
        //    List<AddressWallet> keyList = new List<AddressWallet>();

        //    for (BigInteger i = start; i < end; i++)
        //    {
        //        string Hex = String.Format("{0:x64}", i);
        //        keyList.Add(new AddressWallet { Address = HexToAddress(Hex), PrivateKey = Hex });
        //    }

        //    return keyList;
        //}
    }
}
