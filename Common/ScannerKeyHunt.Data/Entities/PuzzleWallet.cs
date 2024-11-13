namespace ScannerKeyHunt.Data.Entities
{
    public class PuzzleWallet : BaseModel
    {
        public string StartKey { get; set; }
        public string EndKey { get; set; }
        public string? PrivateKey { get; set; }
        public string Address { get; set; }
        public string PuzzleId { get; set; }
        public bool IsLocked { get; set; }
        public bool Disabled { get; set; }
        public bool IsCompleted { get; set; }
        public ICollection<Section> Sections { get; set; }
    }
}
