namespace ScannerKeyHunt.Domain.ModelDTOs
{
    public class EmailAddressModelDTO : BaseModelDTO
    {
        public EmailAddressModelDTO() : base()
        {
            UUID = UUID == null ? Guid.NewGuid() : UUID;
            CreationDate = CreationDate == null ? DateTime.UtcNow : CreationDate;
            UpdateDate = UpdateDate == null ? DateTime.UtcNow : UpdateDate;
        }

        public EmailAddressModelDTO(
            string name,
            string address,
            List<EmailModelDTO> emailsTo,
            List<EmailModelDTO> emailsCc,
            List<EmailModelDTO> emailsBcc,
            long id,
            DateTime? deletionDate
        ) : base(id, deletionDate)
        {
            Id = id;
            UUID = UUID == null ? Guid.NewGuid() : UUID;
            CreationDate = CreationDate == null ? DateTime.UtcNow : CreationDate;
            UpdateDate = UpdateDate == null ? DateTime.UtcNow : UpdateDate;
            DeletionDate = deletionDate;

            Name = name;
            Address = address;
            EmailsTo = emailsTo;
            EmailsCc = emailsCc;
            EmailsBcc = emailsBcc;
        }

        public virtual string Name { get; set; }
        public virtual string Address { get; set; }
        public virtual List<EmailModelDTO> EmailsTo { get; set; }
        public virtual List<EmailModelDTO> EmailsCc { get; set; }
        public virtual List<EmailModelDTO> EmailsBcc { get; set; }
    }
}
