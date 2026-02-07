using Central_Hub.Data;
using Central_Hub.Models;
using Central_Hub.Models.ViewModels;
using Central_Hub.Services;
using Central_Hub.Services.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Security.Policy;
using System.Text.RegularExpressions;

namespace Central_Hub.Controllers
{
    public class ClientCompanyController : Controller
    {
        public readonly Central_HubDbContext _Db;
        public readonly ILicenseService _LS;
        private readonly IEmailService _email;
        public ClientCompanyController(Central_HubDbContext Db, ILicenseService LS, IEmailService email)
        {
            _Db = Db;
            _LS = LS;
            _email = email;
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
                return NotFound();

            var now = DateTime.UtcNow;

            var availableCredits = company.CreditBatches
                .Where(b => b.ExpiryDate > now)
                .Sum(b => b.RemainingAmount);

            var totalPurchased = company.CreditBatches.Sum(b => b.OriginalAmount);
            var totalUsed = company.CreditBatches.Sum(b => b.OriginalAmount - b.RemainingAmount);

            var vm = new CompanyDetailsViewModel
            {
                Company = company,
                AvailableCredits = availableCredits,
                TotalCreditsPurchased = totalPurchased,
                TotalCreditsUsed = totalUsed,

                // ✅ show this on the Add Credit form (disabled)
                NextCreditPurchaseReference = GenerateCreditInvoiceNo(company.CompanyId)
            };

            return View(vm);
        }



        [HttpGet]
        public async Task<IActionResult> Details1(int id)
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
        public async Task<IActionResult> CreateFromDemo(CreateFromDemoViewModel model, CancellationToken ct)
        {
            var company = model.Company;
            var admin = model.Administrator;

            var existingCompany = await _Db.ClientCompanies
                .FirstOrDefaultAsync(c => c.EmailDomain == company.EmailDomain, ct);

            if (existingCompany != null)
            {
                ModelState.AddModelError("Company.EmailDomain",
                    "A company with this email domain already exists.");
                return View(model);
            }

            // License details
            company.LicenseKey = _LS.GenerateLicenseKey(company);
            company.LicenseIssueDate = DateTime.UtcNow;
            company.LicenseExpiryDate = _LS.CalculateLicenseExpiryDate();
            company.LicenseStatus = LicenseStatus.Trial;
            company.CreatedDate = DateTime.UtcNow;
            company.IsActive = false;

            // link admin (includes EmployeeNumber if you added it to the view)
            company.Administrator = admin;

            _Db.ClientCompanies.Add(company);
            await _Db.SaveChangesAsync(ct); // ✅ CompanyId now exists

            // ✅ PaymentReference: DECL-<CompanyId>-XXX
            company.PaymentReference = await GenerateUniquePaymentReferenceAsync(company.CompanyId, ct);
            await _Db.SaveChangesAsync(ct);

            // Mark demo request converted
            DemoRequest? demoRequest = null;
            if (model.DemoRequestId.HasValue)
            {
                demoRequest = await _Db.DemoRequests.FindAsync(new object[] { model.DemoRequestId.Value }, ct);
                if (demoRequest != null)
                {
                    demoRequest.ConvertedToClient = true;
                    demoRequest.ConversionDate = DateTime.UtcNow;
                    demoRequest.ConvertedCompanyId = company.CompanyId;
                    demoRequest.Status = DemoRequestStatus.Converted;
                    await _Db.SaveChangesAsync(ct);
                }
            }

            // ✅ Email: "Converted" (License Key + Payment Reference)
            // If email fails, do NOT block conversion
            try
            {
                var to = admin?.Email;

                if (!string.IsNullOrWhiteSpace(to))
                {
                    var fullName = $"{admin?.FirstName} {admin?.Surname}".Trim();
                    if (string.IsNullOrWhiteSpace(fullName)) fullName = "there";

                    var subject = "Your Declarify account is ready (License Key & Payment Reference)";

                    var body = $@"
<div style='font-family:Segoe UI,Arial,sans-serif;font-size:14px;line-height:1.55;color:#111'>
  <h2 style='margin:0 0 10px 0'>Account Created</h2>

  <p style='margin:0 0 14px 0'>Hi {fullName},</p>

  <p style='margin:0 0 14px 0'>
    Your demo request has been converted to an active client account for <b>{company.CompanyName}</b>.
  </p>

  <hr style='border:none;border-top:1px solid #e5e7eb;margin:18px 0' />

  <h3 style='margin:0 0 10px 0'>Your access details</h3>
  <p style='margin:0 0 10px 0'>
    <b>License Key:</b> {company.LicenseKey}<br/>
    <b>Payment Reference:</b> {company.PaymentReference}
  </p>

  <p style='margin:0 0 18px 0'>
    Please use this <b>Payment Reference</b> on <b>all invoices and transactions</b> going forward.
  </p>

  <hr style='border:none;border-top:1px solid #e5e7eb;margin:18px 0' />

  <h3 style='margin:0 0 10px 0'>Company details</h3>
  <table style='border-collapse:collapse;width:100%'>
    <tr><td style='padding:6px 0;width:220px;color:#374151'><b>Company Name</b></td><td style='padding:6px 0'>{company.CompanyName}</td></tr>
    <tr><td style='padding:6px 0;color:#374151'><b>Registration Number</b></td><td style='padding:6px 0'>{company.RegistrationNumber}</td></tr>
    <tr><td style='padding:6px 0;color:#374151'><b>Email Domain</b></td><td style='padding:6px 0'>{company.EmailDomain}</td></tr>
    <tr><td style='padding:6px 0;color:#374151'><b>Province</b></td><td style='padding:6px 0'>{company.Province}</td></tr>
    <tr><td style='padding:6px 0;color:#374151'><b>Organisation Type</b></td><td style='padding:6px 0'>{company.CompanyType}</td></tr>
    <tr><td style='padding:6px 0;color:#374151'><b>Estimated Employees</b></td><td style='padding:6px 0'>{(company.EstimatedEmployeeCount?.ToString() ?? "Not provided")}</td></tr>
  </table>

  <h3 style='margin:18px 0 10px 0'>Administrator</h3>
  <table style='border-collapse:collapse;width:100%'>
    <tr><td style='padding:6px 0;width:220px;color:#374151'><b>Name</b></td><td style='padding:6px 0'>{admin?.FirstName} {admin?.Surname}</td></tr>
    <tr><td style='padding:6px 0;color:#374151'><b>Email</b></td><td style='padding:6px 0'>{admin?.Email}</td></tr>
    <tr><td style='padding:6px 0;color:#374151'><b>Phone</b></td><td style='padding:6px 0'>{admin?.PhoneNumber}</td></tr>
    <tr><td style='padding:6px 0;color:#374151'><b>Job Title</b></td><td style='padding:6px 0'>{(string.IsNullOrWhiteSpace(admin?.JobTitle) ? "Not provided" : admin!.JobTitle)}</td></tr>
    <tr><td style='padding:6px 0;color:#374151'><b>Department</b></td><td style='padding:6px 0'>{(string.IsNullOrWhiteSpace(admin?.Department) ? "Not provided" : admin!.Department)}</td></tr>
    <tr><td style='padding:6px 0;color:#374151'><b>Employee Number</b></td><td style='padding:6px 0'>{(string.IsNullOrWhiteSpace(admin?.EmployeeNumber) ? "Not provided" : admin!.EmployeeNumber)}</td></tr>
  </table>

  <p style='margin:18px 0 0 0'>
    Regards,<br/>
    <b>Inspired IT Central Hub</b>
  </p>
</div>";

                    await _email.SendAsync(to, subject, body, ct);
                }
            }
            catch (Exception ex)
            {
                // Do not block conversion if email fails
                // You can replace Console with ILogger if you inject it
                Console.WriteLine(ex);
            }

            TempData["SuccessMessage"] =
                $"Client company created successfully. License Key: {company.LicenseKey} | Payment Ref: {company.PaymentReference}";

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
        public async Task<IActionResult> AddCredit(
     int companyId,
     int creditAmount,
     decimal amountPaid,
     string? notes,
     string? purchaseReference, // ignored
     IFormFile? attachment,
     CancellationToken ct)
        {
            if (creditAmount <= 0)
            {
                TempData["ErrorMessage"] = "Credit amount must be greater than zero.";
                return RedirectToAction(nameof(Details), new { id = companyId });
            }

            if (attachment == null || attachment.Length == 0)
            {
                TempData["ErrorMessage"] = "Please upload the invoice attachment (PDF) before adding credit.";
                return RedirectToAction(nameof(Details), new { id = companyId });
            }

            var ext = Path.GetExtension(attachment.FileName);
            if (!string.Equals(ext, ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Invoice attachment must be a PDF file.";
                return RedirectToAction(nameof(Details), new { id = companyId });
            }

            var company = await _Db.ClientCompanies.FindAsync(new object[] { companyId }, ct);
            if (company == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(company.PaymentReference))
            {
                TempData["ErrorMessage"] = "This company does not have a Payment Reference yet. Please generate it first.";
                return RedirectToAction(nameof(Details), new { id = companyId });
            }

            var now = DateTime.UtcNow;

            // ✅ invoice number is still generated for the file name
            var invoiceNo = GenerateCreditInvoiceNo(company.CompanyId);

            // ✅ Save invoice PDF
            var safeCompanyName = SafeFolderName(company.CompanyName);
            var baseDir = @"C:\Inspired IT Central Hub";
            var creditsDir = Path.Combine(baseDir, safeCompanyName, "Credits");
            Directory.CreateDirectory(creditsDir);

            var fileName = $"{invoiceNo}_{safeCompanyName}.pdf";
            var fullPath = EnsureUniqueFilePath(Path.Combine(creditsDir, fileName));

            try
            {
                await using var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                await attachment.CopyToAsync(stream, ct);
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "Server does not have permission to save the invoice file. Please grant write access to: C:\\Inspired IT Central Hub";
                return RedirectToAction(nameof(Details), new { id = companyId });
            }
            catch (IOException ex)
            {
                TempData["ErrorMessage"] = $"Failed to save invoice file. {ex.Message}";
                return RedirectToAction(nameof(Details), new { id = companyId });
            }

            // ✅ CREDIT BATCH: PurchaseReference MUST be PaymentReference (your requirement)
            var newBatch = new CreditBatch
            {
                CompanyId = company.CompanyId,
                OriginalAmount = creditAmount,
                RemainingAmount = creditAmount,
                LoadDate = now,

                PurchaseReference = company.PaymentReference, // ✅ IMPORTANT
                Notes = notes,

                FileName = Path.GetFileName(fullPath),
                FilePath = fullPath
            };

            _Db.CreditBatches.Add(newBatch);
            await _Db.SaveChangesAsync(ct);

            // ✅ CREDIT TRANSACTION: show payment ref in history table “Reference”
            var transaction = new CreditTransaction
            {
                CompanyId = company.CompanyId,
                BatchId = newBatch.BatchId,
                TransactionType = CreditTransactionType.Purchase,
                CreditsAmount = creditAmount,
                AmountPaid = amountPaid,
                TransactionDate = now,
                ExpiryDate = newBatch.ExpiryDate,
                ReferenceNumber = company.PaymentReference, // ✅ payment ref
                CreatedBy = "Admin",
                Notes = notes
            };

            _Db.CreditTransactions.Add(transaction);

            company.LastModifiedDate = now;
            await _Db.SaveChangesAsync(ct);

            TempData["SuccessMessage"] =
                $"Successfully added {creditAmount} credits to {company.CompanyName}. Payment Ref: {company.PaymentReference}";

            return RedirectToAction(nameof(Details), new { id = companyId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCredit1(int companyId, int creditAmount, decimal amountPaid, string notes)
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
        public async Task<IActionResult> RenewLicense(
       int companyId,
       decimal amountPaid,
       string? paymentReference,   // ignored (read-only in UI)
       string? invoiceNumber,      // ignored (read-only in UI)
       string paymentMethod,       // ✅ NEW: from dropdown
       string? notes,
       IFormFile? attachment,
       CancellationToken ct)
        {
            var company = await _Db.ClientCompanies.FindAsync(new object[] { companyId }, ct);
            if (company == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(company.PaymentReference))
            {
                TempData["ErrorMessage"] = "This company does not have a Payment Reference yet. Please generate it first.";
                return RedirectToAction(nameof(Details), new { id = companyId });
            }

            // ✅ Validate payment method
            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                TempData["ErrorMessage"] = "Please select a Payment Method.";
                return RedirectToAction(nameof(Details), new { id = companyId });
            }

            // ✅ Must upload invoice PDF
            if (attachment == null || attachment.Length == 0)
            {
                TempData["ErrorMessage"] = "Please upload the invoice attachment (PDF) before renewing the license.";
                return RedirectToAction(nameof(Details), new { id = companyId });
            }

            var ext = Path.GetExtension(attachment.FileName);
            if (!string.Equals(ext, ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Invoice attachment must be a PDF file.";
                return RedirectToAction(nameof(Details), new { id = companyId });
            }

            var now = DateTime.UtcNow;

            // ✅ Generate server-side license invoice number
            var licInvoiceNo = GenerateLicenseInvoiceNo(company.CompanyId);

            // ✅ Save invoice PDF to: C:\Inspired IT Central Hub\[Company]\License Renewals\
            var safeCompanyName = SafeFolderName(company.CompanyName);
            var baseDir = @"C:\Inspired IT Central Hub";
            var renewalsDir = Path.Combine(baseDir, safeCompanyName, "License Renewals");
            Directory.CreateDirectory(renewalsDir);

            var fileName = $"{licInvoiceNo}_{safeCompanyName}.pdf";
            var fullPath = EnsureUniqueFilePath(Path.Combine(renewalsDir, fileName));

            try
            {
                await using var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                await attachment.CopyToAsync(stream, ct);
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "Server does not have permission to save the license invoice file. Please grant write access to: C:\\Inspired IT Central Hub";
                return RedirectToAction(nameof(Details), new { id = companyId });
            }
            catch (IOException ex)
            {
                TempData["ErrorMessage"] = $"Failed to save license invoice file. {ex.Message}";
                return RedirectToAction(nameof(Details), new { id = companyId });
            }

            // ✅ Calculate new expiry
            var newExpiryDate = CalculateNextFebruaryExpiry(company.LicenseExpiryDate);

            // ✅ Create renewal record
            var renewal = new LicenseRenewal
            {
                CompanyId = companyId,
                PreviousExpiryDate = company.LicenseExpiryDate,
                NewExpiryDate = newExpiryDate,
                RenewalDate = now,
                AmountPaid = amountPaid,

                PaymentMethod = paymentMethod,               // ✅ NEW
                PaymentReference = company.PaymentReference, // ✅ always company-level reference
                InvoiceNumber = licInvoiceNo,

                Notes = notes,
                ProcessedBy = "Inspired IT Admin",

                FileName = Path.GetFileName(fullPath),
                FilePath = fullPath
            };

            company.LicenseExpiryDate = newExpiryDate;
            company.LicenseStatus = LicenseStatus.Active;
            company.LastModifiedDate = now;

            _Db.LicenseRenewals.Add(renewal);
            await _Db.SaveChangesAsync(ct);

            TempData["SuccessMessage"] =
                $"License renewed successfully until {newExpiryDate:dd MMMM yyyy}. Payment Method: {paymentMethod}. Invoice: {licInvoiceNo}";

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

        private static string GenerateLicenseInvoiceNo(int companyId)
        {
            var now = DateTime.UtcNow;
            var rand = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
            return $"LIC-{now:yyyyMMdd-HHmmss}-{companyId}-{rand}";
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
        private static string GenerateCreditInvoiceNo(int companyId)
        {
            // Example: INV-20260204-143355-123-7F2A
            var now = DateTime.UtcNow;
            var rand = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
            return $"INV-{now:yyyyMMdd-HHmmss}-{companyId}-{rand}";
        }
        private static string SafeFolderName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Company";
            var invalid = Path.GetInvalidFileNameChars();
            var cleaned = new string(name.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
            cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();
            return cleaned.Length > 80 ? cleaned[..80] : cleaned;
        }

        private static string EnsureUniqueFilePath(string fullPath)
        {
            if (!System.IO.File.Exists(fullPath))
                return fullPath;

            var dir = Path.GetDirectoryName(fullPath)!;
            var name = Path.GetFileNameWithoutExtension(fullPath);
            var ext = Path.GetExtension(fullPath);

            for (int i = 2; i < 10_000; i++)
            {
                var candidate = Path.Combine(dir, $"{name} ({i}){ext}");
                if (!System.IO.File.Exists(candidate))
                    return candidate;
            }

            // Extremely unlikely, but safe fallback
            return Path.Combine(dir, $"{name}_{Guid.NewGuid():N}{ext}");
        }

        private static string GeneratePaymentReference(int companyId)
        {
            // DECL-000{CompanyId}00INS-XXXX
            // Example: DECL-00012300INS-7F2A

            var companyCode = $"000{companyId}00INS";

            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var suffix = new string(Enumerable.Range(0, 4)
                .Select(_ => chars[Random.Shared.Next(chars.Length)])
                .ToArray());

            return $"DECL-{companyCode}-{suffix}";
        }


        private async Task<string> GenerateUniquePaymentReferenceAsync(int companyId, CancellationToken ct)
        {
            // very low collision risk, but we’ll still guard it
            for (int i = 0; i < 30; i++)
            {
                var candidate = GeneratePaymentReference(companyId);

                var exists = await _Db.ClientCompanies
                    .AnyAsync(c => c.PaymentReference == candidate, ct);

                if (!exists)
                    return candidate;
            }

            // fallback (extremely unlikely)
            return $"DECL-{companyId}-{Guid.NewGuid().ToString("N")[..3].ToUpperInvariant()}";
        }
        [HttpGet]
        public async Task<IActionResult> DownloadCreditInvoice(int batchId, CancellationToken ct)
        {
            var batch = await _Db.CreditBatches
                .Include(b => b.Company)
                .FirstOrDefaultAsync(b => b.BatchId == batchId, ct);

            if (batch == null)
                return NotFound("Credit batch not found.");

            if (string.IsNullOrWhiteSpace(batch.FilePath) || !System.IO.File.Exists(batch.FilePath))
                return NotFound("Invoice file not found on disk.");

            var downloadName = !string.IsNullOrWhiteSpace(batch.FileName)
                ? batch.FileName
                : Path.GetFileName(batch.FilePath);

            // Force download
            return PhysicalFile(batch.FilePath, "application/pdf", downloadName);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadLicenseInvoice(int renewalId, CancellationToken ct)
        {
            var renewal = await _Db.LicenseRenewals
                .Include(r => r.Company)
                .FirstOrDefaultAsync(r => r.RenewalId == renewalId, ct);

            if (renewal == null)
                return NotFound("License renewal not found.");

            if (string.IsNullOrWhiteSpace(renewal.FilePath) || !System.IO.File.Exists(renewal.FilePath))
                return NotFound("Invoice file not found on disk.");

            var downloadName = !string.IsNullOrWhiteSpace(renewal.FileName)
                ? renewal.FileName
                : Path.GetFileName(renewal.FilePath);

            return PhysicalFile(renewal.FilePath, "application/pdf", downloadName);
        }

    }
}
