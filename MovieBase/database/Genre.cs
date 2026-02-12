using System;
using System.Collections.Generic;

namespace MovieBase.database;

public partial class Genre
{
    public int Genreid { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();
}
