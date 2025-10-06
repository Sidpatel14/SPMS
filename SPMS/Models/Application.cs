using System;
using System.Collections.Generic;

namespace SPMS.Models;

public partial class Application
{
    public long ApplicationId { get; set; }

    public long ApplicantId { get; set; }

    public long PermitTypeId { get; set; }

    public long StatusId { get; set; }

    public string ReferenceNumber { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public DateTime? LastUpdatedAt { get; set; }

    public long? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? Comments { get; set; }

    public string Address1 { get; set; } = null!;

    public string? Address2 { get; set; }

    public string? Town { get; set; }

    public string State { get; set; } = null!;

    public string Country { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual User Applicant { get; set; } = null!;

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual PermitType PermitType { get; set; } = null!;

    public virtual ApplicationStatus Status { get; set; } = null!;
}
