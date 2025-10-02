public class StaffViewModel
{
    public int TotalApplications { get; set; }
    public int Submitted { get; set; }
    public int UnderReview { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public List<StaffAppRow> LatestApplications { get; set; } = new();
}

public class StaffAppRow
{
    public int ApplicationID { get; set; }
    public string CitizenName { get; set; } = null!;
    public string PermitType { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime SubmissionDate { get; set; }
    public string State { get; set; } = null!;
    public string Country { get; set; } = null!;
}