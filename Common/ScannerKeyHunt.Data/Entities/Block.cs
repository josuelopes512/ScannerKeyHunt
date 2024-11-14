using System.ComponentModel.DataAnnotations.Schema;

namespace ScannerKeyHunt.Data.Entities
{
    public class Block : BaseModel
    {
        [ForeignKey("Area")]
        public virtual long AreaId { get; set; }
        public Area Area { get; set; }
        public bool IsLocked { get; set; }
        public bool Disabled { get; set; }
        public bool IsCompleted { get; set; }
        public string StartKey { get; set; }
        public string EndKey { get; set; }
        public string? LastKey { get; set; }
        public string Seed { get; set; }
    }
}