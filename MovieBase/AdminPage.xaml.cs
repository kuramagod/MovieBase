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
using System.IO;
using MovieBase.Сonverters;

namespace MovieBase
{
    /// <summary>
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            RefreshData();
            UpdateAddButtonVisibility();
        }
        private string GetCurrentTableTag()
        {
            return (AdminTabs.SelectedItem as TabItem)?.Tag.ToString();
        }

        private void UpdateAddButtonVisibility()
        {
            string currentTable = GetCurrentTableTag();

            // Скрываем кнопку для вкладок "Рецензии" и "Избранное"
            if (currentTable == "Reviews" || currentTable == "Favorites")
            {
                AddButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                AddButton.Visibility = Visibility.Visible;
            }
        }

        private void RefreshData()
        {
            var context = MovieBaseContext.GetContext();
            string currentTable = GetCurrentTableTag();

            switch (currentTable)
            {
                case "Movies":
                    MoviesGrid.ItemsSource = context.Movies.Include(m => m.Contry).ToList();
                    break;
                case "Users":
                    UsersGrid.ItemsSource = context.Users.Include(u => u.Role).ToList();
                    break;
                case "Reviews":
                    ReviewsGrid.ItemsSource = context.Reviews.Include(r => r.Movie).Include(r => r.User).ToList();
                    break;
                case "Genres":
                    GenresGrid.ItemsSource = context.Genres.ToList();
                    break;
                case "Countries":
                    CountriesGrid.ItemsSource = context.Countries.ToList();
                    break;
                case "Roles":
                    RolesGrid.ItemsSource = context.Roles.ToList();
                    break;
                case "Favorites":
                    FavoritesGrid.ItemsSource = context.Favorites.Include(f => f.Movie).Include(f => f.User).ToList();
                    break;
            }
        }
        private void AdminTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                RefreshData();
                UpdateAddButtonVisibility();
            }
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext;
            if (MessageBox.Show("Удалить эту запись?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var context = MovieBaseContext.GetContext();
                context.Remove(item);
                context.SaveChanges();
                RefreshData();
            }
        }

        // Кнопка добавления
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            object newItem = null;
            string currentTable = GetCurrentTableTag();

            switch (currentTable)
            {
                case "Movies": newItem = new database.Movie(); break;
                case "Users": newItem = new database.User(); break;
                case "Reviews": newItem = new database.Review(); break;
                case "Genres": newItem = new database.Genre(); break;
                case "Countries": newItem = new database.Country(); break;
                case "Roles": newItem = new database.Role(); break;
                case "Favorites": newItem = new database.Favorite(); break;
            }

            if (newItem != null)
            {
                AdminEditWindow editWin = new AdminEditWindow(newItem, isNew: true);
                if (editWin.ShowDialog() == true) RefreshData();
            }
        }

        // Кнопка редактирования
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (sender as Button).DataContext;
            if (selectedItem != null)
            {
                AdminEditWindow editWin = new AdminEditWindow(selectedItem, isNew: false);
                if (editWin.ShowDialog() == true) RefreshData();
            }
        }

        // Поиск (пример для фильмов и пользователей)
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filter = SearchBox.Text.ToLower();
            var context = MovieBaseContext.GetContext();
            string currentTable = GetCurrentTableTag();

            if (currentTable == "Movies")
                MoviesGrid.ItemsSource = context.Movies.Include(m => m.Contry)
                    .Where(m => m.Title.ToLower().Contains(filter)).ToList();

            else if (currentTable == "Users")
                UsersGrid.ItemsSource = context.Users.Include(u => u.Role)
                    .Where(u => u.Username.ToLower().Contains(filter)).ToList();
        }
    }
}
