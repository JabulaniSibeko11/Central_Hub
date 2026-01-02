using Central_Hub.Data;
using Central_Hub.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace Central_Hub.Controllers.Api
{

    [Route("api/[controller]")]
    [ApiController]
    public class LicenseApiController : ControllerBase
    {
        private readonly Central_HubDbContext _hubContext;

        public LicenseApiController(Central_HubDbContext context)
        {
            _hubContext = context;

        }

        [HttpGet("Validate/{LicenseKey}")]
        public async Task<IActionResult> ValidateLicense(string LicenseKey)
        {

            if (string.IsNullOrWhiteSpace(LicenseKey))
            {

                return BadRequest(new { error = "License key is required" });


            }
            var company = await _hubContext.ClientCompanies.FirstOrDefaultAsync(c => c.LicenseKey == LicenseKey);

            if (company == null)
            {
                return NotFound(new
                {
                    isValid = false,
                    error = "Invalid license key"
                });
            }
            company.IsActive = true;
            company.LastSyncDate = DateTime.UtcNow;
            await _hubContext.SaveChangesAsync();


            bool IsExpireds = company.LicenseExpiryDate < DateTime.UtcNow;

            int daysUntilExpirys = (company.LicenseExpiryDate - DateTime.UtcNow).Days;


            return Ok(new
            {
                isValid = !IsExpireds,
                LicenseKey = company.LicenseKey,
                companyName = company.CompanyName,
                emailDomain = company.EmailDomain,
                licenseStatus = company.LicenseStatus,
                expiryDate = company.LicenseExpiryDate,
                daysUntilExpiry = daysUntilExpirys,

                IsExpired = IsExpireds,
                message = GetLicenseMessage(IsExpireds, daysUntilExpirys)
            });
        }
        


        [HttpGet("Credit/{licenseKey}")]
        public async Task<IActionResult> GetCreditBalance(string licenseKey)
        {

            var company = await _hubContext.ClientCompanies.FirstOrDefaultAsync(c => c.LicenseKey == licenseKey);
            if (company == null)
            {
                return NotFound(
                new
                {
                    error = "Invalid license Key"
                });

            }
            return Ok(new
            {
                licenseKey = company.LicenseKey,
                companyName = company.CompanyName,
                currentBalance = company.CurrentCreditBalance,
                totalPurchased = company.TotalCreditsPurchased,
                totalUsed = company.TotalCreditsUsed,
                hasCredits = company.CurrentCreditBalance > 0,
                lowCreditWarning = company.CurrentCreditBalance < 10
            });
        }

        [HttpPost("consume-credits")]
        public async Task<IActionResult> ConsumeCredits([FromBody] CreditConsumptionRequest request) {

            if (request == null || string.IsNullOrWhiteSpace(request.LicenseKey)) {

                return BadRequest(new { error = "Invalid request" });

            }

            var company = await _hubContext.ClientCompanies.FirstOrDefaultAsync(c => c.LicenseKey == request.LicenseKey);

            if (company == null) {
                return NotFound(new { error = "Invalid license Key" });
            }

            if (company.LicenseExpiryDate < DateTime.UtcNow)
            {

                return BadRequest(new
                {
                    success = false,
                    error = "License expired"

                });
            }


            if (company.CurrentCreditBalance < request.CreditsToConsume) {

                return BadRequest(new {
                    success = false,
                    error = "Insufficient credits",
                    currentBalance = company.CurrentCreditBalance,
                    requiredCredits = request.CreditsToConsume
                });

            }

            company.CurrentCreditBalance -= request.CreditsToConsume;
            company.TotalCreditsUsed += request.CreditsToConsume;
            company.LastModifiedDate = DateTime.UtcNow;

            await _hubContext.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                creditsConsumed = request.CreditsToConsume,
                remainingBalance = company.CurrentCreditBalance
            });
        }

        [HttpGet("sync/{licenseKey}")]
        public async Task<IActionResult> Sync(string licenseKey) { 
        
        
        var company = await _hubContext.ClientCompanies
                .Include(x=>x.Administrator)
                .FirstOrDefaultAsync(c=>c.LicenseKey==licenseKey);

            if (company == null) { return NotFound(new { error = "Invalid license key" }); };
           
                
                company.LastSyncDate = DateTime.UtcNow;
                await _hubContext.SaveChangesAsync();

            bool isExpired = company.LicenseExpiryDate < DateTime.UtcNow;
            int daysUntilExpiry = (company.LicenseExpiryDate - DateTime.UtcNow).Days;

            return Ok(new
            {
                license = new
                {
                    key = company.LicenseKey,
                    status = company.LicenseStatus.ToString(),
                    isValid = !isExpired && company.IsActive,
                    expiryDate = company.LicenseExpiryDate,
                    daysUntilExpiry = daysUntilExpiry
                },
                company = new
                {
                    name = company.CompanyName,
                    emailDomain = company.EmailDomain
                },
                administrator = company.Administrator != null ? new
                {
                    name = company.Administrator.FullName,
                    email = company.Administrator.Email
                } : null,
                credits = new
                {
                    currentBalance = company.CurrentCreditBalance,
                    totalPurchased = company.TotalCreditsPurchased,
                    totalUsed = company.TotalCreditsUsed
                }
            });

        }


        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "healthy",
                service = "Declarify Central Hub API",
                timestamp = DateTime.UtcNow
            });
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
