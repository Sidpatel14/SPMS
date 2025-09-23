using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SPMS.Models;
using System.Reflection.Metadata;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


//public class ApplicationsController : Controller
//    {
//    private readonly MyDbContext _context;

//    public ApplicationsController(MyDbContext context)
//    {
//        _context = context;
//    }

//    public IActionResult Index()
//    {
//        var applications = _context.Applications.ToList();
//        return View(applications);
//    }
//    public IActionResult Create()
//    {
//        return View();
//    }

//}

namespace SPMS.Controllers
{
   // [Authorize(Roles = "Citizen")]
    public class ApplicationsController : Controller
    {
        private readonly MyDbContext _context;
       // private readonly UserManager<IdentityUser> _userManager;

        public ApplicationsController(MyDbContext context/*, UserManager<IdentityUser> userManager*/)
        {
            _context = context;
            //_userManager = userManager;
        }

        // GET: My Applications
        public IActionResult Index()
        {
            //long userId = _userManager.GetUserId(User);
            //var applications = _context.Applications
            //    .Where(a => a.UserId == userId)
            //    .ToList();
            var applications = _context.Applications.ToList();
            return View(applications);
        }

        // GET: Create Application
        public IActionResult Create()
        {
            return View();
        }

        //// POST: Create Application
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(Application application, List<IFormFile> files)
        //{
        //    //if (ModelState.IsValid)
        //    //{
        //    //    application.UserId = _userManager.GetUserId(User);
        //    //    application.Status = "Pending";
        //    //    _context.Add(application);
        //    //    await _context.SaveChangesAsync();

        //    //    // Handle file uploads
        //    //    foreach (var file in files)
        //    //    {
        //    //        if (file.Length > 0)
        //    //        {
        //    //            var filePath = Path.Combine("wwwroot/uploads", file.FileName);

        //    //            using (var stream = new FileStream(filePath, FileMode.Create))
        //    //            {
        //    //                await file.CopyToAsync(stream);
        //    //            }

        //    //            var doc = new Document
        //    //            {
        //    //                ApplicationId = application.ApplicationId,
        //    //                FileName = file.FileName,
        //    //                FilePath = filePath,
        //    //                ContentType = file.ContentType
        //    //            };

        //    //            _context.Documents.Add(doc);
        //    //        }
        //    //    }
        //    //    await _context.SaveChangesAsync();

        //    //    return RedirectToAction(nameof(MyApplications));
        //    //}
        //    return View(application);
        //}
    }
}


