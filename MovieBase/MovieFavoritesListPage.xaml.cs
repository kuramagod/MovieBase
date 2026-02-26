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
    /// Логика взаимодействия для MovieFavoritesListPage.xaml
    /// </summary>
    public partial class MovieFavoritesListPage : Page
    {
        public MovieFavoritesListPage()
        {
            InitializeComponent();
            LoadFavorites();
        }


        private void LoadFavorites()
        {
            if (AppSession.CurrentUser == null)
            {
                MessageBox.Show("Ошибка: Пользователь не авторизован.");
                return;
            }

            var context = MovieBaseContext.GetContext();

            var userFavorites = context.Favorites
                .Include(f => f.Movie)
                .Where(f => f.Userid == AppSession.CurrentUser.Userid)
                .OrderByDescending(f => f.Date)
                .ToList();

            FavoritesListControl.ItemsSource = userFavorites;

            if (userFavorites.Count == 0)
            {
                EmptyListMessage.Visibility = Visibility.Visible;
            }
            else
            {
                EmptyListMessage.Visibility = Visibility.Collapsed;
            }
        }

    }
}
