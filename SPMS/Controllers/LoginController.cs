
using Microsoft.AspNetCore.Mvc;
using SPMS.Models;




namespace SPMS.Controllers
{

    public class LoginController : Controller
    {
        private readonly MyDbContext _context;

        public LoginController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult CitizenLogin()
        {
            return View();
        }

        

 
    }
}



