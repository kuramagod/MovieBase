using Microsoft.EntityFrameworkCore;
using MovieBase.database;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private database.Movie _currentMovie;
        private Review _existingUserReview;
        private int _selectedRating = 0;
        
        public MovieDetailPage(database.Movie selectedMovie)
        {
            InitializeComponent();

            _currentMovie = selectedMovie;

            var context = MovieBaseContext.GetContext();

            var movieWithData = context.Movies
                .Include(m => m.Contry)
                .Include(m => m.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefault(m => m.Movieid == selectedMovie.Movieid);

            this.DataContext = movieWithData;

            LoadReviews();

            UpdateFavoriteButtonState();
            CheckUserReviewStatus();
        }

        private void LoadReviews()
        {
            var context = MovieBaseContext.GetContext();

            var reviews = context.Reviews
                .Include(r => r.User)
                .Where(r => r.Movieid == _currentMovie.Movieid)
                .OrderByDescending(r => r.Date)
                .ToList();

            ReviewsList.ItemsSource = reviews;
        }

        private void CheckUserReviewStatus()
        {
            if (AppSession.CurrentUser == null) return;

            var context = MovieBaseContext.GetContext();

            // Загружаем существующий отзыв текущего пользователя для текущего фильма
            _existingUserReview = context.Reviews
                .Include(r => r.User)
                .FirstOrDefault(r => r.Userid == AppSession.CurrentUser.Userid && r.Movieid == _currentMovie.Movieid);

            if (_existingUserReview != null)
            {
                WriteReviewBtn.Content = "Редактировать рецензию";
                WriteReviewBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA000"));
                WriteReviewBtn.Opacity = 1;
                WriteReviewBtn.IsEnabled = true;
            }
            else
            {
                WriteReviewBtn.Content = "Написать рецензию";
                WriteReviewBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC107"));
                WriteReviewBtn.Opacity = 1;
                WriteReviewBtn.IsEnabled = true;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void UpdateFavoriteButtonState()
        {
            if (AppSession.CurrentUser == null) return;

            var context = MovieBaseContext.GetContext();

            bool isFavorite = context.Favorites.Any(f =>
                f.Userid == AppSession.CurrentUser.Userid &&
                f.Movieid == _currentMovie.Movieid);

            if (isFavorite)
            {
                FavoriteBtn.Content = "Убрать из избранного";
                FavoriteBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#424242"));
            }
            else
            {
                FavoriteBtn.Content = "Добавить в избранное";
                FavoriteBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D32F2F"));
            }
        }

        private void Add_Favorite(object sender, RoutedEventArgs e)
        {
            if (AppSession.CurrentUser == null)
            {
                MessageBox.Show("Пожалуйста, войдите в аккаунт, чтобы добавлять фильмы в избранное.");
                return;
            }

            var context = MovieBaseContext.GetContext();

            var favoriteEntry = context.Favorites.FirstOrDefault(f =>
                f.Userid == AppSession.CurrentUser.Userid &&
                f.Movieid == _currentMovie.Movieid);

            if (favoriteEntry != null)
            {
                context.Favorites.Remove(favoriteEntry);
            }
            else
            {
                var newFavorite = new Favorite
                {
                    Userid = AppSession.CurrentUser.Userid,
                    Movieid = _currentMovie.Movieid,
                    Date = DateOnly.FromDateTime(DateTime.Now)
                };
                context.Favorites.Add(newFavorite);
            }

            context.SaveChanges();

            UpdateFavoriteButtonState();
        }

        private void OpenReviewForm_Click(object sender, RoutedEventArgs e)
        {
            if (AppSession.CurrentUser == null)
            {
                MessageBox.Show("Войдите, чтобы оставить отзыв");
                return;
            }

            // Блокируем скролл страницы
            MainScrollViewer.IsHitTestVisible = false;

            // Загружаем существующий отзыв, если есть
            if (_existingUserReview != null)
            {
                _selectedRating = _existingUserReview.Rating ?? 0;
                ReviewTitleTxt.Text = _existingUserReview.Title;
                ReviewTextTxt.Text = _existingUserReview.Text;

                // Подсвечиваем звезды
                UpdateStarsDisplay(_selectedRating);

                RatingDisplay.Text = $"{_selectedRating}/10";

                // Меняем заголовок и кнопку
                ReviewFormTitle.Text = "Редактировать рецензию";
                SubmitReviewBtn.Content = "Сохранить изменения";
            }
            else
            {
                // Очищаем форму для нового отзыва
                _selectedRating = 0;
                ReviewTitleTxt.Text = "";
                ReviewTextTxt.Text = "";

                // Сбрасываем звезды
                UpdateStarsDisplay(0);

                RatingDisplay.Text = "0/10";

                // Меняем заголовок и кнопку
                ReviewFormTitle.Text = "Ваша рецензия";
                SubmitReviewBtn.Content = "Опубликовать";
            }

            ReviewOverlay.Visibility = Visibility.Visible;
        }

        // Вспомогательный метод для обновления звезд
        private void UpdateStarsDisplay(int rating)
        {
            for (int i = 0; i < StarsStack.Children.Count; i++)
            {
                var star = StarsStack.Children[i] as Button;
                if (star != null)
                {
                    star.Foreground = i < rating
                        ? Brushes.Gold
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAAAAA"));
                }
            }
        }

        private void CloseReviewForm_Click(object sender, RoutedEventArgs e)
        {
            ReviewOverlay.Visibility = Visibility.Collapsed;
            MainScrollViewer.IsHitTestVisible = true;
        }

        private void Star_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            _selectedRating = int.Parse(btn.Tag.ToString());

            UpdateStarsDisplay(_selectedRating);

            RatingDisplay.Text = $"{_selectedRating}/10";
        }

        private void SubmitReview_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRating == 0)
            {
                MessageBox.Show("Пожалуйста, поставьте оценку звездами.");
                return;
            }

            if (string.IsNullOrWhiteSpace(ReviewTitleTxt.Text))
            {
                MessageBox.Show("Пожалуйста, введите заголовок рецензии.");
                ReviewTitleTxt.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(ReviewTextTxt.Text))
            {
                MessageBox.Show("Пожалуйста, введите текст рецензии.");
                ReviewTextTxt.Focus();
                return;
            }

            var context = MovieBaseContext.GetContext();

            if (_existingUserReview != null)
            {
                _existingUserReview.Title = ReviewTitleTxt.Text;
                _existingUserReview.Text = ReviewTextTxt.Text;
                _existingUserReview.Rating = _selectedRating;
                _existingUserReview.Date = DateOnly.FromDateTime(DateTime.Now);

                context.Reviews.Update(_existingUserReview);
            }
            else
            {
                var newReview = new Review
                {
                    Userid = AppSession.CurrentUser.Userid,
                    Movieid = _currentMovie.Movieid,
                    Title = ReviewTitleTxt.Text,
                    Text = ReviewTextTxt.Text,
                    Rating = _selectedRating,
                    Date = DateOnly.FromDateTime(DateTime.Now)
                };

                context.Reviews.Add(newReview);
            }

            context.SaveChanges();

            var movieReviews = context.Reviews.Where(r => r.Movieid == _currentMovie.Movieid).ToList();

            if (movieReviews.Any())
            {
                double average = movieReviews.Average(r => (double)r.Rating);

                var movieToUpdate = context.Movies.Find(_currentMovie.Movieid);
                if (movieToUpdate != null)
                {
                    movieToUpdate.Rating = average.ToString("F1").Replace(',', '.');
                    context.SaveChanges();

                    _currentMovie.Rating = movieToUpdate.Rating;
                }
            }

            ReviewOverlay.Visibility = Visibility.Collapsed;
            MainScrollViewer.IsHitTestVisible = true;

            CheckUserReviewStatus();

            LoadReviews();
        }
    }
}
