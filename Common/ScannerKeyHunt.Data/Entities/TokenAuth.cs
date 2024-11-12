using System.ComponentModel.DataAnnotations.Schema;

namespace ScannerKeyHunt.Data.Entities
{
    public class TokenAuth : BaseModel
    {
        [ForeignKey("User")]
        public virtual Guid UserId { get; set; }

        public string? RefreshToken { get; set; }

        public bool IsActive { get; set; }

        public DateTime? ExpirationDate { get; set; }
    }
}
