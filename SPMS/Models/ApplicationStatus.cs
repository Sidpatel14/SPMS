using System;
using System.Collections.Generic;

namespace SPMS.Models;

public partial class ApplicationStatus
{
    public long StatusId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsFinal { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
}
