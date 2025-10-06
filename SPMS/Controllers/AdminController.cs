using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPMS.Models;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace SPMS.Controllers
{
    public class AdminController : BaseController
    {
        private readonly SpmsContext _db;
        private readonly string connectionString;

        public AdminController(SpmsContext context)
        {
            _db = context;
             connectionString = _db.Database.GetDbConnection().ConnectionString;
        }

        [HttpGet]
        public ActionResult CreateStaff()
        {
            var role = HttpContext.Session?.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Admin")
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public ActionResult CreateStaff(User model)
        {
            var role = HttpContext.Session?.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Admin")
                return RedirectToAction("Login", "Account");

            // 1. Generate random password
            string randomPassword = GenerateRandomPassword(10);
            string hashedPassword = HashPassword(randomPassword);
            model.Ipaddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            string connectionString = _db.Database.GetDbConnection().ConnectionString;
            if (ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(@"INSERT INTO Users 
                (Title, FirstName, LastName, Email, Password, Role, Phone, Address1, Address2, Town, State, Country, CreatedAt) 
                VALUES 
                (@Title, @FirstName, @LastName, @Email, @Password, 'Staff', @Phone, @Address1, @Address2, @Town, @State, @Country, GETDATE())", con);

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

               
            }

            // 2. Email credentials to the staff
            SendCredentialsEmail(model.Email, randomPassword);

            ViewBag.Message = "Staff account created and credentials emailed successfully.";
            return View();
        }
        private string GenerateRandomPassword(int length)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789@$!%*?&";
            StringBuilder result = new StringBuilder();
            byte[] uintBuffer = new byte[sizeof(uint)];
            while (result.Length < length)
            {
                System.Security.Cryptography.RandomNumberGenerator.Fill(uintBuffer);
                uint num = BitConverter.ToUInt32(uintBuffer, 0);
                result.Append(validChars[(int)(num % (uint)validChars.Length)]);
            }
            return result.ToString();
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        private void SendCredentialsEmail(string email, string password)
        {
            MailMessage mail = new MailMessage("yourapp@email.com", email);
            mail.Subject = "Your Staff Account Credentials";
            mail.Body = $"Your account has been created.\n\nEmail: {email}\nPassword: {password}\n\nPlease change it after first login.";

            SmtpClient smtp = new SmtpClient("smtp.yourmailserver.com", 587)
            {
                Credentials = new NetworkCredential("yourapp@email.com", "yourpassword"),
                EnableSsl = true
            };
            smtp.Send(mail);
        }
        // Role check helper
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        private IActionResult? RequireAdmin()
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
            return null;
        }

        // Dashboard: counts + latest applications
        public IActionResult Dashboard()
        {
            //var reject = RequireAdmin();
            //if (reject != null) return reject;
            var userId = HttpContext.Session.GetString("UserID");
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
                return RedirectToAction("AccessDenied", "Account");

            var model = new AdminDashboardViewModel();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Counts
                using (var cmd = new SqlCommand(@"
                    SELECT 
                      (SELECT COUNT(1) FROM Applications) AS TotalApps,
                      (SELECT COUNT(1) FROM Applications WHERE Status = 'Submitted') AS Submitted,
                      (SELECT COUNT(1) FROM Applications WHERE Status = 'Under Review') AS UnderReview,
                      (SELECT COUNT(1) FROM Applications WHERE Status = 'Approved') AS Approved,
                      (SELECT COUNT(1) FROM Applications WHERE Status = 'Rejected') AS Rejected
                    ", conn))
                {
                    using var r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        model.TotalApplications = Convert.ToInt32(r["TotalApps"]);
                        model.Submitted = Convert.ToInt32(r["Submitted"]);
                        model.UnderReview = Convert.ToInt32(r["UnderReview"]);
                        model.Approved = Convert.ToInt32(r["Approved"]);
                        model.Rejected = Convert.ToInt32(r["Rejected"]);
                    }
                }

                // Latest 8 applications
                using (var cmd = new SqlCommand(@"
                    SELECT TOP 8 A.ApplicationID, A.PermitType, A.Status, A.SubmissionDate,
                           U.FirstName + ' ' + U.LastName AS CitizenName
                    FROM Applications A
                    INNER JOIN Users U ON A.UserID = U.UserID
                    ORDER BY A.SubmissionDate DESC", conn))
                {
                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        model.LatestApplications.Add(new AdminAppRow
                        {
                            ApplicationID = Convert.ToInt32(r["ApplicationID"]),
                            PermitType = r["PermitType"].ToString() ?? string.Empty,
                            Status = r["Status"].ToString() ?? string.Empty,
                            SubmissionDate = Convert.ToDateTime(r["SubmissionDate"]),
                            CitizenName = r["CitizenName"].ToString() ?? string.Empty
                        });
                    }
                }
            }

            return View(model);
        }

        // Users list
        public IActionResult Users()
        {
            var reject = RequireAdmin();
            if (reject != null) return reject;

            var list = new List<AdminUserRow>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using var cmd = new SqlCommand("SELECT UserID, Title, FirstName, LastName, Email, Role, IsActive, CreatedAt FROM Users ORDER BY CreatedAt DESC", conn);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    list.Add(new AdminUserRow
                    {
                        UserID = Convert.ToInt32(r["UserID"]),
                        FullName = $"{r["Title"]} {r["FirstName"]} {r["LastName"]}".Trim(),
                        Email = r["Email"].ToString() ?? string.Empty,
                        Role = r["Role"].ToString() ?? string.Empty,
                        IsActive = Convert.ToBoolean(r["IsActive"]),
                        CreatedAt = Convert.ToDateTime(r["CreatedAt"])
                    });
                }
            }
            return View(list);
        }

        // Toggle user active/inactive
        [HttpPost]
        public IActionResult ToggleUserActive(int id, bool active)
        {
            var reject = RequireAdmin();
            if (reject != null) return reject;

            using var conn = new SqlConnection(connectionString);
            conn.Open();
            using var cmd = new SqlCommand("UPDATE Users SET IsActive = @active, ModifiedAt = GETDATE() WHERE UserID = @id", conn);
            cmd.Parameters.AddWithValue("@active", active);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            return RedirectToAction("Users");
        }

        // Applications list (filterable via querystring)
        public IActionResult Applications(string status = "", string type = "")
        {
            var reject = RequireAdmin();
            if (reject != null) return reject;

            var list = new List<AdminAppRow>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var sql = @"
                    SELECT A.ApplicationID, A.PermitType, A.Status, A.SubmissionDate,
                           U.FirstName + ' ' + U.LastName AS CitizenName, A.State, A.Country
                    FROM Applications A
                    INNER JOIN Users U ON A.UserID = U.UserID
                    WHERE 1=1
                ";

                if (!string.IsNullOrEmpty(status)) sql += " AND A.Status = @status";
                if (!string.IsNullOrEmpty(type)) sql += " AND A.PermitType LIKE @type";

                sql += " ORDER BY A.SubmissionDate DESC";

                using var cmd = new SqlCommand(sql, conn);
                if (!string.IsNullOrEmpty(status)) cmd.Parameters.AddWithValue("@status", status);
                if (!string.IsNullOrEmpty(type)) cmd.Parameters.AddWithValue("@type", $"%{type}%");

                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    list.Add(new AdminAppRow
                    {
                        ApplicationID = Convert.ToInt32(r["ApplicationID"]),
                        PermitType = r["PermitType"].ToString() ?? string.Empty,
                        Status = r["Status"].ToString() ?? string.Empty,
                        SubmissionDate = Convert.ToDateTime(r["SubmissionDate"]),
                        CitizenName = r["CitizenName"].ToString() ?? string.Empty,
                        State = r["State"].ToString() ?? string.Empty,
                        Country = r["Country"].ToString() ?? string.Empty
                    });
                }
            }
            return View(list);
        }

        // Applications -> details (reuse existing staff details or show read-only)
        public IActionResult ApplicationDetails(int id)
        {
            var reject = RequireAdmin();
            if (reject != null) return reject;

            AdminAppDetail detail = new AdminAppDetail();
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT A.*, U.FirstName + ' ' + U.LastName AS CitizenName, U.Email
                FROM Applications A
                INNER JOIN Users U ON A.UserID = U.UserID
                WHERE A.ApplicationID = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var r = cmd.ExecuteReader();
            if (r.Read())
            {
                detail = new AdminAppDetail
                {
                    ApplicationID = Convert.ToInt32(r["ApplicationID"]),
                    PermitType = r["PermitType"].ToString() ?? string.Empty,
                    Status = r["Status"].ToString() ?? string.Empty,
                    SubmissionDate = Convert.ToDateTime(r["SubmissionDate"]),
                    CitizenName = r["CitizenName"].ToString() ?? string.Empty,
                    Email = r["Email"].ToString() ?? string.Empty,
                    Address1 = r["Address1"].ToString() ?? string.Empty,
                    Address2 = r["Address2"]?.ToString() ?? string.Empty,
                    Town = r["Town"]?.ToString() ?? string.Empty,
                    State = r["State"]?.ToString() ?? string.Empty,
                    Country = r["Country"]?.ToString() ?? string.Empty,
                    Comments = r["Comments"]?.ToString() ?? string.Empty
                };
            }
            return View(detail);
        }

        // PermitTypes management
        public IActionResult PermitTypes()
        {
            var reject = RequireAdmin();
            if (reject != null) return reject;

            var list = new List<PermitTypeRow>();
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            using var cmd = new SqlCommand("SELECT PermitTypeID, TypeName, Description FROM PermitTypes ORDER BY TypeName", conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new PermitTypeRow
                {
                    PermitTypeID = Convert.ToInt32(r["PermitTypeID"]),
                    TypeName = r["TypeName"].ToString() ?? string.Empty,
                    Description = r["Description"]?.ToString() ?? string.Empty
                });
            }
            return View(list);
        }

        [HttpPost]
        public IActionResult AddPermitType(string typeName, string description)
        {
            var reject = RequireAdmin();
            if (reject != null) return reject;
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            using var cmd = new SqlCommand("INSERT INTO PermitTypes (TypeName, Description) VALUES (@t,@d)", conn);
            cmd.Parameters.AddWithValue("@t", typeName);
            cmd.Parameters.AddWithValue("@d", description ?? (object)DBNull.Value);
            cmd.ExecuteNonQuery();
            return RedirectToAction("PermitTypes");
        }

        [HttpPost]
        public IActionResult DeletePermitType(int id)
        {
            var reject = RequireAdmin();
            if (reject != null) return reject;
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            using var cmd = new SqlCommand("DELETE FROM PermitTypes WHERE PermitTypeID = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            return RedirectToAction("PermitTypes");
        }

        // Audit logs (simple list)
        public IActionResult AuditLogs()
        {
            var reject = RequireAdmin();
            if (reject != null) return reject;

            var logs = new List<AuditLogRow>();
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            using var cmd = new SqlCommand("SELECT TOP 200 LogID, ApplicationID, Action, PerformedBy, PerformedAt FROM AuditLogs ORDER BY PerformedAt DESC", conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                logs.Add(new AuditLogRow
                {
                    LogID = Convert.ToInt32(r["LogID"]),
                    ApplicationID = r["ApplicationID"] != DBNull.Value ? Convert.ToInt32(r["ApplicationID"]) : 0,
                    Action = r["Action"].ToString() ?? string.Empty,
                    PerformedBy = r["PerformedBy"].ToString() ?? string.Empty,
                    PerformedAt = Convert.ToDateTime(r["PerformedAt"])
                });
            }
            return View(logs);
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
    }

}
