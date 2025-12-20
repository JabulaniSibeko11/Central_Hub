using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Central_Hub.Models
{
    public class SyncLog
    {
        [Key]
        public int SyncLogId { get; set; }

        [Required]
        [ForeignKey("ClientInstance")]
        public int ClientInstanceId { get; set; }

        [Required]
        [Display(Name = "Sync Date")]
        public DateTime SyncDate { get; set; }

        [Required]
        [Display(Name = "Sync Type")]
        public SyncType SyncType { get; set; }

        [Display(Name = "Success")]
        public bool Success { get; set; }

        [StringLength(1000)]
        [Display(Name = "Error Message")]
        public string? ErrorMessage { get; set; }

        [StringLength(500)]
        [Display(Name = "Details")]
        public string? Details { get; set; }

        // Navigation Properties
        public virtual ClientInstance? ClientInstance { get; set; }
    }

    public enum SyncType
    {
        LicenseCheck,
        CreditUpdate,
        VerificationAPICall,
        StatusUpdate
    }
}
