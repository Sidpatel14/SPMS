using System;
using System.Collections.Generic;

namespace SPMS.Models;

public partial class State
{
    public long StateId { get; set; }

    public string Name { get; set; } = null!;

    public string CountryIso { get; set; } = null!;

    public virtual Country CountryIsoNavigation { get; set; } = null!;
}
