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
    public string CitizenName { get; set; }
    public string PermitType { get; set; }
    public string Status { get; set; }
    public DateTime SubmissionDate { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
}

public class AdminUserRow
{
    public int UserID { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminAppDetail
{
    public int ApplicationID { get; set; }
    public string PermitType { get; set; }
    public string Status { get; set; }
    public DateTime SubmissionDate { get; set; }
    public string CitizenName { get; set; }
    public string Email { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string Town { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
    public string Comments { get; set; }
}

public class PermitTypeRow
{
    public int PermitTypeID { get; set; }
    public string TypeName { get; set; }
    public string Description { get; set; }
}

public class AuditLogRow
{
    public int LogID { get; set; }
    public int ApplicationID { get; set; }
    public string Action { get; set; }
    public string PerformedBy { get; set; }
    public DateTime PerformedAt { get; set; }
}