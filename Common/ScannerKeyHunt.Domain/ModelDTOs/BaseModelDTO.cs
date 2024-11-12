namespace ScannerKeyHunt.Domain.ModelDTOs
{
    public class BaseModelDTO
    {
        public BaseModelDTO()
        {
            UUID = UUID == null ? Guid.NewGuid() : UUID;
            CreationDate = CreationDate == null ? DateTime.UtcNow : CreationDate;
            UpdateDate = UpdateDate == null ? DateTime.UtcNow : UpdateDate;
        }

        public BaseModelDTO(long id, DateTime? deletionDate)
        {
            Id = id;
            UUID = UUID == null ? Guid.NewGuid() : UUID;
            CreationDate = CreationDate == null ? DateTime.UtcNow : CreationDate;
            UpdateDate = UpdateDate == null ? DateTime.UtcNow : UpdateDate;
            DeletionDate = deletionDate;
        }

        public long Id { get; set; }
        public Guid? UUID { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? DeletionDate { get; set; }
    }
}
