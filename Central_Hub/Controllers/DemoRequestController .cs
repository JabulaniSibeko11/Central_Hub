using Central_Hub.Data;
using Central_Hub.Models;
using Central_Hub.Services.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Central_Hub.Controllers
{
    public class DemoRequestController : Controller
    {
        private readonly ILogger<DemoRequestController> _logger;
        private readonly IConfiguration _config;
        private readonly Central_HubDbContext _Db;
        private readonly IEmailService _email;

        public DemoRequestController(
            ILogger<DemoRequestController> logger,
            IConfiguration config,
            Central_HubDbContext Db,
            IEmailService email)
        {
            _logger = logger;
            _config = config;
            _Db = Db;
            _email = email;
        }

        [HttpGet]
        public IActionResult RequestDemo()
        {
            return View(new DemoRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestDemo(DemoRequest demoRequest, CancellationToken ct)
        {
            var existingRequest = await _Db.DemoRequests
                .FirstOrDefaultAsync(dr =>
                        dr.Email == demoRequest.Email
                        && dr.Status != DemoRequestStatus.NotInterested
                        && dr.Status != DemoRequestStatus.Cancelled,
                    ct);

            if (existingRequest != null)
            {
                ModelState.AddModelError(string.Empty, "A demo request with this email already exists.");
                return View(demoRequest);
            }

            try
            {
                demoRequest.RequestDate = DateTime.UtcNow;
                demoRequest.Status = DemoRequestStatus.Pending;

                _Db.DemoRequests.Add(demoRequest);
                await _Db.SaveChangesAsync(ct);

                // ✅ Email: We received your request (demo SMTP)
                // ✅ Email: We received your request (includes request details)
                try
                {
                    if (!string.IsNullOrWhiteSpace(demoRequest.Email))
                    {
                        var firstName =
                            demoRequest.ContactPersonName?.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
                            ?? "there";

                        string fmtDate(DateTime? d) => d.HasValue ? d.Value.ToString("dd MMMM yyyy") : "Not provided";
                        string fmtDateOnly(DateOnly? d) => d.HasValue ? d.Value.ToString("dd MMMM yyyy") : "Not provided";

                        // Your model uses PreferredDemoDate (from the view). In your DB it might be DateTime? or DateOnly?
                        // We'll support both patterns safely by using ToString where possible.
                        var preferredDateText =
                            demoRequest.PreferredDemoDate != null
                                ? demoRequest.PreferredDemoDate.Value.ToString("dd MMMM yyyy")
                                : "Not provided";

                        var preferredTimeText = string.IsNullOrWhiteSpace(demoRequest.PreferredTime)
                            ? "Not provided"
                            : demoRequest.PreferredTime;

                        var subject = "We received your demo request (Declarify)";

                        var body = $@"
<div style='font-family:Segoe UI,Arial,sans-serif;font-size:14px;line-height:1.55;color:#111'>
  <h2 style='margin:0 0 10px 0'>Demo Request Received</h2>

  <p style='margin:0 0 14px 0'>Hi {firstName},</p>

  <p style='margin:0 0 14px 0'>
    Thank you — we have received your demo request for <b>{demoRequest.CompanyName}</b>.
  </p>

  <p style='margin:0 0 18px 0'>
    <b>Next step:</b> Our team will contact you within <b>24 hours</b> to schedule your personalised demo.
  </p>

  <hr style='border:none;border-top:1px solid #e5e7eb;margin:18px 0' />

  <h3 style='margin:0 0 10px 0'>Your submitted details</h3>

  <table style='border-collapse:collapse;width:100%'>
    <tr>
      <td style='padding:6px 0;width:220px;color:#374151'><b>Company Name</b></td>
      <td style='padding:6px 0'>{demoRequest.CompanyName}</td>
    </tr>
    <tr>
      <td style='padding:6px 0;color:#374151'><b>Registration Number</b></td>
      <td style='padding:6px 0'>{demoRequest.RegistrationNumber}</td>
    </tr>
    <tr>
      <td style='padding:6px 0;color:#374151'><b>Organisation Type</b></td>
      <td style='padding:6px 0'>{demoRequest.OrganizationType}</td>
    </tr>
    <tr>
      <td style='padding:6px 0;color:#374151'><b>Estimated Employees</b></td>
      <td style='padding:6px 0'>{(demoRequest.EstimatedEmployeeCount?.ToString() ?? "Not provided")}</td>
    </tr>
    <tr>
      <td style='padding:6px 0;color:#374151'><b>Email Domain</b></td>
      <td style='padding:6px 0'>{demoRequest.OrganizationEmailDomain}</td>
    </tr>
    <tr>
      <td style='padding:6px 0;color:#374151'><b>Province</b></td>
      <td style='padding:6px 0'>{demoRequest.Province}</td>
    </tr>

    <tr><td colspan='2' style='padding:8px 0'></td></tr>

    <tr>
      <td style='padding:6px 0;color:#374151'><b>Contact Person</b></td>
      <td style='padding:6px 0'>{demoRequest.ContactPersonName}</td>
    </tr>
    <tr>
      <td style='padding:6px 0;color:#374151'><b>Job Title</b></td>
      <td style='padding:6px 0'>{(string.IsNullOrWhiteSpace(demoRequest.JobTitle) ? "Not provided" : demoRequest.JobTitle)}</td>
    </tr>
    <tr>
      <td style='padding:6px 0;color:#374151'><b>Department</b></td>
      <td style='padding:6px 0'>{(string.IsNullOrWhiteSpace(demoRequest.Department) ? "Not provided" : demoRequest.Department)}</td>
    </tr>
    <tr>
      <td style='padding:6px 0;color:#374151'><b>Email</b></td>
      <td style='padding:6px 0'>{demoRequest.Email}</td>
    </tr>
    <tr>
      <td style='padding:6px 0;color:#374151'><b>Phone</b></td>
      <td style='padding:6px 0'>{demoRequest.PhoneNumber}</td>
    </tr>

    <tr><td colspan='2' style='padding:8px 0'></td></tr>

    <tr>
      <td style='padding:6px 0;color:#374151'><b>Preferred Demo Date</b></td>
      <td style='padding:6px 0'>{preferredDateText}</td>
    </tr>
    <tr>
      <td style='padding:6px 0;color:#374151'><b>Preferred Time</b></td>
      <td style='padding:6px 0'>{preferredTimeText}</td>
    </tr>
    <tr>
      <td style='padding:6px 0;color:#374151'><b>Additional Info</b></td>
      <td style='padding:6px 0'>{(string.IsNullOrWhiteSpace(demoRequest.AdditionalInfo) ? "Not provided" : demoRequest.AdditionalInfo)}</td>
    </tr>
    <tr>
      <td style='padding:6px 0;color:#374151'><b>How you heard about us</b></td>
      <td style='padding:6px 0'>{(string.IsNullOrWhiteSpace(demoRequest.HearAboutUs) ? "Not provided" : demoRequest.HearAboutUs)}</td>
    </tr>
  </table>

  <hr style='border:none;border-top:1px solid #e5e7eb;margin:18px 0' />

  <p style='margin:0'>
    Regards,<br/>
    <b>Inspired IT Central Hub</b>
  </p>
</div>";

                        await _email.SendAsync(demoRequest.Email, subject, body, ct);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send demo request confirmation email to {Email}", demoRequest.Email);
                }


                return RedirectToAction(nameof(DemoRequestSuccess));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DbUpdateException while saving demo request for {Email}", demoRequest.Email);

                ModelState.AddModelError(string.Empty,
                    "Unable to submit your demo request. Please try again later or contact support.");
                return View(demoRequest);
            }
        }

        public IActionResult DemoRequestSuccess()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ManageDemoRequest(string status = "all")
        {
            var query = _Db.DemoRequests.AsQueryable();

            if (status != "all" && Enum.TryParse<DemoRequestStatus>(status, out var statusEnum))
            {
                query = query.Where(dr => dr.Status == statusEnum);
            }

            var request = await query
                .OrderByDescending(dr => dr.RequestDate)
                .ToListAsync();

            ViewBag.SelectedStatus = status;
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> DemoRequestDetails(int id)
        {
            var request = await _Db.DemoRequests.FirstOrDefaultAsync(d => d.DemoRequestId == id);

            if (request == null)
                return NotFound();

            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDemoRequestStatus(int id, DemoRequestStatus status, string notes, CancellationToken ct)
        {
            var request = await _Db.DemoRequests.FindAsync(new object[] { id }, ct);
            if (request == null)
                return NotFound();

            request.Status = status;

            if (!string.IsNullOrWhiteSpace(notes))
                request.InternalNotes = notes;

            if (status == DemoRequestStatus.Scheduled && !request.DemoScheduledDate.HasValue)
            {
                request.DemoScheduledDate = DateTime.UtcNow;
            }
            else if (status == DemoRequestStatus.Completed && !request.DemoCompletedDate.HasValue)
            {
                request.DemoCompletedDate = DateTime.UtcNow;
            }

            await _Db.SaveChangesAsync(ct);

            TempData["SuccessMessage"] = "Demo request status updated successfully.";
            return RedirectToAction(nameof(DemoRequestDetails), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConvertToClient(int id, CancellationToken ct)
        {
            var demorequest = await _Db.DemoRequests.FindAsync(new object[] { id }, ct);
            if (demorequest == null)
                return NotFound();

            if (demorequest.ConvertedToClient)
            {
                TempData["ErrorMessage"] = "This demo request has already been converted to a client.";
                return RedirectToAction(nameof(DemoRequestDetails), new { id });
            }

            TempData["DemoRequestId"] = demorequest.DemoRequestId;
            TempData["CompanyName"] = demorequest.CompanyName;
            TempData["RegistrationNumber"] = demorequest.RegistrationNumber;
            TempData["EmailDomain"] = demorequest.OrganizationEmailDomain;
            TempData["CompanyType"] = demorequest.OrganizationType.ToString();
            TempData["EstimatedEmployeeCount"] = demorequest.EstimatedEmployeeCount;
            TempData["Province"] = demorequest.Province;

            TempData["AdminFirstName"] = demorequest.ContactPersonName.Split(' ').First();
            TempData["AdminLastName"] = string.Join(" ", demorequest.ContactPersonName.Split(' ').Skip(1));
            TempData["AdminEmail"] = demorequest.Email;
            TempData["AdminPhone"] = demorequest.PhoneNumber;
            TempData["AdminJobTitle"] = demorequest.JobTitle;
            TempData["AdminDepartment"] = demorequest.Department;

            TempData.Keep();

            return RedirectToAction("CreateFromDemo", "ClientCompany");
        }
    }
}
