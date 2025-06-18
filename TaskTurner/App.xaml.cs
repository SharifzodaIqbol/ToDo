using System.Configuration;
using System.Data;
using System.Windows;

namespace TaskTurner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_Startup(object sender, StartupEventArgs e)
        {
            // Явно создаём окно авторизации
            var authWindow = new AuthorizationWindow();

            Application.Current.MainWindow = authWindow;

            // Показываем
            authWindow.Show();
        }

    }
}
