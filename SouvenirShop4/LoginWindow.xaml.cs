using SouvenirShop4.Connect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SouvenirShop4
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            txtUsername.Focus();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                txtMessage.Text = "Введите логин и пароль!";
                return;
            }

            try
            {
                var user = Connection.entities.Users
                    .FirstOrDefault(u => u.Username == username && u.Password == password);

                if (user != null)
                {
                    NavigationManager.CurrentUser = user;
                    NavigationManager.ShowMainWindow();
                }
                else
                {
                    txtMessage.Text = "Неверный логин или пароль!";
                }
            }
            catch (System.Exception ex)
            {
                txtMessage.Text = $"Ошибка подключения: {ex.Message}";
            }
        }
    }
}
