using ScannerKeyHunt.Data.Enums;

namespace ScannerKeyHunt.Domain.ModelDTOs
{
    public class UserModelDTO : BaseDateModelDTO
    {
        public UserModelDTO() : base()
        {
            Id = Id == null ? Guid.NewGuid() : Id;
            CreationDate = CreationDate == null ? DateTime.UtcNow : CreationDate;
            UpdateDate = UpdateDate == null ? DateTime.UtcNow : UpdateDate;
        }

        public UserModelDTO(string email, string username, string password, RoleEnum role, bool isActive, DateTime? deletionDate) : base(deletionDate)
        {
            Id = Id == null ? Guid.NewGuid() : Id;
            CreationDate = CreationDate == null ? DateTime.UtcNow : CreationDate;
            UpdateDate = UpdateDate == null ? DateTime.UtcNow : UpdateDate;
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
