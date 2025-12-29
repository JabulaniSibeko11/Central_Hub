using System.ComponentModel.DataAnnotations;

namespace Central_Hub.Models
{
  
        public class DemoRequest
        {
            [Key]
            public int DemoRequestId { get; set; }

            [Required(ErrorMessage = "Company name is required")]
            [StringLength(200)]
            [Display(Name = "Company/Organization Name")]
            public string? CompanyName { get; set; }

            [Required(ErrorMessage = "Registration number is required")]
            [StringLength(50)]
            [Display(Name = "Registration/Entity Number")]
            public string? RegistrationNumber { get; set; }

            [Display(Name = "Organization Type")]
            public CompanyType OrganizationType { get; set; }

            [Required(ErrorMessage = "Contact person name is required")]
            [StringLength(150)]
            [Display(Name = "Contact Person Name")]
            public string? ContactPersonName { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Please enter a valid email")]
            [StringLength(150)]
            [Display(Name = "Email Address")]
            public string? Email { get; set; }

            [Required(ErrorMessage = "Phone number is required")]
            [Phone]
            [Display(Name = "Phone Number")]
            public string? PhoneNumber { get; set; }

            [StringLength(100)]
            [Display(Name = "Job Title/Position")]
            public string? JobTitle { get; set; }

            [StringLength(100)]
            [Display(Name = "Department")]
            public string? Department { get; set; }

            [Required(ErrorMessage = "Email domain is required")]
            [StringLength(100)]
            [Display(Name = "Organization Email Domain")]
            [RegularExpression(@"^[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter a valid domain (e.g., company.co.za)")]
            public string? OrganizationEmailDomain { get; set; }

            [Display(Name = "Estimated Number of Employees")]
            [Range(1, 100000, ErrorMessage = "Please enter a valid number")]
            public int? EstimatedEmployeeCount { get; set; }

            [StringLength(100)]
            [Display(Name = "Province/Region")]
            public string? Province { get; set; }

            [Display(Name = "Preferred Demo Date")]
            [DataType(DataType.Date)]
            public DateTime? PreferredDemoDate { get; set; }

            [Display(Name = "Preferred Time")]
            public string? PreferredTime { get; set; }

            [StringLength(1000)]
            [Display(Name = "Additional Information / Questions")]
            [DataType(DataType.MultilineText)]
            public string? AdditionalInfo { get; set; }

            [Display(Name = "How did you hear about us?")]
            public string? HearAboutUs { get; set; }

            // Request Status
            [Display(Name = "Status")]
            public DemoRequestStatus Status { get; set; } = DemoRequestStatus.Pending;

            [Display(Name = "Request Date")]
            public DateTime RequestDate { get; set; } = DateTime.UtcNow;

            [Display(Name = "Demo Scheduled Date")]
            public DateTime? DemoScheduledDate { get; set; }

            [Display(Name = "Demo Completed Date")]
            public DateTime? DemoCompletedDate { get; set; }

            [Display(Name = "Assigned Sales Rep")]
            public string? AssignedSalesRep { get; set; }

            [StringLength(500)]
            [Display(Name = "Internal Notes")]
            [DataType(DataType.MultilineText)]
            public string? InternalNotes { get; set; }

            // Conversion tracking
            [Display(Name = "Converted to Client")]
            public bool ConvertedToClient { get; set; } = false;

            [Display(Name = "Conversion Date")]
            public DateTime? ConversionDate { get; set; }

            public int? ConvertedCompanyId { get; set; }
        //public int? ClientInstanceId { get; set; }
        //public virtual ClientInstance? ClientInstance { get; set; }
    }

        public enum DemoRequestStatus
        {
            [Display(Name = "Pending Review")]
            Pending,

            [Display(Name = "Demo Scheduled")]
            Scheduled,

            [Display(Name = "Demo Completed")]
            Completed,

            [Display(Name = "Follow-up Required")]
            FollowUp,

            [Display(Name = "Converted to Client")]
            Converted,

            [Display(Name = "Not Interested")]
            NotInterested,

            [Display(Name = "Cancelled")]
            Cancelled
        }
    }




