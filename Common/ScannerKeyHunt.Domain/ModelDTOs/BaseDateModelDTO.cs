namespace ScannerKeyHunt.Domain.ModelDTOs
{
    public class BaseDateModelDTO
    {
        public BaseDateModelDTO()
        {
            CreationDate = CreationDate == null ? DateTime.UtcNow : CreationDate;
            UpdateDate = UpdateDate == null ? DateTime.UtcNow : UpdateDate;
            DeletionDate = null;
        }

        public BaseDateModelDTO(DateTime? deletionDate)
        {
            CreationDate = CreationDate == null ? DateTime.UtcNow : CreationDate;
            UpdateDate = UpdateDate == null ? DateTime.UtcNow : UpdateDate;
            DeletionDate = deletionDate;
        }

        public DateTime? CreationDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? DeletionDate { get; set; }
    }
}
