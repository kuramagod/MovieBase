using MovieBase.database;
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
using System.Windows.Shapes;

namespace MovieBase
{
    /// <summary>
    /// Логика взаимодействия для RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void Authorization_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void Registration_Click(object sender, RoutedEventArgs e)
        {
            var context = MovieBaseContext.GetContext();
            var existingUser = context.Users.FirstOrDefault(u => u.Username == loginTxt.Text);
            var existingEmail = context.Users.FirstOrDefault(u => u.Email == emailTxt.Text);
            if (existingUser != null || existingEmail != null)
            {
                MessageBox.Show("Пользователь или почтой с таким именем уже существует");
                return;
            }
            User newUser = new User
            {
                Username = loginTxt.Text,
                Password = passwordTxt.Password,
                Email = emailTxt.Text,
                Roleid = 1 
            };
            context.Users.Add(newUser);
            context.SaveChanges();
            MessageBox.Show("Регистрация прошла успешно");
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
