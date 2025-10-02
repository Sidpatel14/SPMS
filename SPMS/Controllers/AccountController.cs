using Microsoft.AspNetCore.Http;
//using System.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPMS.Models;
using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace SPMS.Controllers
{
    public class AccountController : Controller
    {

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

        

        [HttpPost]
        public IActionResult Register(User model)
        {
            model.Ipaddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            string connectionString = _db.Database.GetDbConnection().ConnectionString;
            if (_db.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

           

            if (ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    //string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
                    string hashedPassword = HashPassword(model.Password);

                    SqlCommand cmd = new SqlCommand(@"INSERT INTO Users 
                (Title, FirstName, LastName, Email, Password, Role, Phone, Address1, Address2, Town, State, Country, CreatedAt) 
                VALUES 
                (@Title, @FirstName, @LastName, @Email, @Password, 'Citizen', @Phone, @Address1, @Address2, @Town, @State, @Country, GETDATE())", con);

                    cmd.Parameters.AddWithValue("@Title", model.Title);
                    cmd.Parameters.AddWithValue("@FirstName", model.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", model.LastName);
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    cmd.Parameters.AddWithValue("@Password", hashedPassword);
                    cmd.Parameters.AddWithValue("@Phone", model.Phone ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address1", model.Address1);
                    cmd.Parameters.AddWithValue("@Address2", model.Address2 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Town", model.Town ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@State", model.State);
                    cmd.Parameters.AddWithValue("@Country", model.Country);
                    cmd.Parameters.AddWithValue("@IPAddress", HttpContext.Connection.RemoteIpAddress?.ToString());
                    cmd.ExecuteNonQuery();
                }
                //SendWelcomeEmail(model.Email, model.FirstName);
                TempData["SuccessMessage"] = "Registration successful! You can now log in.";
                return RedirectToAction("Login");
            }

            return View(model);
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
            //hash = BCrypt.Net.BCrypt.HashPassword(Password);
            var user = _db.Users.FirstOrDefault(u => u.Email == Email && u.Password == hash);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }
            if (user != null)
            {
                HttpContext.Session.SetString("UserID", user.UserId.ToString());
                HttpContext.Session.SetString("FullName", user.FirstName);
                HttpContext.Session.SetString("Role", user.Role);

                var role = user.Role.ToString();
                if (role == "Citizen")
                    return RedirectToAction("Dashboard", "Citizen");
                else if (role == "Staff")
                    return RedirectToAction("Dashboard", "Staff");
                else if (role == "Admin")
                    return RedirectToAction("Dashboard", "Admin");
            }


            return RedirectToAction("Dashboard", "Citizen");
        }
        // Logout
        public ActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clear all session data
            return RedirectToAction("Login");
        }

        // GET: /Account/GetCountries
        public JsonResult GetCountries()
        {
            var countries = _db.Country
                .Select(c => new
                {
                    countryID = c.CountryID,
                    countryName = c.Name
                })
                .OrderBy(c => c.countryName)
                .ToList();

            return Json(countries);
        }

        // GET: /Account/GetStates?countryId=1
        public JsonResult GetStates(int countryId)
        {
            // Get ISO code for country
            var countryIso = _db.Country
                .Where(c => c.CountryID == countryId)
                .Select(c => c.ISO)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(countryIso))
                return Json(new List<object>());

            var states = _db.State
                .Where(s => s.CountryISO == countryIso)
                .Select(s => new
                {
                    stateID = s.StateID,
                    stateName = s.Name
                })
                .OrderBy(s => s.stateName)
                .ToList();

            return Json(states);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();

            base.Dispose(disposing);
        }


        [HttpGet]
        public JsonResult CheckEmail(string email)
        {
            var exists = _db.Users.Any(u => u.Email == email);
            return Json(exists);
        }

        private void SendWelcomeEmail(string toEmail, string firstName)
        {
            var fromAddress = new MailAddress("yourapp@example.com", "Smart Permit System");
            var toAddress = new MailAddress(toEmail);
            const string fromPassword = "your-app-password"; // Use a real SMTP app password, not your normal one
            string subject = "Welcome to Smart Permit System";
            string body = $"Hi {firstName},\n\nYour registration was successful. You can now log in and start using the system.\n\nThanks,\nSmart Permit Team";

            using (var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",      // change if using another provider
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            })
            {
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }
            }
        }

        // =========== Forget Password =============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string email)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                // generate token
                //user.PasswordResetToken = Guid.NewGuid().ToString();
                //user.TokenExpiry = DateTime.UtcNow.AddHours(1);
                _db.SaveChanges();

                // TODO: Send email with link like /Account/ResetPassword?token=xxx
                ViewBag.Message = "If this email is registered, a reset link has been sent.";
            }
            else
            {
                ViewBag.Message = "If this email is registered, a reset link has been sent.";
            }
            return View();
        }

        // ===========================
        // Profile Page
        // ===========================
        [HttpGet]
        public IActionResult Profile()
        {
            // Get the string value from session
            string? userIdString = HttpContext.Session.GetString("UserID");

            // Convert to integer if needed
            int userId = 0;
            if (!string.IsNullOrEmpty(userIdString))
            {
                userId = int.Parse(userIdString);
            }
            var user = _db.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return RedirectToAction("Login");
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(User model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Invalid input";
                return View("Profile", model);
            }

            var user = _db.Users.FirstOrDefault(u => u.UserId == model.UserId);
            if (user == null)
            {
                ViewBag.Error = "User not found";
                return View("Profile", model);
            }

            // Update fields
            user.Title = model.Title;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.Phone = model.Phone;
            user.Address1 = model.Address1;
            user.Address2 = model.Address2;
            user.Town = model.Town;
            user.State = model.State;
            user.Country = model.Country;

            _db.SaveChanges();
            ViewBag.Message = "Profile updated successfully";
            return View("Profile", user);
        }

    }
}

