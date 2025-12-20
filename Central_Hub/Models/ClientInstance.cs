using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Central_Hub.Models
{
    public class ClientInstance
    {
        [Key]
        public int ClientInstanceId { get; set; }

        [Required(ErrorMessage = "Company name is required")]
        [StringLength(200)]
        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

        [Required(ErrorMessage = "Region is required")]
        [StringLength(100)]
        public string? Region { get; set; }

        [StringLength(500)]
        [Display(Name = "Company Address")]
        public string? CompanyAddress { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(200)]
        [Display(Name = "Company Email")]
        public string? CompanyEmail { get; set; }

        [Phone]
        [StringLength(20)]
        [Display(Name = "Company Phone")]
        public string? CompanyPhone { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Email Domain")]
        public string? EmailDomain { get; set; }

        // Administrator Details
        [Required(ErrorMessage = "Administrator name is required")]
        [StringLength(200)]
        [Display(Name = "Administrator Name")]
        public string? AdminName { get; set; }

        [Required(ErrorMessage = "Administrator email is required")]
        [EmailAddress]
        [StringLength(200)]
        [Display(Name = "Administrator Email")]
        public string? AdminEmail { get; set; }

        [Phone]
        [StringLength(20)]
        [Display(Name = "Administrator Phone")]
        public string? AdminPhone { get; set; }

        [StringLength(100)]
        [Display(Name = "Administrator Department")]
        public string? AdminDepartment { get; set; }

        // License Information
        [Required]
        [StringLength(50)]
        [Display(Name = "License Key")]
       
        public string? LicenseKey { get; set; }

        [Required]
        [Display(Name = "License Issue Date")]
        public DateTime LicenseIssueDate { get; set; }

        [Required]
        [Display(Name = "License Expiry Date")]
        public DateTime LicenseExpiryDate { get; set; }

        [Display(Name = "License Status")]
        public LicenseStatus Status { get; set; }

        // Credit Information
        [Display(Name = "Current Credit Balance")]
        public int CurrentCreditBalance { get; set; }

        [Display(Name = "Total Credits Purchased")]
        public int TotalCreditsPurchased { get; set; }

        // System Information
        [StringLength(100)]
        [Display(Name = "Instance Server URL")]
        public string? InstanceServerUrl { get; set; }

        [Display(Name = "Last Sync Date")]
        public DateTime? LastSyncDate { get; set; }

        [Display(Name = "Date Created")]
        public DateTime DateCreated { get; set; }

        [Display(Name = "Date Modified")]
        public DateTime DateModified { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        public bool IsActive { get; set; }

        // Navigation Properties
        public virtual ICollection<CreditTransaction>? CreditTransactions { get; set; }
        public virtual ICollection<LicenseRenewal>? LicenseRenewals { get; set; }
        public virtual ICollection<SyncLog>? SyncLogs { get; set; }

        // Computed Properties
        [NotMapped]
        public int DaysUntilExpiry
        {
            get
            {
                return (LicenseExpiryDate - DateTime.Now).Days;
            }
        }

        [NotMapped]
        public bool IsExpiringSoon
        {
            get
            {
                return DaysUntilExpiry <= 30 && DaysUntilExpiry > 0;
            }
        }

        [NotMapped]
        public bool IsExpired
        {
            get
            {
                return DateTime.Now > LicenseExpiryDate;
            }
        }


    }


   
}





