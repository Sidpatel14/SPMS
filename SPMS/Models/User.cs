using System;
using System.Collections.Generic;

namespace SPMS.Models;

public partial class User
{
    public long UserId { get; set; }

    public string Title { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string? Phone { get; set; }

    public string Address1 { get; set; } = null!;

    public string? Address2 { get; set; }

    public string? Town { get; set; }

    public string State { get; set; } = null!;

    public string Country { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string? Ipaddress { get; set; }

    public DateTime? LastLogin { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Application> ApplicationApplicants { get; set; } = new List<Application>();

    public virtual ICollection<Application> ApplicationApprovedByNavigations { get; set; } = new List<Application>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
