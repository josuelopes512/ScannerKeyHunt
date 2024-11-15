namespace ScannerKeyHunt.Utils
{
    public class AddressWallet
    {
        public string Address { get; set; }

        public string PrivateKey { get; set; }

        public string ToString()
        {
            return $"Address: {Address}, PrivateKey: {PrivateKey}";
        }
    }
}
