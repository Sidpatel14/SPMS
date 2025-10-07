public class CitizenDashboardViewModel
{
    public Int64 TotalApplications { get; set; }
    public Int64 Submitted { get; set; }
    public Int64 UnderReview { get; set; }
    public Int64 Approved { get; set; }
    public Int64 Rejected { get; set; }
    public List<CitizenAppRow> LatestApplications { get; set; } = new();
}

public class CitizenAppRow
{
    public Int64 ApplicationID { get; set; }
    public string ReferenceNumber { get; set; } = null!;
    public string PermitType { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime SubmissionDate { get; set; }
    public string State { get; set; } = null!;
    public string Country { get; set; } = null!;
}