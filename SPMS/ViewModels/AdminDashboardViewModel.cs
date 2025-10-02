public class AdminDashboardViewModel
{
    public int TotalApplications { get; set; }
    public int Submitted { get; set; }
    public int UnderReview { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public List<AdminAppRow> LatestApplications { get; set; } = new();
}

public class AdminAppRow
{
    public int ApplicationID { get; set; }
    public string CitizenName { get; set; } = null!;
    public string PermitType { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime SubmissionDate { get; set; }
    public string State { get; set; } = null!;
    public string Country { get; set; } = null!;
}

public class AdminUserRow
{
    public int UserID { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminAppDetail
{
    public int ApplicationID { get; set; }
    public string PermitType { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime SubmissionDate { get; set; }
    public string CitizenName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Address1 { get; set; } = null!;
    public string Address2 { get; set; } = null!;
    public string Town { get; set; } = null!;
    public string State { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string Comments { get; set; } = null!;
}

public class PermitTypeRow
{
    public int PermitTypeID { get; set; }
    public string TypeName { get; set; } = null!;
    public string Description { get; set; } = null!;
}

public class AuditLogRow
{
    public int LogID { get; set; }
    public int ApplicationID { get; set; }
    public string Action { get; set; } = null!;
    public string PerformedBy { get; set; } = null!;
    public DateTime PerformedAt { get; set; }
}