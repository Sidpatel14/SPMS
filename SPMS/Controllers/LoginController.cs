using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPMS.Models;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using BCrypt.Net;



namespace SPMS.Controllers
{

    public class LoginController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public LoginController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult CitizenLogin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UserLogin([FromBody] User model)
        {
           
                string connectionString = _context.Database.GetDbConnection().ConnectionString;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT Password, Role FROM users WHERE Email = @Email";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", model.Email);
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedHashedPassword = reader.GetString(0);
                                string role = reader.GetString(1);

                                // Verify password
                                if (BCrypt.Net.BCrypt.Verify(model.Password, storedHashedPassword))
                                {
                                // Password correct, set session or cookie
                                //HttpContext.Session.SetString("UserEmail", model.Email);
                                //HttpContext.Session.SetString("UserRole", role);
                                return CitizenLogin();// or wherever
                            }
                            }
                        }
                    }
                }

                ModelState.AddModelError("", "Invalid email or password.");

           return CitizenLogin();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CitizenLogin(User model)
        {
            model.Ipaddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            if (ModelState.IsValid)
            {
                try

                {
                    

                    // Hash password before saving
                    model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

                    string connectionString = _context.Database.GetDbConnection().ConnectionString;
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        string query = @"
                            INSERT INTO Users
                            (Title, FirstName, LastName, Email, Password, Role, Phone, Address1, Address2, Town, State, Country, IPAddress)
                            VALUES
                            (@Title, @FirstName, @LastName, @Email, @Password, @Role, @Phone, @Address1, @Address2, @Town, @State, @Country, @IPAddress)";

                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@Title", model.Title);
                            cmd.Parameters.AddWithValue("@FirstName", model.FirstName);
                            cmd.Parameters.AddWithValue("@LastName", model.LastName);
                            cmd.Parameters.AddWithValue("@Email", model.Email);
                            cmd.Parameters.AddWithValue("@Password", model.Password); // Hash in production
                            cmd.Parameters.AddWithValue("@Role", model.Role);
                            cmd.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(model.Phone) ? (object)DBNull.Value : model.Phone);
                            cmd.Parameters.AddWithValue("@Address1", model.Address1);
                            cmd.Parameters.AddWithValue("@Address2", string.IsNullOrEmpty(model.Address2) ? (object)DBNull.Value : model.Address2);
                            cmd.Parameters.AddWithValue("@Town", string.IsNullOrEmpty(model.Town) ? (object)DBNull.Value : model.Town);
                            cmd.Parameters.AddWithValue("@State", model.State);
                            cmd.Parameters.AddWithValue("@Country", model.Country);
                            cmd.Parameters.AddWithValue("@IPAddress", HttpContext.Connection.RemoteIpAddress?.ToString());

                            con.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }

                    TempData["Success"] = "Registration successful!";
                    return RedirectToAction("CitizenLogin");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            return View(model);
        }

    }
}




