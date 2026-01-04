namespace Central_Hub.Models.DTO
{
    public class CreditRequestResponse
    {
        public int RequestId { get; set; }
        public string RequestReference { get; set; }
        public int RequestedCredits { get; set; }
        public DateTime RequestDate { get; set; }
        public string? Status { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string? ProcessedBy { get; set; }
        public string? RequestedBy { get; set; }
        public string? Notes { get; set; }
    }
}
