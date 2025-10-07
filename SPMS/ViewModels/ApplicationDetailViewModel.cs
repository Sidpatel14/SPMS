
public class ApplicationDetailViewModel
{
    public string ApplicationNumber { get; set; }
    public string PermissionType { get; set; }
    public string Status { get; set; }
    public DateTime SubmittedDate { get; set; }
    public DateTime LastUpdated { get; set; }
    public string FullAddress { get; set; }
    public string Comment { get; set; }
    public List<DocumentViewModel> Documents { get; set; }
    public List<AuditLogViewModel> AuditLogs { get; set; }
}

public class DocumentViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public DateTime UploadedDate { get; set; }
}

public class AuditLogViewModel
{
    public string Action { get; set; }
    public string PerformedBy { get; set; }
    public DateTime Timestamp { get; set; }
    public string Details { get; set; }
}
