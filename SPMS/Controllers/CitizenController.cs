using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace SPMS.Controllers
{
    public class CitizenController : Controller
    {
        public IActionResult Dashboard()
        {

            return View();
        }
        public IActionResult Apply()
        {

            return View();
        }
        [HttpPost]
        public ActionResult SubmitApplication(ApplicationModel model)
        {
            if (ModelState.IsValid)
            {
                // Save to database
                // Handle file upload
                // Redirect to MyApplications
            }
            return View("Apply");
        }
        public ActionResult MyApplications(string search, string status, DateTime? fromDate, DateTime? toDate)
        {
            // Fetch list from DB based on filter criteria
            //var apps = db.Applications.Where(...).ToList();
            return View();
        }

    }
}
