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
    /// Логика взаимодействия для ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            LoadUserData();
        }

        private void LoadUserData()
        {
            var user = AppSession.CurrentUser;
            if (user == null) return;

            var context = MovieBaseContext.GetContext();

            UsernameTxt.Text = user.Username;
            EmailTxt.Text = user.Email ?? "Почта не привязана";

            var userRole = context.Roles.FirstOrDefault(r => r.Roleid == user.Roleid);
            RoleTxt.Text = userRole?.Name?.ToUpper() ?? "ПОЛЬЗОВАТЕЛЬ";

            int reviewsCount = context.Reviews.Count(r => r.Userid == user.Userid);
            ReviewsCountTxt.Text = reviewsCount.ToString();

            int favoritesCount = context.Favorites.Count(f => f.Userid == user.Userid);
            FavoritesCountTxt.Text = favoritesCount.ToString();

            var userReviews = context.Reviews.Where(r => r.Userid == user.Userid).ToList();
            if (userReviews.Any())
            {
                double avg = userReviews.Average(r => (double)(r.Rating ?? 0));
                AvgRatingTxt.Text = avg.ToString("F1").Replace(',', '.');
            }
            else
            {
                AvgRatingTxt.Text = "—";
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                AppSession.CurrentUser = null;

                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                Window.GetWindow(this).Close();
            }
        }
    }
}
