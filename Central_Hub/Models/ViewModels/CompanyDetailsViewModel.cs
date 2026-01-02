namespace Central_Hub.Models.ViewModels
{
    public class CompanyDetailsViewModel
    {
        public ClientCompany Company { get; set; }
        public int AvailableCredits { get; set; }
        public int TotalCreditsPurchased { get; set; }
        public int TotalCreditsUsed { get; set; }
    }
}
