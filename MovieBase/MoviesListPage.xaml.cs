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
    /// Логика взаимодействия для MoviesListPage.xaml
    /// </summary>
    public partial class MoviesListPage : Page
    {
        public MoviesListPage()
        {
            InitializeComponent();
            LoadMovies();
        }

        private void CardFilm_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is database.Movie selectedMovie)
            {
                NavigationService.Navigate(new MovieDetailPage(selectedMovie));
            }
        }

        private void LoadMovies()
        {
            var movies = MovieBaseContext.GetContext().Movies.Include(m => m.Genre).ToList();
            MoviesList.ItemsSource = movies;
        }

    }
}
