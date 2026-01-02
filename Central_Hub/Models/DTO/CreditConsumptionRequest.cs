using System.ComponentModel.DataAnnotations;

namespace Central_Hub.Models.DTO
{
    public class CreditConsumptionRequest
    {

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Credits to consume must be at least 1")]
        public int CreditsToConsume { get; set; }

        public string? Reason { get; set; }
    }
}
