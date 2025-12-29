namespace Central_Hub.Models.DTO
{
    public class ActivationResponse
    {
        public bool isValid { get; set; }
        public int companyId { get; set; }
        public string companyName { get; set; }
        public string emailDomain { get; set; }
        public string message { get; set; }

        public DateTime? ExpiryDate { get; set; }
        public int daysUntilExpiry { get; set; }
        public int isExpired { get; set; }


    }
}
