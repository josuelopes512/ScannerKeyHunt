using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScannerKeyHunt.Data.Entities
{
    public class BaseModel : BaseDate
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("User")]
        public Guid CreationUserId { get; set; } = Guid.Empty;

        [ForeignKey("User")]
        public Guid? UpdateUserId { get; set; }
    }
}
