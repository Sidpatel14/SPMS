using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
//using System.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using SPMS.Models;

namespace SPMS.Controllers
{
    public class AccountController : Controller
    {
        //private SmartPermitDbContext _db = new SmartPermitDbContext();


        private readonly MyDbContext _db;
        public AccountController(MyDbContext context)
        {
            _db = context;
            
        }

        // Hash password
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        // GET: /Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string FullName, string Email, string Password)
        {
            if (_db.Users.Any(u => u.Email == Email))
            {
                ViewBag.Error = "Email already exists.";
                return View();
            }

            var user = new User
            {
                FirstName = FullName,
                Email = Email,
                Password = HashPassword(Password),
                Role = "Citizen",
                CreatedAt = DateTime.Now
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            return RedirectToAction("Login");
        }

        // GET: /Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string Email, string Password)
        {
            var hash = HashPassword(Password);
            var user = _db.Users.FirstOrDefault(u => u.Email == Email && u.Password == hash);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            //Session["UserID"] = user.UserId;
           // Session["FullName"] = user.FirstName;
           // Session["Role"] = user.Role;

            return RedirectToAction("Dashboard", "Citizen");
        }

        // Logout
        public ActionResult Logout()
        {
            //Session.Clear();
            return RedirectToAction("Login");
        }
    }
}

