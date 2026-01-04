using System.ComponentModel;
using System.ComponentModel.Design;
using Central_Hub.Data;
using Central_Hub.Filter;
using Central_Hub.Models;
using Central_Hub.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using static Azure.Core.HttpHeader;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Central_Hub.Controllers.Api
{

    [Route("api/core")]
    [ApiController]
    [ServiceFilter(typeof(CompanyAuthFilter))]
    public class LicenseApiController : ControllerBase
    {
        private readonly Central_HubDbContext _hubContext;

        public LicenseApiController(Central_HubDbContext context)
        {
            _hubContext = context;

        }

        // ----------- Test API ----------------//

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            //API To Ping Central Hub using company Keys
            var company = HttpContext.Items["Company"] as ClientCompany;

            if (company == null)
            {
                return Unauthorized(new { Message = "Authentication failed: Company not found or inactive" });
            }

            return Ok(new
            {
                Message = "Pong",
                CompanyId = company.CompanyId,  // Numeric ID
                Name = company.CompanyName,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            //API To test the connectivity to Central Hub
            return Ok(new
            {
                status = "healthy",
                service = "Declarify Central Hub API",
                timestamp = DateTime.UtcNow
            });
        }


        // ----------- End of Test API ----------------//

        // ----------- License Logic ----------------//




        [HttpGet("validate/{LicenseKey}")]
        [AllowAnonymous]  // ← THIS BYPASSES the CompanyAuthFilter completely for this action
        public async Task<IActionResult> ValidateLicense(string LicenseKey)
        {
            if (string.IsNullOrWhiteSpace(LicenseKey))
            {
                return BadRequest(new { error = "License key is required" });
            }

            var company = await _hubContext.ClientCompanies.FirstOrDefaultAsync(c => c.LicenseKey == LicenseKey.Trim());

            if (company == null)
            {
                return NotFound(new
                {
                    isValid = false,
                    error = "Invalid license key"
                });
            }

            // Activate the company on first successful validation
            if (!company.IsActive)
            {
                company.IsActive = true;
                company.LastSyncDate = DateTime.UtcNow;
                await _hubContext.SaveChangesAsync();
            }

            bool isExpired = company.LicenseExpiryDate != null && company.LicenseExpiryDate < DateTime.UtcNow;
            int daysUntilExpiry = company.LicenseExpiryDate != null ? (company.LicenseExpiryDate - DateTime.UtcNow).Days : 0;

            //Collect the admin
            var adminUser = await _hubContext.CompanyAdministrators.FirstOrDefaultAsync(ad => ad.CompanyId == company.CompanyId);

            return Ok(new
            {
                isValid = !isExpired,
                companyId = company.CompanyId,
                companyName = company.CompanyName,
                emailDomain = company.EmailDomain,
                expiryDate = company.LicenseExpiryDate,
                daysUntilExpiry = daysUntilExpiry,
                isExpired = isExpired,
                message = isExpired ? "License has expired" : "License valid and activated",

                //Admin
                FirstName = adminUser.FirstName,
                Surname = adminUser.Surname,
                Email = adminUser.Email,
                PhoneNumber = adminUser.PhoneNumber,
                JobTitle = adminUser.JobTitle,
                Department = adminUser.Department
            });
        }

        [HttpGet("check-license")]
        public ActionResult<LicenseCheckResponse> CheckLicense()
        {
            //API to check company license status
            var company = HttpContext.Items["Company"] as ClientCompany;

           
            if (company == null)
                return StatusCode(500, new LicenseCheckResponse { IsValid = false, Message = "Internal error" });

            bool isValid = company.IsActive && (company.LicenseExpiryDate == null || company.LicenseExpiryDate > DateTime.UtcNow);

            var response = new LicenseCheckResponse
            {
                IsValid = isValid,
                Message = isValid ? "License valid" : "License expired, inactive, or insufficient credits",
                ExpiryDate = company.LicenseExpiryDate,
                MaxUsers = company.EstimatedEmployeeCount ?? 0,
                CompanyName = company.CompanyName
            };

            return Ok(response);
        }

        // ----------- End of License Logic ----------------//



        // ----------- Credits Logic ----------------//

        [HttpGet("check-credits")]
        public async Task<IActionResult> GetCreditBalance()
        {
            //API to check company credits 
            var company = HttpContext.Items["Company"] as ClientCompany;

            if (company == null)
            {
                return Unauthorized(new { Message = "Authentication failed: Company not found or inactive" });
            }

            //Check the company batches
            var now = DateTime.UtcNow;
            var batches = _hubContext.CreditBatches.Where(b => b.CompanyId == company.CompanyId).ToList();

            var availableCredits = batches.Where(b => b.ExpiryDate > now).Sum(b => b.RemainingAmount);

            var response = new CreditCheckResponse
            {
                hasCredits = availableCredits > 0,
                lowCreditWarning = availableCredits < 10,
                currentBalance = availableCredits,
                totalPurchased = batches.Sum(b => b.OriginalAmount),
                totalUsed = batches.Sum(b => b.OriginalAmount - b.RemainingAmount)

            };

            return Ok(response);
        }

        [HttpPost("request-credits")]
        public async Task<IActionResult> RequestCredits([FromBody] CreditRequestDto requestDto)
        {
            var company = HttpContext.Items["Company"] as ClientCompany;
            if (company == null) return Unauthorized();

            var request = new CreditRequest
            {
                RequestReference =  $"CR-{DateTime.UtcNow.Ticks}",
                CompanyId = company.CompanyId,
                RequestedCredits = requestDto.RequestedCredits,
                Reason = requestDto.Reason,
                RequestedBy = requestDto.RequestedBy,
                RequesterEmail = requestDto.RequesterEmail,
                Status = CreditRequestStatus.Pending,
            };

            _hubContext.CreditRequests.Add(request);
            await _hubContext.SaveChangesAsync();

            // Then Send emails

            return Ok(new { success = true, message = "Credit request sent successfully" });
        }

        [HttpGet("check-credits-requests")]
        public async Task<IActionResult> GetCreditsRequests()
        {
            //API to check company credits 
            var company = HttpContext.Items["Company"] as ClientCompany;

            if (company == null)
            {
                return Unauthorized(new { Message = "Authentication failed: Company not found or inactive" });
            }

            var requests = await _hubContext.CreditRequests.Where(cr => cr.CompanyId == company.CompanyId).Select(cr => new CreditRequestResponse{
               RequestId = cr.RequestId,
               RequestReference = cr.RequestReference,
               RequestedCredits = cr.RequestedCredits,
               RequestDate = cr.RequestDate,
               Status = cr.Status.ToString(),
               ProcessedBy = cr.ProcessedBy,
               ProcessedDate = cr.ProcessedDate,
               Notes = cr.Notes,
               RequestedBy = cr.RequestedBy
            }).OrderByDescending(r => r.RequestDate).ToListAsync(); ;

            return Ok(requests);

        }


        [HttpPost("consume-credits")]
        public async Task<IActionResult> ConsumeCredits([FromBody] CreditConsumptionRequest request)
        {
            //API to consume credits
            if (request == null || request.CreditsToConsume <= 0)
            {
                return BadRequest(new { success = false, error = "Invalid request" });
            }

            //1. Is license active
            var company = HttpContext.Items["Company"] as ClientCompany;

            if (company == null)
            {
                return Unauthorized(new { Message = "Authentication failed: Company not found or inactive" });
            }

            if (!company.IsActive || company.LicenseExpiryDate < DateTime.UtcNow) return StatusCode(StatusCodes.Status403Forbidden, new { Message = "License Check failed: Company license expired or inactive" });

            //2. Are credits enough?
            var now = DateTime.UtcNow;

            var batches = await _hubContext.CreditBatches.Where(b => b.CompanyId == company.CompanyId && b.ExpiryDate > now).OrderBy(b => b.LoadDate).ToListAsync();
            int totalAvailable = batches.Sum(b => b.RemainingAmount);

            if (totalAvailable < request.CreditsToConsume)
            {
                return BadRequest(new
                {
                    success = false,
                    error = "Insufficient credits",
                    currentBalance = totalAvailable,
                    requiredCredits = request.CreditsToConsume
                });
            }


            //3. Consume  from oldest batches [FIFO]
            int remainingToConsume = request.CreditsToConsume;
            var transactions = new List<CreditTransaction>();
            string referenceNumber = $"SPEND-{DateTime.UtcNow.Ticks}";

            foreach (var batch in batches)
            {
                if (remainingToConsume <= 0) break;

                int deduct = Math.Min(remainingToConsume, batch.RemainingAmount);

                // Update batch
                batch.RemainingAmount -= deduct;

                //4.Save transaction log
                transactions.Add(new CreditTransaction
                {
                    CompanyId = company.CompanyId,
                    BatchId = batch.BatchId,
                    TransactionType = CreditTransactionType.Spend,
                    CreditsAmount = deduct,
                    TransactionDate = now,
                    ReferenceNumber = referenceNumber,
                    Notes = request.Reason ?? "Credits consumed via API",
                    CreatedBy = "API"
                });

                remainingToConsume -= deduct;
            }

            //Save batch updates
            company.LastModifiedDate = now;
            _hubContext.CreditTransactions.AddRange(transactions);
            await _hubContext.SaveChangesAsync();


            int newCreditBalance = await _hubContext.CreditBatches.Where(b => b.CompanyId == company.CompanyId && b.ExpiryDate > now).SumAsync(b => b.RemainingAmount);


            return Ok(new
            {
                success = true,
                creditsConsumed = request.CreditsToConsume,
                remainingBalance = newCreditBalance
            });
        }




        // ----------- End of Credits Logic ----------------//


        // ----------- Company Information ----------------//
        [HttpGet("company-info")]
        public async Task<IActionResult> CompanyInfomation()
        {
            //API to collect information
            var company = HttpContext.Items["Company"] as ClientCompany;
            if (company == null)
            {
                return Unauthorized(new { Message = "Authentication failed: Company not found or inactive" });
            }

            var response = new CompanyInformationResponse
            {
                CompanyName = company.CompanyName,
                CompanyRegistration = company.RegistrationNumber,
                Domain = company.EmailDomain,
                RegisteredDate = company.CreatedDate
            };

            return Ok(response);

        }




        private string GetLicenseMessage(bool isExpired, int daysUntilExpiry)
        {
            if (isExpired)
                return "License expired. Please renew.";
            else if (daysUntilExpiry <= 30)
                return $"License expires in {daysUntilExpiry} days.";
            else
                return "License is active.";
        }
    }
}
