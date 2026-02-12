using System;
using System.Collections.Generic;

namespace MovieBase.database;

public partial class Favorite
{
    public int Favoriteid { get; set; }

    public int? Userid { get; set; }

    public int Movieid { get; set; }

    public DateOnly? Date { get; set; }

    public virtual Movie Movie { get; set; } = null!;

    public virtual User? User { get; set; }
}
