using Microsoft.AspNetCore.Http;
//using System.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPMS.Models;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SPMS.Controllers
{
    public class AccountController : Controller
    {

        private readonly SpmsContext _db;
        private readonly string connectionString;
        private readonly EmailService _emailService;
        public AccountController(SpmsContext context, EmailService emailService)
        {
            _db = context;
            connectionString = _db.Database.GetDbConnection().ConnectionString;
            _emailService = emailService;
        }

        // GET: /Account/Register
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            model.Ipaddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

            string hashedPassword = HashPassword(model.Password);
            //model.Ipaddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            try
            {
                long newUserId;
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("sp_CreateUser", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Title", model.Title);
                    cmd.Parameters.AddWithValue("@FirstName", model.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", model.LastName);
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    cmd.Parameters.AddWithValue("@Password", hashedPassword);
                    cmd.Parameters.AddWithValue("@Role", "Citizen");
                    cmd.Parameters.AddWithValue("@Phone", model.Phone ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address1", model.Address1);
                    cmd.Parameters.AddWithValue("@Address2", model.Address2 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Town", model.Town ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@State", model.State);
                    cmd.Parameters.AddWithValue("@Country", model.Country);
                    cmd.Parameters.AddWithValue("@IPAddress", model.Ipaddress ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserId", model.UserId);
                    conn.Open();
                    newUserId = Convert.ToInt64(cmd.ExecuteScalar());
                }

                // log in AuditLogs
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("INSERT INTO AuditLogs(UserID, Action, Timestamp, Notes) VALUES(@UserID, @Action, GETDATE(), @Notes)", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", newUserId);
                    cmd.Parameters.AddWithValue("@Action", "User Created");
                    cmd.Parameters.AddWithValue("@Notes", $"User {model.Email} created with role Citizen");
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                // Send welcome email
                //string subject = "Welcome to SPMS";
                //string body = $"Hi {model.FirstName},<br/>Your account has been created successfully.";
                //await _emailService.SendEmailAsync(model.Email, subject, body);
                TempData["SuccessMessage"] = "Registration successful! You can now log in.";
                return RedirectToAction("Login");
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("Email already exists"))
                    ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
                throw;
            }
            //}
            //return BadRequest("Invalid data.");

            //return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
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
            var countries = _db.Countries
                .Select(c => new
                {
                    countryID = c.CountryId,
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
            var countryIso = _db.Countries
                .Where(c => c.CountryId == countryId)
                .Select(c => c.Iso)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(countryIso))
                return Json(new List<object>());

            var states = _db.States
                .Where(s => s.CountryIso == countryIso)
                .Select(s => new
                {
                    stateID = s.StateId,
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

        // Hash password
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
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

        // GET: /Account/ForgotPassword
        public ActionResult ForgetPassword()
        {
            return View();
        }
        // =========== Forget Password =============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgetPassword(string email)
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

            long stateId = 0;
            if (long.TryParse(user.State?.ToString(), out var parsedState))
                stateId = parsedState;

            long countryId = 0;
            if (long.TryParse(user.Country?.ToString(), out var parsedCountry))
                countryId = parsedCountry;

            ViewBag.Countries = _db.Countries
               .Select(c => new
               {
                   countryID = c.CountryId,
                   countryName = c.Name
               })
               .OrderBy(c => c.countryID)
               .ToList();

            ViewBag.states = _db.States
               .Select(s => new
               {
                   stateID = s.StateId,
                   stateName = s.Name
               })
               .OrderBy(s => s.stateID)
               .ToList();

            var model = new UserProfileViewModel
            {
                UserId = user.UserId,
                Title = user.Title,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Password = user.Password,
                Role = user.Role,
                Phone = user.Phone,
                Address1 = user.Address1,
                Address2 = user.Address2,
                Town = user.Town,
                State = user.State,
                Country = user.Country,
                CreatedAt = user.CreatedAt,
                ModifiedAt = user.ModifiedAt,
                Ipaddress = user.Ipaddress,
                LastLogin = user.LastLogin,
                IsActive = user.IsActive,
                StateId = stateId,
                CountryId = countryId
            };
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(UserProfileViewModel model)
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

            long stateId = 0;
            if (long.TryParse(model.State?.ToString(), out var parsedState))
                stateId = parsedState;

            long countryId = 0;
            if (long.TryParse(model.Country?.ToString(), out var parsedCountry))
                countryId = parsedCountry;

            ViewBag.Countries = _db.Countries
               .Select(c => new
               {
                   countryID = c.CountryId,
                   countryName = c.Name
               })
               .OrderBy(c => c.countryID)
               .ToList();

            ViewBag.states = _db.States
               .Select(s => new
               {
                   stateID = s.StateId,
                   stateName = s.Name
               })
               .OrderBy(s => s.stateID)
               .ToList();
            // Update fields
         
            model.Ipaddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            string hashedPassword = HashPassword(model.Password);

            try
            {
                long newUserId;
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("sp_CreateUser", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Title", model.Title);
                    cmd.Parameters.AddWithValue("@FirstName", model.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", model.LastName);
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    // Only update password if user entered a new one
                    if (!string.IsNullOrWhiteSpace(model.Password) && model.Password != "********")
                    {
                        cmd.Parameters.AddWithValue("@Password", HashPassword(model.Password));
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Password", DBNull.Value);
                    }

                    cmd.Parameters.AddWithValue("@Role", "Citizen");
                    cmd.Parameters.AddWithValue("@Phone", model.Phone ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address1", model.Address1);
                    cmd.Parameters.AddWithValue("@Address2", model.Address2 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Town", model.Town ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@State", model.State);
                    cmd.Parameters.AddWithValue("@Country", model.Country);
                    cmd.Parameters.AddWithValue("@IPAddress", model.Ipaddress ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserId", model.UserId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    newUserId = Convert.ToInt64(model.UserId);
                }

                // log in AuditLogs
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("INSERT INTO AuditLogs(UserID, Action, Timestamp, Notes) VALUES(@UserID, @Action, GETDATE(), @Notes)", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", newUserId);
                    cmd.Parameters.AddWithValue("@Action", "User Updated");
                    cmd.Parameters.AddWithValue("@Notes", $"User {model.Email} updated with role Citizen");
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ViewBag.Message = "Profile updated successful";
                return RedirectToAction("Profile", "Account");
                //return View("Profile", model);
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("Email already exists"))
                    ModelState.AddModelError("Email", "This email is already registered.");
                ViewBag.Error = "This email is already registered.";
                return View("Profile",model);
                throw;
            }

        }

    }
}

