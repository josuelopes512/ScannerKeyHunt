namespace ScannerKeyHunt
{
    public class BlockResult
    {
        public int AreaId { get; set; }
        public string? Area { get; set; }
        public bool IsLocked { get; set; }
        public bool Disabled { get; set; }
        public bool IsCompleted { get; set; }
        public string StartKey { get; set; } = string.Empty;
        public string EndKey { get; set; } = string.Empty;
        public string? LastKey { get; set; }
        public string? Seed { get; set; }
        public int Id { get; set; }
        public Guid CreationUserId { get; set; }
        public Guid? UpdateUserId { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? DeletionDate { get; set; }
    }
}
