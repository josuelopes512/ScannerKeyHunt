using ScannerKeyHunt.Data.Enums;

namespace ScannerKeyHunt.Data.DTOs
{
    public class UserDTO : BaseDateDTO
    {
        public UserDTO() : base()
        {
            Id = this.Id == null ? Guid.NewGuid() : this.Id;
            CreationDate = this.CreationDate == null ? DateTime.UtcNow : this.CreationDate;
            UpdateDate = this.UpdateDate == null ? DateTime.UtcNow : this.UpdateDate;
        }

        public UserDTO(string email, string username, string password, RoleEnum role, bool isActive, DateTime? deletionDate) : base(deletionDate)
        {
            Id = this.Id == null ? Guid.NewGuid() : this.Id;
            CreationDate = this.CreationDate == null ? DateTime.UtcNow : this.CreationDate;
            UpdateDate = this.UpdateDate == null ? DateTime.UtcNow : this.UpdateDate;
            DeletionDate = deletionDate;

            Email = email;
            UserName = username;
            Password = password;
            Role = role;
            IsActive = isActive;
        }

        public Guid? Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public RoleEnum Role { get; set; }
        public bool IsActive { get; set; }
    }
}
