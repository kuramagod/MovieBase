using System;
using System.Collections.Generic;

namespace MovieBase;

public partial class Review
{
    public int Reviewid { get; set; }

    public int? Userid { get; set; }

    public string? Title { get; set; }

    public string? Text { get; set; }

    public int? Movieid { get; set; }

    public DateOnly? Date { get; set; }

    public virtual Movie? Movie { get; set; }

    public virtual User? User { get; set; }
}
