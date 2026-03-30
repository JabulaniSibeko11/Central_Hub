using System.ComponentModel.DataAnnotations;

namespace Central_Hub.Models
{
    /// <summary>Immutable API audit record — never updated or deleted.</summary>
    public class ApiAuditLog
    {
        [Key]
        public long LogId { get; set; }

        public int? CompanyId { get; set; }

        [MaxLength(10)]
        public string HttpMethod { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Path { get; set; } = string.Empty;

        public int StatusCode { get; set; }

        public bool SignatureValid { get; set; }

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [MaxLength(300)]
        public string? UserAgent { get; set; }

        public long DurationMs { get; set; }

        public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;

        [MaxLength(200)]
        public string? FailureReason { get; set; }
    }
}
