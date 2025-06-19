using System.Data.SQLite;
using System.IO;
using System.Windows;
using TaskTurner.DataService;
namespace TaskTurner
{
    public partial class AuthorizationWindow : Window
    {
        public AuthorizationWindow()
        {
            DatabaseHelper.InitializeDatabase();
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
            string username = usernameBox.Text.Trim();
            string password = passwordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string dbPath = "users.db";

            try
            {
                if (!File.Exists(dbPath))
                {
                    MessageBox.Show("База данных не найдена. Зарегистрируйтесь сначала.", "Ошибка",
                                 MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using (SQLiteConnection connection = new SQLiteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    // 1. Проверяем существование пользователя
                    string authQuery = "SELECT COUNT(*) FROM Users WHERE Username = @username AND Password = @password";
                    using (SQLiteCommand command = new SQLiteCommand(authQuery, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);

                        long count = Convert.ToInt64(command.ExecuteScalar());

                        if (count > 0)
                        {
                            // 2. Получаем ID пользователя
                            int userId = GetUserId(username);
                            if (userId == -1)
                            {
                                MessageBox.Show("Ошибка получения данных пользователя.", "Ошибка",
                                              MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            // 3. Открываем главное окно
                            var mainWindow = new MainWindow(userId);
                            Application.Current.MainWindow = mainWindow;
                            mainWindow.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Неверный логин или пароль.", "Ошибка",
                                          MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show($"Ошибка базы данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неизвестная ошибка: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}