using System;
using System.Collections.Generic;

namespace SPMS.Models;

public partial class Workflow
{
    public long WorkflowId { get; set; }

    public long PermitTypeId { get; set; }

    public int StepNumber { get; set; }

    public string RoleAssigned { get; set; } = null!;

    public bool? IsFinalStep { get; set; }

    public virtual PermitType PermitType { get; set; } = null!;
}
