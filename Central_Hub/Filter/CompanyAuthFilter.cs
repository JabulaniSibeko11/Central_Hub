using System.Collections.Concurrent;
using System.Diagnostics;
using Central_Hub.Data;
using Central_Hub.Models;
using Central_Hub.Services.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Central_Hub.Filter
{
    public class CompanyAuthFilter : IAsyncActionFilter
    {
        private readonly Central_HubDbContext _db;
        private readonly IHmacSigningService _hmac;
        private readonly INonceCache _nonce;
        private readonly ILogger<CompanyAuthFilter> _log;

        // Per-company rate limiter: companyId → (window start, count)
        private static readonly ConcurrentDictionary<int, (DateTime Window, int Count)> _rl = new();
        private const int RateLimit = 120; // requests per minute

        // Actions that bypass company auth entirely
        private static readonly HashSet<string> PublicActions =
            new(StringComparer.OrdinalIgnoreCase) { "ValidateLicense", "Health" };

        public CompanyAuthFilter(
            Central_HubDbContext db,
            IHmacSigningService hmac,
            INonceCache nonce,
            ILogger<CompanyAuthFilter> log)
        {
            _db = db;
            _hmac = hmac;
            _nonce = nonce;
            _log = log;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext ctx, ActionExecutionDelegate next)
        {
            var sw = Stopwatch.StartNew();
            var action = ctx.ActionDescriptor.RouteValues["action"] ?? "";
            var path = ctx.HttpContext.Request.Path.Value ?? "";
            var method = ctx.HttpContext.Request.Method;
            var ip = ctx.HttpContext.Connection.RemoteIpAddress?.ToString();
            var hdrs = ctx.HttpContext.Request.Headers;

            // ── Public endpoints ───────────────────────────────
            if (PublicActions.Contains(action))
            {
                var r = await next();
                await Audit(null, method, path, GetStatus(r.Result), sw.ElapsedMilliseconds, true, null, ip, hdrs);
                return;
            }

            // ── 1. X-Company-Code ──────────────────────────────
            if (!hdrs.TryGetValue("X-Company-Code", out var codeVals) ||
                !int.TryParse(codeVals.FirstOrDefault()?.Trim(), out int companyId))
            {
                await Deny(ctx, 401, "Missing or invalid X-Company-Code header",
                           sw.ElapsedMilliseconds, null, path, method, ip, "Missing company code");
                return;
            }

            // ── 2. Load company ────────────────────────────────
            ClientCompany? company;
            try { company = await _db.ClientCompanies.FirstOrDefaultAsync(c => c.CompanyId == companyId); }
            catch (Exception ex)
            {
                await Deny(ctx, 500, "Server error during authentication",
                           sw.ElapsedMilliseconds, companyId, path, method, ip, ex.Message[..Math.Min(100, ex.Message.Length)]);
                return;
            }

            if (company == null)
            {
                await Deny(ctx, 403, $"Company {companyId} not found or inactive",
                           sw.ElapsedMilliseconds, companyId, path, method, ip, "Company not found");
                return;
            }

            // ── 3. Rate limiting ───────────────────────────────
            if (!RateOk(companyId))
            {
                await Deny(ctx, 429, "Rate limit exceeded — 120 requests/minute maximum",
                           sw.ElapsedMilliseconds, companyId, path, method, ip, "Rate limited");
                return;
            }

            // ── 4. HMAC signature (optional but logged) ────────
            var ts = hdrs.TryGetValue("X-Timestamp", out var tv) ? tv.FirstOrDefault() : null;
            var nc = hdrs.TryGetValue("X-Nonce", out var nv) ? nv.FirstOrDefault() : null;
            var sig = hdrs.TryGetValue("X-Signature", out var sv) ? sv.FirstOrDefault() : null;
            bool sigValid = false;

            if (!string.IsNullOrEmpty(ts) && !string.IsNullOrEmpty(nc) && !string.IsNullOrEmpty(sig))
            {
                // Replay check
                var nonceKey = $"{companyId}:{nc}";
                if (!_nonce.TryConsume(nonceKey))
                {
                    await Deny(ctx, 401, "Replayed nonce — request rejected",
                               sw.ElapsedMilliseconds, companyId, path, method, ip, "Replay attack");
                    return;
                }

                sigValid = _hmac.ValidateRequest(
                    companyId, company.LicenseKey ?? string.Empty,
                    ts, nc, sig, method, path);

                if (!sigValid)
                {
                    await Deny(ctx, 401, "Invalid HMAC signature",
                               sw.ElapsedMilliseconds, companyId, path, method, ip, "Signature mismatch");
                    return;
                }
            }
            // No HMAC headers = legacy plain-key mode (backwards compatible)

            ctx.HttpContext.Items["Company"] = company;
            ctx.HttpContext.Items["SignatureValid"] = sigValid;

            var result = await next();
            await Audit(companyId, method, path, GetStatus(result.Result),
                        sw.ElapsedMilliseconds, sigValid, null, ip, hdrs);
        }

        // ── Helpers ────────────────────────────────────────────

        private static bool RateOk(int id)
        {
            var now = DateTime.UtcNow;
            _rl.AddOrUpdate(id,
                _ => (now, 1),
                (_, e) => (now - e.Window).TotalMinutes >= 1 ? (now, 1) : (e.Window, e.Count + 1));
            return _rl[id].Count <= RateLimit;
        }

        private static int GetStatus(IActionResult? r) => r switch
        {
            ObjectResult o => o.StatusCode ?? 200,
            OkResult => 200,
            UnauthorizedResult => 401,
            NotFoundResult => 404,
            BadRequestResult => 400,
            _ => 200
        };

        private async Task Deny(ActionExecutingContext ctx, int code, string msg,
                                 long ms, int? co, string path, string method,
                                 string? ip, string reason)
        {
            ctx.Result = new ObjectResult(new { Message = msg }) { StatusCode = code };
            _log.LogWarning("API {Code} {Method} {Path} company={Co} — {Reason}", code, method, path, co, reason);
            await Audit(co, method, path, code, ms, false, reason, ip, default);
        }

        private async Task Audit(int? co, string method, string path, int code,
                                  long ms, bool sigValid, string? reason,
                                  string? ip, IHeaderDictionary? hdrs)
        {
            try
            {
                _db.ApiAuditLogs.Add(new ApiAuditLog
                {
                    CompanyId = co,
                    HttpMethod = method,
                    Path = path,
                    StatusCode = code,
                    SignatureValid = sigValid,
                    IpAddress = ip,
                    UserAgent = hdrs?.TryGetValue("User-Agent", out var ua) == true ? ua.FirstOrDefault()?[..Math.Min(300, ua.FirstOrDefault()?.Length ?? 0)] : null,
                    DurationMs = ms,
                    FailureReason = reason,
                    RequestedAtUtc = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
            }
            catch { /* never crash the request for audit logging */ }
        }
    }
}