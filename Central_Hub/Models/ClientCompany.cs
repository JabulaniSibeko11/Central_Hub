using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace Central_Hub.Models
{
    public class ClientCompany
    {
        [Key]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "Company name is required")]
        [StringLength(200)]
        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

        [Required(ErrorMessage = "Registration number is required")]
        [StringLength(50)]
        [Display(Name = "Registration Number")]
        public string? RegistrationNumber { get; set; }

        [Required(ErrorMessage = "Email domain is required")]
        [StringLength(100)]
        [Display(Name = "Email Domain")]
        [RegularExpression(@"^[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter a valid domain (e.g., company.co.za)")]
        public string? EmailDomain { get; set; }

        [StringLength(200)]
        [Display(Name = "Physical Address")]
        public string? PhysicalAddress { get; set; }

        [StringLength(100)]
        [Display(Name = "City")]
        public string? City { get; set; }

        [StringLength(100)]
        [Display(Name = "Province/Region")]
        public string? Province { get; set; }

        [StringLength(10)]
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Country")]
        public string Country { get; set; } = "South Africa";

        [Phone]
        [Display(Name = "Contact Number")]
        public string? ContactNumber { get; set; }

        [Display(Name = "Company Type")]
        public CompanyType CompanyType { get; set; }

        [Display(Name = "Number of Employees")]
        public int? EstimatedEmployeeCount { get; set; }

        // License Information
        [Required]
        [StringLength(50)]
        [Display(Name = "License Key")]
        public string? LicenseKey { get; set; }

        [Display(Name = "License Issue Date")]
        public DateTime LicenseIssueDate { get; set; }

        [Display(Name = "License Expiry Date")]
        public DateTime LicenseExpiryDate { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Status")]
        public LicenseStatus LicenseStatus { get; set; } = LicenseStatus.Trial;

        // Credit Information
        //[Display(Name = "Current Credit Balance")]
        //public int CurrentCreditBalance { get; set; } = 0;

        //[Display(Name = "Total Credits Purchased")]
        //public int TotalCreditsPurchased { get; set; } = 0;

        //[Display(Name = "Total Credits Used")]
        //public int TotalCreditsUsed { get; set; } = 0;

        // Timestamps
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Last Modified Date")]
        public DateTime? LastModifiedDate { get; set; }

        [Display(Name = "Last Sync Date")]
        public DateTime? LastSyncDate { get; set; }

        // Navigation Properties
        public virtual CompanyAdministrator? Administrator { get; set; }
        public virtual ICollection<CreditBatch> CreditBatches { get; set; } = new List<CreditBatch>();
        public virtual ICollection<CreditTransaction> CreditTransactions { get; set; } = new List<CreditTransaction>();
        public virtual ICollection<LicenseRenewal>? LicenseRenewals { get; set; } = new List<LicenseRenewal>();

        // Computed Properties
        [NotMapped]
        public int DaysUntilExpiry => (LicenseExpiryDate - DateTime.UtcNow).Days;

        [NotMapped]
        public bool IsExpiringSoon => DaysUntilExpiry <= 30 && DaysUntilExpiry > 0;

        [NotMapped]
        public bool IsExpired => LicenseExpiryDate < DateTime.UtcNow;
    }
    public enum CompanyType
    {
        [Display(Name = "National Government")]
        NationalGovernment,

        [Display(Name = "Provincial Government")]
        ProvincialGovernment,

        [Display(Name = "Municipality - Metro")]
        MunicipalityMetro,

        [Display(Name = "Municipality - Local")]
        MunicipalityLocal,

        [Display(Name = "Municipality - District")]
        MunicipalityDistrict,

        [Display(Name = "State-Owned Enterprise")]
        StateOwnedEnterprise,

        [Display(Name = "Public Entity")]
        PublicEntity,

        [Display(Name = "Other Public Sector")]
        OtherPublicSector
    }

    public enum LicenseStatus
    {
        [Display(Name = "Trial")]
        Trial = 0,

        [Display(Name = "Active")]
        Active = 1,

        [Display(Name = "Expiring Soon")]
        ExpiringSoon = 2,

        [Display(Name = "Expired")]
        Expired = 3,

        [Display(Name = "Suspended")]
        Suspended = 4,

        [Display(Name = "Cancelled")]
        Cancelled = 5
    }
}
