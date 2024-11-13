using System.ComponentModel.DataAnnotations.Schema;

namespace ScannerKeyHunt.Data.Entities
{
    public class Area : BaseModel
    {
        [ForeignKey("Section")]
        public virtual Guid SectionId { get; set; }
        public Section Section { get; set; }
        public bool IsLocked { get; set; }
        public bool Disabled { get; set; }
        public bool IsCompleted { get; set; }
        public string StartKey { get; set; }
        public string EndKey { get; set; }
        public string Seed { get; set; }
        public ICollection<Block> Blocks { get; set; }
    }
}