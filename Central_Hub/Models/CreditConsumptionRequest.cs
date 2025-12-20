using System.ComponentModel.DataAnnotations;

namespace Central_Hub.Models
{
    public class CreditConsumptionRequest
    {
        [Required]
        public string LicenseKey { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Credits to consume must be at least 1")]
        public int CreditsToConsume { get; set; }
    }
}
