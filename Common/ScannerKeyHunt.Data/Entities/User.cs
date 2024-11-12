using Microsoft.AspNetCore.Identity;
using ScannerKeyHunt.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace ScannerKeyHunt.Data.Entities
{
    public class User : IdentityUser
    {
        public User()
        {
            this.Id = base.Id;
            this.SecurityStamp = base.SecurityStamp;
            this.ConcurrencyStamp = Guid.NewGuid().ToString();
            this.CreationDate = DateTime.UtcNow;
            this.UpdateDate = DateTime.UtcNow;
            this.CreationUserId = Guid.Empty;
            this.UpdateUserId = Guid.Empty;
        }

        [Required]
        public RoleEnum Role { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public Guid CreationUserId { get; set; } = Guid.Empty;

        public Guid UpdateUserId { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime? DeletionDate { get; set; }
    }
}
