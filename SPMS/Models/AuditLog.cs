using System;
using System.Collections.Generic;

namespace SPMS.Models;

public partial class AuditLog
{
    public long AuditId { get; set; }

    public long UserId { get; set; }

    public string Action { get; set; } = null!;

    public long? ApplicationId { get; set; }

    public DateTime? Timestamp { get; set; }

    public string? Notes { get; set; }

    public virtual Application? Application { get; set; }

    public virtual User User { get; set; } = null!;
}
