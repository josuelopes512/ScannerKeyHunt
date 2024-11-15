namespace ScannerKeyHunt.Utils
{
    public class Puzzle
    {
        /// <summary>
        /// Puzzle number
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Puzzle HEX start
        /// </summary>
        public string HexStart { get; set; }

        /// <summary>
        /// Puzzle HEX strop
        /// </summary>
        public string HexStop { get; set; }

        /// <summary>
        /// Target Bitcoin wallet address
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Bitcoin wallet address type
        /// </summary>
        public AddressType AddressType { get; set; }
    }
}
