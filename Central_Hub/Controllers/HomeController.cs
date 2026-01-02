using Central_Hub.Data;
using Central_Hub.Models;
using Central_Hub.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Security.Claims;

namespace Central_Hub.Controllers
{
    public class HomeController : Controller
    {
       
        private readonly Central_HubDbContext _db;
        private readonly ILogger _logger;

        public HomeController(Central_HubDbContext db, ILogger logger = null)
        {

            _db = db;
            _logger = logger;
        }
        public IActionResult LandingPage() { 
        return View();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Login")]
        public IActionResult AdminLogin(string? returnUrl = null)
        {
            // If already authenticated, redirect to dashboard
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("CentralAdmin"))
            {
                return RedirectToAction("Index");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminLogin(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var adminUser = await _db.CentralUser
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.IsActive);

            if (adminUser == null || !VerifyPassword(model.Password, adminUser.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, adminUser.FullName ?? adminUser.Email),
                new Claim(ClaimTypes.Email, adminUser.Email),
                new Claim("AdminId", adminUser.Id.ToString()),
                new Claim(ClaimTypes.Role, "CentralAdmin")
            };

            const string CentralAdminScheme = "CentralAdminScheme";

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CentralAdminScheme
            );

            await HttpContext.SignInAsync(
                CentralAdminScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = model.RememberMe }
            );

          

            // Redirect to returnUrl or dashboard
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]

        [Route("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {

            _logger.LogInformation("Central Admin logged out");
            return RedirectToAction("AdminLogin");
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            // Use BCrypt for secure password verification
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity.Name;

            ViewBag.UserEmail = userEmail;  


            var activeClient = await _db.ClientCompanies.CountAsync(c => c.LicenseStatus == LicenseStatus.Active);
            var TrialClients = await _db.ClientCompanies.CountAsync(c => c.LicenseStatus == LicenseStatus.Trial);
            var ExpiredClients = await _db.ClientCompanies.CountAsync(c => c.LicenseStatus == LicenseStatus.Expired);
            var ExpiringSoonClients = await _db.ClientCompanies.CountAsync(c => c.LicenseStatus == LicenseStatus.ExpiringSoon);
            var PendingDemoRequests = await _db.DemoRequests.CountAsync(d => d.Status == DemoRequestStatus.Pending);
           var  ScheduledDemos = await _db.DemoRequests.CountAsync(d => d.Status == DemoRequestStatus.Scheduled);
            var CompletedDemos = await _db.DemoRequests.CountAsync(d => d.Status == DemoRequestStatus.Completed);
            var ConvertedDemos = await _db.DemoRequests.CountAsync(d => d.Status == DemoRequestStatus.Converted);

            var TotalCreditsInCirculation = await _db.CreditBatches.Where(b => b.ExpiryDate > DateTime.UtcNow).SumAsync(b => b.RemainingAmount);
            var TotalCreditsPurchased = await _db.CreditBatches.SumAsync(b => b.OriginalAmount);
            var TotalCreditsUsed = await _db.CreditBatches.SumAsync(b => b.OriginalAmount - b.RemainingAmount);

            var lowCreditCompanyIds = await _db.CreditBatches
    .Where(b => b.RemainingAmount < 50 &&
                b.RemainingAmount > 0 &&
                b.ExpiryDate > DateTime.UtcNow)
    .GroupBy(b => b.CompanyId)
    .Select(g => new
    {
        CompanyId = g.Key,
        TotalLowCredits = g.Sum(b => b.RemainingAmount)
    })
    .Where(g => g.TotalLowCredits < 50)
    .OrderBy(g => g.TotalLowCredits)
    .Select(g => g.CompanyId)
    .ToListAsync();

            // Step 2: Load full ClientCompany records for those IDs
            // Step 2: Load full companies (no ordering here)
            var LowCreditClients = await _db.ClientCompanies
                .Where(c => lowCreditCompanyIds.Contains(c.CompanyId))
                .ToListAsync();

            // Step 3: Order in memory using the original ID list
            LowCreditClients = LowCreditClients
                .OrderBy(c => lowCreditCompanyIds.IndexOf(c.CompanyId))
                .ToList();


            var model = new DashboardViewModel
            {

                TotalClients = await _db.ClientCompanies.CountAsync(),
                ActiveClients = activeClient,
                TotalDemoRequests = await _db.DemoRequests.CountAsync(),
                TrialClients = TrialClients,
                ExpiredClients = ExpiredClients,
                ExpiringSoonClients = ExpiringSoonClients,
                PendingDemoRequests = PendingDemoRequests,
                ScheduledDemos = ScheduledDemos,
                CompletedDemos = CompletedDemos,
                ConvertedDemos = ConvertedDemos,
                TotalCreditsPurchased = TotalCreditsPurchased,
                TotalCreditsUsed = TotalCreditsUsed,
                TotalCreditsInCirculation = TotalCreditsInCirculation,

                MonthlyRevenue = await CalculateMonthlyRevenue(),
                YearlyRevenue = await CalclateYearlyRevenue(),

                RecentClients= await _db.ClientCompanies
                .Include(c=>c.Administrator)
                .OrderByDescending(c=>c.CreatedDate).Take(5).ToListAsync(),

                RecentDemoRequests= await _db.DemoRequests
                .OrderByDescending(d=>d.RequestDate)
                .Take(5).ToListAsync(),

                RecentCreditTransactions = await _db.CreditTransactions
                .Include(t=>t.Company)
                .OrderByDescending(t=>t.TransactionDate)
                .Take(10).ToListAsync(),

                ExpiringLicenses= await _db.ClientCompanies
               .Where(c=>c.LicenseExpiryDate<DateTime.UtcNow.AddDays(30)
               && c.LicenseExpiryDate>DateTime.UtcNow)
               .OrderBy(t=>t.LicenseExpiryDate)
               .ToListAsync(),

                 LowCreditClients = LowCreditClients


            };
            if (model.CompletedDemos > 0)
            {
                model.ConversionRate = (decimal)model.ConvertedDemos / model.CompletedDemos * 100;
            }
            else
            {
                model.ConversionRate = 0;
            }

            return View(model);
        }
        private async Task<decimal> CalculateMonthlyRevenue() {
            var firstdayOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var creditRevenue = await _db.CreditTransactions
                .Where(t => t.TransactionDate > firstdayOfMonth && t.AmountPaid.HasValue)
                .SumAsync(t => t.AmountPaid.Value);

            var renewalRevenue = await _db.LicenseRenewals
                .Where(r => r.RenewalDate >= firstdayOfMonth)
                .SumAsync(r => r.AmountPaid);

            return creditRevenue+ renewalRevenue;
        }

        public async Task<decimal> CalclateYearlyRevenue() { 
        
        var firstdayOfYear= new DateTime(DateTime.UtcNow.Year,1,1);

            var creditRevenue = await _db.CreditTransactions
                .Where(t => t.TransactionDate >= firstdayOfYear && t.AmountPaid.HasValue)
                .SumAsync(t => t.AmountPaid.Value);

            var renewalRevenue = await _db.LicenseRenewals
                .Where(r => r.RenewalDate >= firstdayOfYear)
                .SumAsync(r => r.AmountPaid);

         return creditRevenue+renewalRevenue ;
        }

        [HttpGet]
        public async Task<IActionResult> GetClientByType() {

            var data = await _db.ClientCompanies
                    .GroupBy(c => c.CompanyType)
                    .Select(g => new
                    {
                        Type = g.Key.ToString(),
                        Count=g.Count()
                    }).ToListAsync();
            return Json(data);
        }



        [HttpGet]
        public async Task<IActionResult> GetRevenueByMonth() { 
        
        var startDate = DateTime.UtcNow.AddMonths(-11);
            var monthlyData = new List<object>();

            for (int i = 0; i < 12; i++) { 
            var monthStart= startDate.AddMonths(i);
                var monthEnd= monthStart.AddMonths(i);

                var creditRevenue = await _db.CreditTransactions
                    .Where(t => t.TransactionDate >= monthStart && t.TransactionDate < monthEnd && t.AmountPaid.HasValue)
                    .SumAsync(t => t.AmountPaid.Value);

                var renewalRevenue = await _db.LicenseRenewals
                    .Where(r => r.RenewalDate >= monthStart && r.RenewalDate < monthEnd)
                    .SumAsync(r => r.AmountPaid);

                monthlyData.Add(new
                {
                    Month = monthStart.ToString("MMM yyyy"),
                    Revenue = creditRevenue + renewalRevenue

                });
            
            }
            return Json(monthlyData);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
