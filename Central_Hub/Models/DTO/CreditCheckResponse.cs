namespace Central_Hub.Models.DTO
{
    public class CreditCheckResponse
    {
        public bool hasCredits { get; set; }
        public bool lowCreditWarning { get; set; }
        public int currentBalance { get; set; }
        public int totalPurchased { get; set; }
        public int totalUsed { get; set; }

    }
}
