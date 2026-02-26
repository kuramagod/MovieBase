using MovieBase.database;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovieBase
{
    public static class AppSession
    {
        public static User? CurrentUser { get; set; }
    }
}
