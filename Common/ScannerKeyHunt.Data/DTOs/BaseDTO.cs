namespace ScannerKeyHunt.Data.DTOs
{
    public class BaseDTO
    {
        public BaseDTO()
        {
            UUID = this.UUID == null ? Guid.NewGuid() : this.UUID;
            CreationDate = this.CreationDate == null ? DateTime.UtcNow : this.CreationDate;
            UpdateDate = this.UpdateDate == null ? DateTime.UtcNow : this.UpdateDate;
        }

        public BaseDTO(long id, DateTime? deletionDate)
        {
            Id = id;
            UUID = this.UUID == null ? Guid.NewGuid() : this.UUID;
            CreationDate = this.CreationDate == null ? DateTime.UtcNow : this.CreationDate;
            UpdateDate = this.UpdateDate == null ? DateTime.UtcNow : this.UpdateDate;
            DeletionDate = deletionDate;
        }

        public long Id { get; set; }
        public Guid? UUID { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? DeletionDate { get; set; }
    }
}
