namespace ScannerKeyHunt
{
    public class BlockRequest
    {
        public long Id { get; set; }
        public string Seed { get; set; }
        public string PrivateKey { get; set; }
        public long PuzzleId { get; set; }
    }
}
