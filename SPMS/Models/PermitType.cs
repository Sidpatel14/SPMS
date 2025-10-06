using System;
using System.Collections.Generic;

namespace SPMS.Models;

public partial class PermitType
{
    public long PermitTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual ICollection<Workflow> Workflows { get; set; } = new List<Workflow>();
}
