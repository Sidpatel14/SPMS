using System;
using System.Collections.Generic;

namespace SPMS.Models;

public partial class Country
{
    public long CountryId { get; set; }

    public string Name { get; set; } = null!;

    public string Iso { get; set; } = null!;

    public virtual ICollection<State> States { get; set; } = new List<State>();
}
