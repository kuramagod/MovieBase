using System;
using System.Collections.Generic;

namespace MovieBase.database;

public partial class Movie
{
    public int Movieid { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public byte[]? Cover { get; set; }

    public string? Year { get; set; }

    public int? Contryid { get; set; }

    public int Genreid { get; set; }

    public string? Rating { get; set; }

    public virtual Country? Contry { get; set; }

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual Genre Genre { get; set; } = null!;

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
