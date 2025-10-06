using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SPMS.Models;
using System.Reflection.Metadata;
using static System.Net.Mime.MediaTypeNames;

namespace SPMS.Controllers
{
    public class CitizenController : Controller
    {
        private readonly SpmsContext _db;
        public CitizenController(SpmsContext context)
        {
            _db = context;
        }

        public IActionResult Dashboard()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }


            return View();
        }
        public IActionResult Apply()
        {

            return View();
        }

        [HttpPost]
        public ActionResult Apply(ApplicationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ApplyForPermit", model);
            }
            int newApplicationId = 0;
            var userId = HttpContext.Session.GetString("UserID");
            string connectionString = _db.Database.GetDbConnection().ConnectionString;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(@"INSERT INTO Applications 
                (PermitType, UserId,Status,Address1, Address2, Town, State, Country,Comments, SubmissionDate) 
                VALUES 
                (@PermitType, @userId,1,@Address1, @Address2, @Town, @State, @Country,@Comments, GETDATE());
        SELECT SCOPE_IDENTITY();", con);

                cmd.Parameters.AddWithValue("@PermitType", model.PermitType);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@Address1", model.Address1);
                cmd.Parameters.AddWithValue("@Address2", model.Address2 ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Town", model.Town ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@State", model.State);
                cmd.Parameters.AddWithValue("@Country", model.Country);
                cmd.Parameters.AddWithValue("@Comments", model.Comments);
                newApplicationId = Convert.ToInt32(cmd.ExecuteScalar());
                //newApplicationId = result != null ? Convert.ToInt32(result) : 0;
            }

            // Handle uploaded files
            if (model.doc != null && model.doc.Any())
            {

                var uploadFolder = Path.Combine("wwwroot/uploads", "uploads");
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                foreach (var file in model.doc)
                {
                    if (file.Length > 0)
                    {

                        var fileName = Path.GetFileName(file.FileName);
                        var filePath = Path.Combine(uploadFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyToAsync(stream);
                        }

                        using (SqlConnection con = new SqlConnection(connectionString))
                        {
                            con.Open();
                            SqlCommand cmd = new SqlCommand(@"INSERT INTO Documents 
                (ApplicationId, FileName, FilePath, UploadedAt, DocumentType) 
                VALUES 
                (@ApplicationId, @FileName, @FilePath, GETDATE(), @DocumentType)", con);

                            cmd.Parameters.AddWithValue("@ApplicationId", newApplicationId);
                            cmd.Parameters.AddWithValue("@FileName", fileName);
                            cmd.Parameters.AddWithValue("@FilePath", filePath);
                            cmd.Parameters.AddWithValue("@DocumentType", file.ContentType);
                            cmd.ExecuteScalar();
                        }
                    }
                }
            }

            TempData["SuccessMessage"] = "Application submitted successfully!";
            return View();
        }


        public ActionResult MyApplications(string search, string status, DateTime? fromDate, DateTime? toDate)
        {
            // Fetch list from DB based on filter criteria
            //var apps = db.Applications.Where(...).ToList();
            return View();
        }

    }
}
