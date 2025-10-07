using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SPMS.Models;
using System.Data;
using System.Data.Entity;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using static System.Net.Mime.MediaTypeNames;

namespace SPMS.Controllers
{
    public class CitizenController : BaseController
    {
        private readonly SpmsContext _db;
        private readonly string connectionString;
        public CitizenController(SpmsContext context)
        {
            _db = context;
            connectionString = _db.Database.GetDbConnection().ConnectionString;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.Session.GetString("UserID");
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(userId))
               return RedirectToAction("Login", "Account");  
           
            if (role != "Citizen")
                return RedirectToAction("AccessDenied", "Account");

            List<CitizenDashboardViewModel> applications = new();
            var model = new CitizenDashboardViewModel();
            model.LatestApplications = new List<CitizenAppRow>();

            using (var conn = _db.Database.GetDbConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_CitizenDashboardApplications";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ApplicantID", userId));

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            model.TotalApplications = reader.GetInt64(0);
                            model.Submitted = reader.GetInt64(1);
                            model.UnderReview = reader.GetInt64(2);
                            model.Approved = reader.GetInt64(3);
                            model.Rejected = reader.GetInt64(4);
                        }

                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                model.LatestApplications.Add(new CitizenAppRow
                                {
                                    ApplicationID = reader.GetInt64(0),
                                    ReferenceNumber = reader.GetString(1),
                                    PermitType = reader.GetString(2),
                                    Status = reader.GetString(3),
                                    SubmissionDate = reader.GetDateTime(4)
                                });
                            }
                        }
                    }
                }
            }

            return View(model);
        }
        public IActionResult Apply()
        {
            // Fetch permit types from the database
            var permitTypes = _db.PermitTypes
                     .Select(p => new { p.PermitTypeId, p.Name })
                     .ToList<object>();

            // Pass to view
            ViewBag.PermitTypes = permitTypes;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Apply(ApplicationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ApplyForPermit", model);
            }
            // 1. Build the documents DataTable
            var dt = new DataTable();
            dt.Columns.Add("FileName", typeof(string));
            dt.Columns.Add("FilePath", typeof(string));
            dt.Columns.Add("DocumentType", typeof(string));

            if (model.doc != null && model.doc.Any())
            {
                var uploadFolder = Path.Combine("wwwroot", "uploads");
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                var maxFileSize = 5 * 1024 * 1024;

                foreach (var file in model.doc)
                {
                    if (file.Length <= 0) continue;

                    var ext = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(ext)) continue;
                    if (file.Length > maxFileSize) continue;

                    var uniqueFileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    dt.Rows.Add(uniqueFileName, filePath, ext);
                }
            }
            // 2. Call stored procedure
            Int64 newApplicationId;
            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                using (var cmd = new SqlCommand("sp_ApplyNewPermit", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@ApplicantID", Convert.ToInt64(HttpContext.Session.GetString("UserID")));
                    cmd.Parameters.AddWithValue("@PermitTypeID", Convert.ToInt64(model.PermitType));
                    cmd.Parameters.AddWithValue("@Title", "");
                    cmd.Parameters.AddWithValue("@Description", "");
                    cmd.Parameters.AddWithValue("@Address1", model.Address1);
                    cmd.Parameters.AddWithValue("@Address2", (object?)model.Address2 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Town", (object?)model.Town ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@State", model.State);
                    cmd.Parameters.AddWithValue("@Country", model.Country);
                    cmd.Parameters.AddWithValue("@Comments", (object?)model.Comments ?? DBNull.Value);

                    var tvpParam = cmd.Parameters.AddWithValue("@Documents", dt);
                    tvpParam.SqlDbType = SqlDbType.Structured;
                    tvpParam.TypeName = "dbo.DocumentListType";

                    newApplicationId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }

            // 3. Return result
            TempData["SuccessMessage"] = "Application submitted successfully!";
            return RedirectToAction("Dashboard", "Citizen");

        }

        public async Task<IActionResult> MyApplications(string search, string status, DateTime? fromDate, DateTime? toDate)
        {
            List<ApplicationViewModel> applications = new List<ApplicationViewModel>();
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetCitizenApplications", con))
            {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ApplicantID", userId));
                    cmd.Parameters.Add(new SqlParameter("@Search", (object)search ?? DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@Status", (object)status ?? DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@FromDate", (object)fromDate ?? DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@ToDate", (object)toDate ?? DBNull.Value));

                await con.OpenAsync();
                using (SqlDataReader rdr = await cmd.ExecuteReaderAsync())
                {
                    while (await rdr.ReadAsync())
                    {
                        applications.Add(new ApplicationViewModel
                        {
                            ApplicationId = rdr.GetInt64(0),
                            ReferenceNumber = rdr.GetString(1),
                            PermitType = rdr.GetString(2),
                            Status = rdr.GetString(3),
                            SubmissionDate = rdr.GetDateTime(4),
                            LastUpdated = rdr.IsDBNull(5) ? null : rdr.GetDateTime(5)
                        });
                    }
                }
            }

            return View(applications);
        }

        [HttpGet]
        public IActionResult ApplicationDetail(Int64 id) // id = ApplicationId from Citizen list
        {
            var model = GetApplicationDetail(id);
            if (model == null)
            {
                return NotFound();
            }
            return View("ApplicationDetail", model);
        }

        private ApplicationDetailViewModel GetApplicationDetail(Int64 applicationId)
        {
           
            var model = new ApplicationDetailViewModel();
            model.Documents = new List<DocumentViewModel>();
            model.AuditLogs = new List<AuditLogViewModel>();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetApplicationDetail", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ApplicationId", applicationId); // pass ID instead of number
                con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    // --- 1. Application Info ---
                    if (reader.Read())
                    {
                        model.ApplicationNumber = reader["ApplicationNumber"].ToString();
                        model.PermissionType = reader["PermissionType"].ToString();
                        model.Status = reader["Status"].ToString();
                        model.SubmittedDate = Convert.ToDateTime(reader["SubmittedDate"]);
                        model.LastUpdated = Convert.ToDateTime(reader["LastUpdated"]);
                        model.FullAddress = reader["FullAddress"].ToString();
                        model.Comment = reader["Comment"].ToString();
                    }

                    // --- 2. Documents ---
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            model.Documents.Add(new DocumentViewModel
                            {
                                Id = Convert.ToInt32(reader["DocumentId"]),
                                Name = reader["DocumentName"].ToString(),
                                Type = reader["DocumentType"].ToString(),
                                UploadedDate = Convert.ToDateTime(reader["UploadedDate"])
                            });
                        }
                    }

                    // --- 3. Audit Logs ---
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            model.AuditLogs.Add(new AuditLogViewModel
                            {
                                Action = reader["Action"].ToString(),
                                PerformedBy = reader["PerformedBy"].ToString(),
                                Timestamp = Convert.ToDateTime(reader["Timestamp"])
                            });
                        }
                    }
                }
            }

            return model.ApplicationNumber == null ? null : model;
        }


    }
}
