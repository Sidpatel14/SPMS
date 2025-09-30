using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class ApplicationViewModel
{
    public int ApplicationId { get; set; }

    public int UserId { get; set; }

    public string PermitType { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime? SubmissionDate { get; set; }

    public DateTime? LastUpdated { get; set; }

    public int? StaffId { get; set; }

    public string? Comments { get; set; }
    public string Address1 { get; set; } = null!;

    public string? Address2 { get; set; }

    public string? Town { get; set; }

    public string State { get; set; } = null!;

    public string Country { get; set; } = null!;
    // Use IFormFile for single or multiple uploads
    public List<IFormFile>? doc { get; set; }
}
