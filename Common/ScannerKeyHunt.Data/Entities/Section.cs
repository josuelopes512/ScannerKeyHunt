using System.ComponentModel.DataAnnotations.Schema;

namespace ScannerKeyHunt.Data.Entities
{
    public class Section : BaseModel
    {
        [ForeignKey("PuzzleWallet")]
        public virtual long PuzzleWalletId { get; set; }
        public PuzzleWallet PuzzleWallet { get; set; }
        public bool IsLocked { get; set; }
        public bool Disabled { get; set; }
        public bool IsCompleted { get; set; }
        public string StartKey { get; set; }
        public string EndKey { get; set; }
        public string Seed { get; set; }
        public ICollection<Area> Areas { get; set; }
    }
}
