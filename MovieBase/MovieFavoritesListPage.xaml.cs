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

        public class FavoriteItemViewModel
        {
            public database.Favorite Favorite { get; set; }
            public database.Movie Movie => Favorite.Movie;
            public string UserRatingDisplay { get; set; }
            public SolidColorBrush RatingColor { get; set; }
        }


        private void LoadFavorites()
        {
            if (AppSession.CurrentUser == null) return;

            var context = MovieBaseContext.GetContext();

            // 1. Получаем список избранного
            var favorites = context.Favorites
                .Include(f => f.Movie)
                .Where(f => f.Userid == AppSession.CurrentUser.Userid)
                .OrderByDescending(f => f.Date)
                .ToList();

            // 2. Получаем все рецензии текущего пользователя
            var userReviews = context.Reviews
                .Where(r => r.Userid == AppSession.CurrentUser.Userid)
                .ToList();

            // 3. Формируем список для отображения (соединяем данные)
            var displayList = favorites.Select(f => {
                // Ищем рецензию пользователя для этого конкретного фильма
                var review = userReviews.FirstOrDefault(r => r.Movieid == f.Movieid);

                bool hasRating = review != null && review.Rating > 0;

                return new FavoriteItemViewModel
                {
                    Favorite = f,
                    UserRatingDisplay = hasRating ? review.Rating.ToString() : "?",
                    RatingColor = hasRating ? new SolidColorBrush(Color.FromRgb(255, 193, 7))
                                            : new SolidColorBrush(Color.FromRgb(158, 158, 158))
                };
            }).ToList();

            FavoritesListControl.ItemsSource = displayList;

            // Управление сообщением о пустом списке
            EmptyListMessage.Visibility = displayList.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

    }
}
