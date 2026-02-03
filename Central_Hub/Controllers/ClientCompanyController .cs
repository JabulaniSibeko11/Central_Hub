using System.Security.Policy;
using Central_Hub.Data;
using Central_Hub.Models;
using Central_Hub.Models.ViewModels;
using Central_Hub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                .Include(c => c.CreditBatches)
                .FirstOrDefaultAsync(c => c.CompanyId == id);

            if (company == null)
            {
                return NotFound();
            }

            // Calculate credit stats from batches
            var now = DateTime.UtcNow;
            var availableCredits = company.CreditBatches
                .Where(b => b.ExpiryDate > now)
                .Sum(b => b.RemainingAmount);

            var totalPurchased = company.CreditBatches
                .Sum(b => b.OriginalAmount);

            var totalUsed = company.CreditBatches
                .Sum(b => b.OriginalAmount - b.RemainingAmount);




            var viewModel = new CompanyDetailsViewModel
            {
                Company = company,
                AvailableCredits = availableCredits,
                TotalCreditsPurchased = totalPurchased,
                TotalCreditsUsed = totalUsed
            };

            return View(viewModel);

        }

        [HttpGet]
        public IActionResult CreateFromDemo1()
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
        public IActionResult CreateFromDemo()
        {
            var company = new ClientCompany
            {
                CompanyName = TempData["CompanyName"]?.ToString(),
                RegistrationNumber = TempData["RegistrationNumber"]?.ToString(),
                EmailDomain = TempData["EmailDomain"]?.ToString(),
                Province = TempData["Province"]?.ToString()
            };

            if (int.TryParse(TempData["EstimatedEmployeeCount"]?.ToString(), out int empCount))
                company.EstimatedEmployeeCount = empCount;

            if (Enum.TryParse<CompanyType>(TempData["CompanyType"]?.ToString(), out var companyType))
                company.CompanyType = companyType;

            var admin = new CompanyAdministrator
            {
                FirstName = TempData["AdminFirstName"]?.ToString(),
                Surname = TempData["AdminLastName"]?.ToString(),
                Email = TempData["AdminEmail"]?.ToString(),
                PhoneNumber = TempData["AdminPhone"]?.ToString(),
                JobTitle = TempData["AdminJobTitle"]?.ToString(),
                Department = TempData["AdminDepartment"]?.ToString()
            };

            var vm = new CreateFromDemoViewModel
            {
                Company = company,
                Administrator = admin,
                DemoRequestId = TempData["DemoRequestId"] as int?
            };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromDemo(CreateFromDemoViewModel model)
        {
            //if (!ModelState.IsValid)
            //    return View(model);

            var company = model.Company;
            var admin = model.Administrator;

            var existingCompany = await _Db.ClientCompanies
                .FirstOrDefaultAsync(c => c.EmailDomain == company.EmailDomain);

            if (existingCompany != null)
            {
                ModelState.AddModelError("Company.EmailDomain",
                    "A company with this email domain already exists.");
                return View(model);
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

            if (model.DemoRequestId.HasValue)
            {
                var demoRequest = await _Db.DemoRequests.FindAsync(model.DemoRequestId.Value);
                if (demoRequest != null)
                {
                    demoRequest.ConvertedToClient = true;
                    demoRequest.ConversionDate = DateTime.UtcNow;
                    demoRequest.ConvertedCompanyId = company.CompanyId;
                    demoRequest.Status = DemoRequestStatus.Converted;
                    await _Db.SaveChangesAsync();
                }
            }

            TempData["SuccessMessage"] =
                $"Client company created successfully. License Key: {company.LicenseKey}";

            return RedirectToAction(nameof(Details), new { id = company.CompanyId });
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

            if (existingCompany != null)
            {
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

            if (int.TryParse(Request.Form["DemoRequestId"], out int demoRequestId) && demoRequestId > 0)
            {
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

            //var instance = new ClientInstance
            //{
            //    CompanyName = company.CompanyName,
            //    Region = "N/A",
            //    CompanyAddress = company.PhysicalAddress,
            //    CompanyEmail = admin.Email,
            //    CompanyPhone = admin.PhoneNumber,
            //    EmailDomain = company.EmailDomain,
            //    AdminName = admin.FullName,
            //    AdminEmail = admin.Email,
            //    AdminDepartment = admin.Department,
            //    LicenseKey = company.LicenseKey,
            //    LicenseIssueDate = company.LicenseIssueDate,
            //    LicenseExpiryDate = company.LicenseExpiryDate,
            //    Status = LicenseStatus.Suspended,
            //    CurrentCreditBalance = company.CurrentCreditBalance,
            //    TotalCreditsPurchased = company.TotalCreditsPurchased,
            //    DateCreated = company.CreatedDate,
            //    DateModified = company.CreatedDate,
            //    IsActive = false

            //};

            //_Db.ClientInstance.Add(instance);
            //await _Db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Client company created successfully. License Key: {company.LicenseKey}";
            return RedirectToAction(nameof(Details), new { id = company.CompanyId });

            //return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCredit(int companyId, int creditAmount, decimal amountPaid, string notes)
        {
            if (creditAmount <= 0)
            {
                TempData["ErrorMessage"] = "Credit amount must be greater than zero.";
                return RedirectToAction(nameof(Details), new { id = companyId });
            }

            var company = await _Db.ClientCompanies.FindAsync(companyId);

            if (company == null)
            {
                return NotFound();
            }

            var now = DateTime.UtcNow;

            // 1. Create the new credit batch
            var newBatch = new CreditBatch
            {
                CompanyId = company.CompanyId,
                OriginalAmount = creditAmount,
                RemainingAmount = creditAmount,
                LoadDate = now,
                //PurchaseReference = $"INV-{now:yyyyMMdd}-{company.CompanyId}",
                PurchaseReference = $"INV-{now.Ticks:yyyyMMdd}-{company.CompanyId}",
                Notes = notes
            };

            _Db.CreditBatches.Add(newBatch);
            await _Db.SaveChangesAsync();

            // 2. Create the audit transaction
            var transaction = new CreditTransaction
            {
                CompanyId = company.CompanyId,
                BatchId = newBatch.BatchId,
                TransactionType = CreditTransactionType.Purchase,
                CreditsAmount = creditAmount,
                AmountPaid = amountPaid,
                TransactionDate = now,
                ExpiryDate = newBatch.ExpiryDate,
                ReferenceNumber = newBatch.PurchaseReference,
                CreatedBy = "Admin",
                Notes = notes
            };

            _Db.CreditTransactions.Add(transaction);

            company.LastModifiedDate = now;

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
            var now = DateTime.UtcNow;
            var newExpiryDate = CalculateNextFebruaryExpiry(now);
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

            company.LicenseExpiryDate = newExpiryDate;
            company.LicenseStatus = LicenseStatus.Active;
            company.LastModifiedDate = DateTime.UtcNow;

            _Db.LicenseRenewals.Add(renewal);
            await _Db.SaveChangesAsync();



            TempData["SuccessMessage"] = $"License renewed successfully until {renewal.NewExpiryDate:dd MMMM yyyy}.";
            return RedirectToAction(nameof(Details), new { id = companyId });
        }
        private DateTime CalculateNextFebruaryExpiry(DateTime currentExpiry)
        {
            // Current expiry is already Feb 1 of some year
            return new DateTime(
                currentExpiry.Year + 1,
                2,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLicenseStatus(int companyId, LicenseStatus status)
        {
            var company = await _Db.ClientCompanies.FindAsync(companyId);
            if (company == null)
            {
                return NotFound();
            }
            company.LicenseStatus = status;
            company.LastModifiedDate = DateTime.UtcNow;
            await _Db.SaveChangesAsync();

            TempData["SuccessMessage"] = "License status updated successfully.";
            return RedirectToAction(nameof(Details), new { id = companyId });
        }


        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int requestId, int approvedAmount)
        {
            var request = await _Db.CreditRequests.FindAsync(requestId);
            if (request == null) return NotFound();

            request.Status = CreditRequestStatus.Approved;
            request.ProcessedDate = DateTime.UtcNow;
            request.ProcessedBy = User.Identity.Name;

            // Create the actual credit batch
            var newBatch = new CreditBatch
            {
                CompanyId = request.CompanyId,
                OriginalAmount = approvedAmount,
                RemainingAmount = approvedAmount,
                LoadDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(12),
                PurchaseReference = $"APPROVED-{request.RequestId}"
            };

            _Db.CreditBatches.Add(newBatch);

            // Log transaction
            _Db.CreditTransactions.Add(new CreditTransaction
            {
                CompanyId = request.CompanyId,
                BatchId = newBatch.BatchId,
                TransactionType = CreditTransactionType.Purchase,
                CreditsAmount = approvedAmount,
                TransactionDate = DateTime.UtcNow,
                Notes = $"Approved request {request.RequestId}"
            });

            await _Db.SaveChangesAsync();

            return RedirectToAction("CreditRequests");
        }
    }
}
