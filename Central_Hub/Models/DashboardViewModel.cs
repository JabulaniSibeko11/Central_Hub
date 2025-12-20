namespace Central_Hub.Models
{
    public class DashboardViewModel
    {
        public int TotalClients { get; set; }
        public int ActiveClients { get; set; }
        public int TrialClients { get; set; }
        public int ExpiredClients { get; set; }
        public int ExpiringSoonClients { get; set; }

        public int TotalDemoRequests { get; set; }
        public int PendingDemoRequests { get; set; }
        public int ScheduledDemos { get; set; }
        public int CompletedDemos { get; set; }
        public int ConvertedDemos { get; set; }
        public decimal ConversionRate { get; set; }

        public int TotalCreditsInCirculation { get; set; }
        public int TotalCreditsPurchased { get; set; }
        public int TotalCreditsUsed { get; set; }

        public decimal MonthlyRevenue { get; set; }
        public decimal YearlyRevenue { get; set; }

        public List<ClientCompany> RecentClients { get; set; } = new List<ClientCompany>();
        public List<DemoRequest> RecentDemoRequests { get; set; } = new List<DemoRequest>();
        public List<CreditTransaction> RecentCreditTransactions { get; set; } = new List<CreditTransaction>();

        public List<ClientCompany> ExpiringLicenses { get; set; } = new List<ClientCompany>();
        public List<ClientCompany> LowCreditClients { get; set; } = new List<ClientCompany>();

    }
}
