using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Central_Hub.Models
{
    public class CreditTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        public int? BatchId { get; set; }

        public CreditTransactionType TransactionType { get; set; }

        public decimal? AmountPaid { get; set; }

        public int CreditsAmount { get; set; } 

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        public DateTime ExpiryDate { get; set; }

        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public string? CreatedBy { get; set; }

        public virtual ClientCompany Company { get; set; } = null!;
        public virtual CreditBatch? Batch { get; set; }
    }

    public enum CreditTransactionType
    {
        [Display(Name = "Purchase")]
        Purchase = 0,

        [Display(Name = "Bonus Credits")]
        Bonus = 1,

        [Display(Name = "Refund")]
        Refund = 2,

        [Display(Name = "Adjustment")]
        Adjustment = 3,

        [Display(Name = "Expired")]
        Expired = 4,

        [Display(Name = "Spend Credits")]
        Spend = 5,
    }

    public class LicenseRenewal
    {
        [Key]
        public int RenewalId { get; set; }

        [Required]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        [Display(Name = "Previous Expiry Date")]
        public DateTime PreviousExpiryDate { get; set; }

        [Display(Name = "New Expiry Date")]
        public DateTime NewExpiryDate { get; set; }

        [Display(Name = "Renewal Date")]
        public DateTime RenewalDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Amount Paid")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        [Display(Name = "Payment Method")]
        public string? PaymentMethod { get; set; }

        [Display(Name = "Payment Reference")]
        [StringLength(100)]
        public string? PaymentReference { get; set; }

        [Display(Name = "Invoice Number")]
        [StringLength(50)]
        public string? InvoiceNumber { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Processed By")]
        public string? ProcessedBy { get; set; }

        // Navigation Property
        public virtual ClientCompany? Company { get; set; }

        //public int ClientInstanceId { get; set; }
        //public virtual ClientInstance? ClientInstance { get; set; }
    }
}
