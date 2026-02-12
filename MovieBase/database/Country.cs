using System;
using System.Collections.Generic;

namespace MovieBase.database;

public partial class Country
{
    public int Contryid { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();
}
