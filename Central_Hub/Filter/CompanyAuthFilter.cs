using Central_Hub.Data;
using Central_Hub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Central_Hub.Filter
{
    // Central App: CompanyAuthFilter.cs
    public class CompanyAuthFilter : IAsyncActionFilter
    {
        private readonly Central_HubDbContext _db;

        public CompanyAuthFilter(Central_HubDbContext db)
        {
            _db = db;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //skip api auth for activation process only
            var actionName = context.ActionDescriptor.RouteValues["action"];
            if (actionName == "ValidateLicense")
            {
                await next();
                return;
            }


            var headers = context.HttpContext.Request.Headers;

            if (!headers.TryGetValue("X-Company-Code", out var companyCodeValues) ||
                string.IsNullOrWhiteSpace(companyCodeValues.FirstOrDefault()))
            {
                context.Result = new UnauthorizedObjectResult("Missing X-Company-Code header");
                return;
            }

            var companyCodeStr = companyCodeValues.First()!.Trim();

            if (!int.TryParse(companyCodeStr, out int companyId))
            {
                context.Result = new BadRequestObjectResult(
                    new { Message = "Invalid company code format. Must be a numeric ID (e.g., 101)" });
                return;
            }

            ClientCompany? company = null;

            try
            {
                if (headers.TryGetValue("X-Api-Key", out var apiKeyValues) &&
                    !string.IsNullOrWhiteSpace(apiKeyValues.FirstOrDefault()))
                {
                    var apiKey = apiKeyValues.First()!.Trim();
                    company = await _db.ClientCompanies.FirstOrDefaultAsync(c => c.CompanyId == companyId && c.LicenseKey == apiKey);
                }
                else
                {
                    company = await _db.ClientCompanies.FirstOrDefaultAsync(c => c.CompanyId == companyId);
                }
            }
            catch (Exception ex)
            {
                // Log ex.Message in production (e.g., using ILogger)
                context.Result = new ObjectResult(new { Message = "Server error during authentication", Error = ex.Message })
                { StatusCode = 500 };
                return;
            }

            if (company == null)
            {
                context.Result = new ObjectResult( new { Message = $"Company ID {companyId} not found, inactive, or invalid key" }) { StatusCode = 403 };
                return;
            }

           

            context.HttpContext.Items["Company"] = company;
            await next();
        }
    }
}
