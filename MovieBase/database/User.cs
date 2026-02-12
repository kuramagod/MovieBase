using System;
using System.Collections.Generic;

namespace MovieBase.database;

public partial class User
{
    public int Userid { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public int? Roleid { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Role? Role { get; set; }
}
