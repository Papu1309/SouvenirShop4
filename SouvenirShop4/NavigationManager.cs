using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SouvenirShop4.Connect;

namespace SouvenirShop4
{
    
    
        public static class NavigationManager
        {
            public static Users CurrentUser { get; set; }

            public static bool IsAdmin
            {
                get
                {
                    return CurrentUser != null && CurrentUser.Username == "admin";
                }
            }

            public static void ShowMainWindow()
            {
                if (CurrentUser != null)
                {
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();

                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window is LoginWindow)
                            window.Close();
                    }
                }
            }

            public static void ShowLoginWindow()
            {
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                foreach (Window window in Application.Current.Windows)
                {
                    if (window is MainWindow)
                        window.Close();
                }
            }
        }
    
}
