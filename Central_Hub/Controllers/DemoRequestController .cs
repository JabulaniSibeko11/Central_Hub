using Central_Hub.Data;
using Central_Hub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Central_Hub.Controllers
{
    public class DemoRequestController : Controller
    {
        private readonly ILogger<DemoRequestController> _logger;
        private readonly IConfiguration _config;
        private readonly Central_HubDbContext _Db;
        public DemoRequestController(ILogger<DemoRequestController> logger, IConfiguration config, Central_HubDbContext Db)
        {
            _logger = logger;
            _config = config;
            _Db = Db;
        }

        [HttpGet]
        public IActionResult RequestDemo()
        {
            return View(new DemoRequest());

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestDemo(DemoRequest demoRequest)
        {
            
                var existingRequest = _Db.DemoRequests
                    .FirstOrDefault(dr => dr.Email == demoRequest.Email
                    && dr.Status != DemoRequestStatus.NotInterested
                    && dr.Status != DemoRequestStatus.Cancelled);
                if (existingRequest != null)
                {

                    ModelState.AddModelError(string.Empty, "A demo request with this email already exists.");
                    return View(demoRequest);
                }

            try
            {

                demoRequest.RequestDate = DateTime.UtcNow;
                demoRequest.Status = DemoRequestStatus.Pending;

                demoRequest.ClientInstanceId = null;


                _Db.DemoRequests.Add(demoRequest);
                await _Db.SaveChangesAsync();


                return RedirectToAction(nameof(DemoRequestSuccess));


            }
            catch (DbUpdateException ex) {
                ModelState.AddModelError(string.Empty,
            "Unable to submit your demo request. Please try again later or contact support.");
                return View(demoRequest);
            }

           // return View(demoRequest);
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
            var request = await query.OrderByDescending(dr => dr.RequestDate).ToListAsync();

            ViewBag.SelectedStatus = status;
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> DemoRequestDetails(int id)
        {
            var request = await _Db.DemoRequests.FirstOrDefaultAsync(d => d.DemoRequestId == id);


            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDemoRequestStatus(int id, DemoRequestStatus status, string notes)
        {

            var request= await _Db.DemoRequests.FindAsync(id);
            if (request == null) {
                return NotFound();


            }

            request.Status = status;

            if (!string.IsNullOrWhiteSpace(notes))
            {
            request.InternalNotes = notes;

            }

            if (status == DemoRequestStatus.Scheduled && !request.DemoScheduledDate.HasValue)
            {
                request.DemoScheduledDate = DateTime.Now;

            }
            else if (status == DemoRequestStatus.Completed && !request.DemoScheduledDate.HasValue) { 
             request.DemoCompletedDate = DateTime.Now;
            }  
            await _Db.SaveChangesAsync();

            TempData["SuccessMessage"]= "Demo request status updated successfully.";

        return RedirectToAction(nameof(DemoRequestDetails), new { id = id } );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConvertToClient(int id)
        {
            var demorequest = await _Db.DemoRequests.FindAsync(id);
            if (demorequest == null) {
                return NotFound();

            }

            if (demorequest.ConvertedToClient)
            {


                TempData["ErrorMessage"] = "This demo request has already been converted to a client.";
                return RedirectToAction(nameof(DemoRequestDetails), new { id = id });

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