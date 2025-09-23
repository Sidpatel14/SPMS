using System;
using System.Collections.Generic;

namespace SPMS.Models;

public partial class Application
{
    public int ApplicationId { get; set; }

    public int UserId { get; set; }

    public string PermitType { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime? SubmissionDate { get; set; }

    public DateTime? LastUpdated { get; set; }

    public int? StaffId { get; set; }

    public string? Comments { get; set; }
}
