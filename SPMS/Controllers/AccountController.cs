using Microsoft.AspNetCore.Http;
//using System.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPMS.Models;
using System;
using System.Data.SqlClient;
using System.Linq;
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

       
    }
}

