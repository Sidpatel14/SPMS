using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SPMS.Models;
using System.Data;

namespace SPMS.Controllers
{
    public class StaffController : BaseController
    {
        private readonly SpmsContext _db;
        private readonly string connectionString;

        public StaffController(SpmsContext context)
        {
            _db = context;
            connectionString = _db.Database.GetDbConnection().ConnectionString;
        }
        // Staff Dashboard - show all pending or under-review applications
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("Role");
            if (role != "Staff")
                return RedirectToAction("AccessDenied", "Account");

            List<StaffViewModel> applications = new();
            var model = new StaffViewModel();
            model.LatestApplications = new List<StaffAppRow>();

            using (var conn = _db.Database.GetDbConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_CitizenDashboard";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@CitizenID", userId));

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
                                model.LatestApplications.Add(new StaffAppRow
                                {
                                    ApplicationID = reader.GetInt64(0),
                                    CitizenName = reader.GetString(1),
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

        // GET: View details of a specific application
        public IActionResult Details(int id)
        {
            ApplicationViewModel app = new ApplicationViewModel();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(@"
                SELECT A.*, U.FirstName + ' ' + U.LastName AS CitizenName, U.Email
                FROM Applications A
                INNER JOIN Users U ON A.UserID = U.UserID
                WHERE A.ApplicationID = @id", con);

                cmd.Parameters.AddWithValue("@id", id);
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    app = new ApplicationViewModel
                    {
                        ApplicationId = Convert.ToInt32(dr["ApplicationID"]),
                        CitizenName = dr["CitizenName"].ToString(),
                        PermitType = dr["PermitType"].ToString() ?? string.Empty,
                        Status = dr["Status"].ToString(),
                        SubmissionDate = Convert.ToDateTime(dr["SubmissionDate"]),
                        Address1 = dr["Address1"].ToString() ?? string.Empty,
                        Address2 = dr["Address2"].ToString(),
                        State = dr["State"].ToString() ?? string.Empty,
                        Country = dr["Country"].ToString() ?? string.Empty,
                        Comments = dr["Comments"].ToString() ?? string.Empty,
                    };
                }
            }

            return View(app);
        }

        // POST: Update status (Approve / Reject / More Info)
        [HttpPost]
        public IActionResult UpdateStatus(int applicationId, string status, string comments)
        {
            int staffId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(@"
                UPDATE Applications 
                SET Status = @Status, Comments = @Comments, StaffID = @StaffID, LastUpdated = GETDATE()
                WHERE ApplicationID = @ApplicationID", con);

                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@Comments", (object)comments ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@StaffID", staffId);
                cmd.Parameters.AddWithValue("@ApplicationID", applicationId);

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Dashboard");
        }
    }


}


