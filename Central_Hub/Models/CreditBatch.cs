using System.ComponentModel.DataAnnotations;

namespace Central_Hub.Models
{
    public class CreditBatch
    {
        [Key]
        public int BatchId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        public int OriginalAmount { get; set; }     

        public int RemainingAmount { get; set; }   

        [Required]
        public DateTime LoadDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddMonths(12); //Expiry after 12 months by default

        public string? PurchaseReference { get; set; } 
        public string? Notes { get; set; }

        // Navigation
        public virtual ClientCompany Company { get; set; } = null!;
        public virtual ICollection<CreditTransaction> Transactions { get; set; }
    }
}
