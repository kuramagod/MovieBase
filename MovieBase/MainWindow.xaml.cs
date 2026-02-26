using Microsoft.EntityFrameworkCore;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigated += MainFrame_Navigated;
            MainFrame.Navigate(new MoviesListPage());
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is MoviesListPage)
            {
                SearchContainer.Visibility = Visibility.Visible;
            }
            else
            {
                SearchContainer.Visibility = Visibility.Collapsed;
            }
        }


        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            SearchTextBox.Focus();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Logo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SearchTextBox.Text = string.Empty;

            MainFrame.Navigate(new MoviesListPage());
        }

        private void FavoritesButton_Click(object sender, RoutedEventArgs e)
        {

            if (AppSession.CurrentUser == null)
            {
                MessageBox.Show("Сначала необходимо авторизоваться!");
                return;
            }

            MainFrame.Navigate(new MovieFavoritesListPage());
        }
    }
}