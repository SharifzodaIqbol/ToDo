using System.Windows;
using System.IO;
using System.Data.SQLite;

namespace TaskTurner
{
    public partial class AuthorizationWindow : Window
    {
        public AuthorizationWindow()
        {
            InitializeComponent();
        }
        private int GetUserId(string username)
        {
            using (var connection = new SQLiteConnection("Data Source=users.db"))
            {
                connection.Open();
                string query = "SELECT Id FROM Users WHERE Username = @username";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
            }
        }
        private void Registration_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow regWindow = new RegistrationWindow();
            regWindow.Show();
        }
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = usernameBox.Text;
            string password = passwordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string dbPath = "users.db";
            string connectionString = $"Data Source={dbPath}";

            if (!File.Exists(dbPath))
            {
                MessageBox.Show("Неверный логин или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Users WHERE Username = @username AND Password = @password";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);

                    long count = (long)command.ExecuteScalar();

                    if (count > 0)
                    {
                        int userId = GetUserId(username); // Получи ID пользователя из БД
                        var mainWindow = new MainWindow(userId);
                        Application.Current.MainWindow = mainWindow;
                        mainWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Неверный логин или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}