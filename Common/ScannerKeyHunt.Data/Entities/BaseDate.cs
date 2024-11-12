namespace ScannerKeyHunt.Data.Entities
{
    public class BaseDate
    {
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        public DateTime UpdateDate { get; set; } = DateTime.UtcNow;

        public DateTime? DeletionDate { get; set; }
    }
}
