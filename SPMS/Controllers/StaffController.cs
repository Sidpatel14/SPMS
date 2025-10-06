using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SPMS.Models;
using System.Data;

namespace SPMS.Controllers
{
    public class StaffController : Controller
    {
        private readonly SpmsContext _db;
        public StaffController(SpmsContext context)
        {
            _db = context;

        }
        // Staff Dashboard - show all pending or under-review applications
        public IActionResult Dashboard()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Staff")
                return RedirectToAction("AccessDenied", "Account");

            List<StaffViewModel> applications = new();
            string connectionString = _db.Database.GetDbConnection().ConnectionString;

            var model = new StaffViewModel();

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
                        model.LatestApplications.Add(new StaffAppRow
                        {
                            ApplicationID = Convert.ToInt32(r["ApplicationID"]),
                            PermitType = r["PermitType"]?.ToString() ?? string.Empty,
                            Status = r["Status"].ToString() ?? string.Empty,
                            SubmissionDate = Convert.ToDateTime(r["SubmissionDate"]),
                            CitizenName = r["CitizenName"].ToString() ?? string.Empty
                        });
                    }
                }
            }

            //using (SqlConnection con = new SqlConnection(connectionString))
            //{
            //    con.Open();
            //    SqlCommand cmd = new SqlCommand(@"
            //    SELECT A.ApplicationID, A.PermitType, A.Status, A.SubmissionDate, 
            //           U.FirstName + ' ' + U.LastName AS CitizenName, A.Address1, A.State, A.Country
            //    FROM Applications A
            //    INNER JOIN Users U ON A.UserID = U.UserID
            //    WHERE A.Status IN ('Submitted', 'Under Review', 'MoreInfoRequired')", con);

            //    SqlDataReader dr = cmd.ExecuteReader();
            //    while (dr.Read())
            //    {
            //        applications.Add(new StaffViewModel
            //        {
            //            ApplicationId = Convert.ToInt32(dr["ApplicationID"]),
            //            CitizenName = dr["CitizenName"].ToString(),
            //            PermitType = dr["PermitType"]?.ToString() ?? string.Empty,
            //            Status = dr["Status"].ToString(),
            //            SubmissionDate = Convert.ToDateTime(dr["SubmissionDate"]),
            //            Address1 = dr["Address1"]?.ToString() ?? string.Empty,
            //            State = dr["State"]?.ToString() ?? string.Empty,
            //            Country = dr["Country"]?.ToString() ?? string.Empty,
            //        });
            //    }
            //}
            return View(model);
        }

        // GET: View details of a specific application
        public IActionResult Details(int id)
        {
            ApplicationViewModel app = new ApplicationViewModel();
            string connectionString = _db.Database.GetDbConnection().ConnectionString;
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
            string connectionString = _db.Database.GetDbConnection().ConnectionString;
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


