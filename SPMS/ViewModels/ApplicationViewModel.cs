using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


public class ApplicationsFilterViewModel
{
    public string SelectedStatus { get; set; }
    public List<SelectListItem> StatusList { get; set; } = new();
    public List<ApplicationViewModel> Applications { get; set; } = new();
    public string Search { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class ApplicationViewModel
{
    public Int64 ApplicationId { get; set; }
    public string ReferenceNumber { get; set; } = null!;
    public Int64 UserId { get; set; }

    public string PermitType { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime? SubmissionDate { get; set; }

    public DateTime? LastUpdated { get; set; }

    public Int64? StaffId { get; set; }

    public string? Comments { get; set; }
    public string Address1 { get; set; } = null!;

    public string? Address2 { get; set; }

    public string? Town { get; set; }

    public string State { get; set; } = null!;

    public string Country { get; set; } = null!;
    // Use IFormFile for single or multiple uploads
    public List<IFormFile>? doc { get; set; }
    public string? CitizenName { get; set; }
}
