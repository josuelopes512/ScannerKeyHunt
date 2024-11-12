namespace ScannerKeyHunt.Domain.ModelDTOs
{
    public class EmailModelDTO : BaseModelDTO
    {
        public EmailModelDTO() : base()
        {
            UUID = UUID == null ? Guid.NewGuid() : UUID;
            CreationDate = CreationDate == null ? DateTime.UtcNow : CreationDate;
            UpdateDate = UpdateDate == null ? DateTime.UtcNow : UpdateDate;
            To = new List<EmailAddressModelDTO>();
            Cc = new List<EmailAddressModelDTO>();
            Bcc = new List<EmailAddressModelDTO>();
        }

        public EmailModelDTO(
            string subject,
            string body,
            string body2,
            string body3,
            bool isHtmlBody,
            List<EmailAddressModelDTO> to,
            List<EmailAddressModelDTO> cc,
            List<EmailAddressModelDTO> bcc,
            long id,
            DateTime? deletionDate
        ) : base(id, deletionDate)
        {
            Id = id;
            UUID = UUID == null ? Guid.NewGuid() : UUID;
            CreationDate = CreationDate == null ? DateTime.UtcNow : CreationDate;
            UpdateDate = UpdateDate == null ? DateTime.UtcNow : UpdateDate;
            DeletionDate = deletionDate;

            Subject = subject;
            Body = body;
            Body2 = body2;
            Body3 = body3;
            IsHtmlBody = isHtmlBody;
            To = to;
            Cc = cc;
            Bcc = bcc;
        }

        public virtual string Subject { get; set; }
        public virtual string Body { get; set; }
        public virtual string Body2 { get; set; }
        public virtual string Body3 { get; set; }
        public virtual bool IsHtmlBody { get; set; }
        public virtual List<EmailAddressModelDTO> To { get; set; }
        public virtual List<EmailAddressModelDTO> Cc { get; set; }
        public virtual List<EmailAddressModelDTO> Bcc { get; set; }
    }
}
