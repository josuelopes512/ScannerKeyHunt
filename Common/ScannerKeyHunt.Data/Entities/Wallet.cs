using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScannerKeyHunt.Data.Entities
{
    public class Wallet
    {
        [ForeignKey("User")]
        public virtual Guid UserId { get; set; }
        [MaxLength(100)]
        public string Address { get; set; }
        public string PrivateKey { get; set; }
        public decimal Amount { get; set; }
        public decimal CreditLock { get; set; }
        public decimal DebitLock { get; set; }
        public long Version { get; set; }
    }
}
