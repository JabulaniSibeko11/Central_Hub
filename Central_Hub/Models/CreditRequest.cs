using System.ComponentModel.DataAnnotations;

namespace Central_Hub.Models
{
    public class CreditRequest
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        public int RequestedCredits { get; set; }

        public string? Reason { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        public CreditRequestStatus Status { get; set; } = CreditRequestStatus.Pending;

        public DateTime? ProcessedDate { get; set; }

        public string? ProcessedBy { get; set; }

        public string? Notes { get; set; }

        public decimal? AmountToPay { get; set; } 

        // Navigation
        public virtual ClientCompany Company { get; set; } = null!;
    }
}

public enum CreditRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Paid = 3
}

public class CreditRequestDto
{
    public int RequestedCredits { get; set; }
    public string? Reason { get; set; }
}
