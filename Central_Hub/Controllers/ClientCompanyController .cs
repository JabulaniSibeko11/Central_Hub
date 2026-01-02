using Central_Hub.Data;
using Central_Hub.Models;
using Central_Hub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Policy;

namespace Central_Hub.Controllers
{
    public class ClientCompanyController : Controller
    {
        public readonly Central_HubDbContext _Db;
        public readonly ILicenseService _LS;
        public ClientCompanyController(Central_HubDbContext Db, ILicenseService LS)
        {
            _Db = Db;
            _LS = LS;
        }
        public async Task<IActionResult> Index(string searchTerm = "", string status = "all")
        {
            var query = _Db.ClientCompanies.Include(c => c.Administrator).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.CompanyName.Contains(searchTerm) || c.EmailDomain.Contains(searchTerm) || c.LicenseKey.Contains(searchTerm));
            }

            if (status != "all" && Enum.TryParse<LicenseStatus>(status, out var statusEnum))
            {
                query = query.Where(c => c.LicenseStatus == statusEnum);
            }

            var companies = await query.OrderByDescending(c => c.CreatedDate).ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedStatus = status;
            return View(companies);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {

            var company = await _Db.ClientCompanies
                .Include(c => c.Administrator)
                .Include(c => c.CreditTransactions)
                .Include(c => c.LicenseRenewals)
                .FirstOrDefaultAsync(c => c.CompanyId == id);

            if (company == null)
            {
                return NotFound();
            }


            return View(company);

        }

        [HttpGet]
        public IActionResult CreateFromDemo()
        {


            var model = new ClientCompany
            {
                CompanyName = TempData["CompanyName"]?.ToString(),
                RegistrationNumber = TempData["RegistrationNumber"]?.ToString(),
                EmailDomain = TempData["EmailDomain"]?.ToString(),
                Province = TempData["Province"]?.ToString(),

            };

            if (int.TryParse(TempData["EstimatedEmployeeCount"]?.ToString(), out int empCount))
            {

                model.EstimatedEmployeeCount = empCount;
            }

            if (Enum.TryParse<CompanyType>(TempData["CompanyType"]?.ToString(), out var companyType))
            {
                model.CompanyType = companyType;
            }

            var admin = new CompanyAdministrator
            {
                FirstName = TempData["AdminFullName"]?.ToString(),
                Surname = TempData["AdminSurname"]?.ToString(),
                Email = TempData["AdminEmail"]?.ToString(),
                PhoneNumber = TempData["AdminphoneNumber"]?.ToString(),
                JobTitle = TempData["AdminJobTitle"]?.ToString(),
                Department = TempData["AdminDepartment"]?.ToString(),
            };
            ViewBag.Administrator = admin;
            ViewBag.DemoRequestId = TempData["DemoRequestId"];
            return View();
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View(new ClientCompany());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientCompany company, CompanyAdministrator admin)
        {
          
                var existingCompany = await _Db.ClientCompanies.FirstOrDefaultAsync(c => c.EmailDomain == company.EmailDomain);

                if (existingCompany != null) {
                    ModelState.AddModelError("EmailDomain", "A company with this email domain already exists.");
                    return View(company);
                }


                company.LicenseKey = _LS.GenerateLicenseKey(company);
                company.LicenseIssueDate = DateTime.UtcNow;
                company.LicenseExpiryDate = _LS.CalculateLicenseExpiryDate();
                company.LicenseStatus = LicenseStatus.Trial;
                company.CreatedDate = DateTime.UtcNow;
                company.IsActive = false;

            company.Administrator = admin;

                _Db.ClientCompanies.Add(company);
                await _Db.SaveChangesAsync();

                if (int.TryParse(Request.Form["DemoRequestId"], out int demoRequestId) && demoRequestId > 0) {
                    var demoRequest = await _Db.DemoRequests.FindAsync(demoRequestId);
                    if (demoRequest != null)
                    {
                        demoRequest.ConvertedToClient = true;
                        demoRequest.ConversionDate = DateTime.UtcNow;
                        demoRequest.ConvertedCompanyId = company.CompanyId;
                        demoRequest.Status = DemoRequestStatus.Converted;
                        await _Db.SaveChangesAsync();
                    }
                }
                TempData["SuccessMessage"] = $"Client company created successfully. License Key: {company.LicenseKey}";
                return RedirectToAction(nameof(Details), new { id = company.CompanyId });
            
            //return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCredit(int companyId, int creditAmount, decimal amountPaid, string notes)
        {

            var company = await _Db.ClientCompanies.FindAsync(companyId);
            if (company == null)
            {
                return NotFound();
            }

            var transaction = new CreditTransaction
            {
                CompanyId = companyId,
                TransactionType = CreditTransactionType.Purchase,
                CreditsAmount = creditAmount,
                AmountPaid = amountPaid,
                TransactionDate = DateTime.UtcNow,
                ExpiryDate = _LS.CalculateCreditExpiryDate(),
                ReferenceNumber = $"CR-{DateTime.UtcNow.Ticks}",
                CreatedBy = "Admin",
                Notes = notes
            };

            company.CurrentCreditBalance += creditAmount;
            company.TotalCreditsPurchased += creditAmount;

            _Db.CreditTransactions.Add(transaction);
            await _Db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Successfully added {creditAmount} credits to {company.CompanyName}.";
            return RedirectToAction(nameof(Details), new { id = companyId });


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenewLicense(int companyId, decimal amountPaid, string paymentReference, string notes)
        {
            var company = await _Db.ClientCompanies.FindAsync(companyId);
            if (company == null)
            {
                return NotFound();
            }
            var renewal = new LicenseRenewal
            {
                CompanyId = companyId,
                PreviousExpiryDate = company.LicenseExpiryDate,
                RenewalDate = DateTime.UtcNow,
                AmountPaid = amountPaid,
                PaymentReference = paymentReference,
                ProcessedBy = "Admin",
                Notes = notes
            };

            company.LicenseExpiryDate = renewal.NewExpiryDate;
            company.LicenseStatus = LicenseStatus.Active;
            company.LastModifiedDate = DateTime.UtcNow;

            _Db.LicenseRenewals.Add(renewal);
            await _Db.SaveChangesAsync();



            TempData["SuccessMessage"] = $"License renewed successfully until {renewal.NewExpiryDate:dd MMMM yyyy}.";
            return RedirectToAction(nameof(Details), new { id = companyId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLicenseStatus(int companyId, LicenseStatus status) { 
         var company = await _Db.ClientCompanies.FindAsync(companyId);
            if (company == null) { 
            return NotFound();  
            }
             company.LicenseStatus = status;
            company.LastModifiedDate = DateTime.UtcNow;
            await _Db.SaveChangesAsync();

            TempData["SuccessMessage"] = "License status updated successfully.";
            return RedirectToAction(nameof(Details), new { id = companyId });
        }
    }
}
