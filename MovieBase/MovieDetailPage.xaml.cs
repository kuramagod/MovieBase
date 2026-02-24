using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MovieBase
{
    /// <summary>
    /// Логика взаимодействия для MovieDetailPage.xaml
    /// </summary>
    public partial class MovieDetailPage : Page
    {
        public MovieDetailPage(database.Movie selectedMovie)
        {
            InitializeComponent();

            var movieWithData = MovieBaseContext.GetContext().Movies
                .Include(m => m.Contry)
                .Include(m => m.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefault(m => m.Movieid == selectedMovie.Movieid);

            this.DataContext = movieWithData;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    
    }
}
