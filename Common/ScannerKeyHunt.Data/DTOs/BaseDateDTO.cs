namespace ScannerKeyHunt.Data.DTOs
{
    public class BaseDateDTO
    {
        public BaseDateDTO()
        {
            CreationDate = this.CreationDate == null ? DateTime.UtcNow : this.CreationDate;
            UpdateDate = this.UpdateDate == null ? DateTime.UtcNow : this.UpdateDate;
            DeletionDate = null;
        }

        public BaseDateDTO(DateTime? deletionDate)
        {
            CreationDate = this.CreationDate == null ? DateTime.UtcNow : this.CreationDate;
            UpdateDate = this.UpdateDate == null ? DateTime.UtcNow : this.UpdateDate;
            DeletionDate = deletionDate;
        }

        public DateTime? CreationDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? DeletionDate { get; set; }
    }
}
