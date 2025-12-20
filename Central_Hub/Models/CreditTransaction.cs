using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Central_Hub.Models
{
    public class CreditTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        [Display(Name = "Transaction Type")]
        public CreditTransactionType TransactionType { get; set; }

        [Display(Name = "Credits Amount")]
        public int CreditsAmount { get; set; }

        [Display(Name = "Transaction Date")]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Expiry Date")]
        public DateTime ExpiryDate { get; set; }

        [Display(Name = "Reference Number")]
        [StringLength(50)]
        public string? ReferenceNumber { get; set; }

        [Display(Name = "Amount Paid")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? AmountPaid { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        // Navigation Property
        public virtual ClientCompany? Company { get; set; }
        public int ClientInstanceId { get; set; }
        public virtual ClientInstance? ClientInstance { get; set; }
    }

    public enum CreditTransactionType
    {
        [Display(Name = "Purchase")]
        Purchase,

        [Display(Name = "Bonus Credits")]
        Bonus,

        [Display(Name = "Refund")]
        Refund,

        [Display(Name = "Adjustment")]
        Adjustment,

        [Display(Name = "Expired")]
        Expired
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

        public int ClientInstanceId { get; set; }
        public virtual ClientInstance? ClientInstance { get; set; }
    }
}
