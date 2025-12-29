namespace Central_Hub.Models.DTO
{
    public class LicenseCheckResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public int MaxUsers { get; set; }
        public string CompanyName { get; set; } = string.Empty;
    }
}
