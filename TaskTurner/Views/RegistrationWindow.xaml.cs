using System.Data.SQLite;
using System.Windows;
using System.IO;

namespace TaskTurner
{
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
            EnsureDatabase();
        }
        public void EnsureDatabase()
        {
            string dbFile = "users.db";
            string connectionString = $"Data Source={dbFile}";

            // Если базы нет — создаём файл
            if (!File.Exists(dbFile))
            {
                SQLiteConnection.CreateFile(dbFile);
            }

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string createTableQuery = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL,
                Password TEXT NOT NULL
            );";

                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = usernameBox.Text;
            string password = passwordBox.Password;
            string repeat_password = repeat_passwordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(repeat_password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (password != repeat_password)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                string dbPath = "Data Source=users.db";

                using (SQLiteConnection connection = new SQLiteConnection(dbPath))
                {
                    connection.Open();

                    string query = "INSERT INTO Users (Username, Password) VALUES (@Username, @Password)";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", password);

                        command.ExecuteNonQuery();
                    }
                    string idQuery = "SELECT Id FROM Users WHERE Username = @Username";
                    using (SQLiteCommand idCommand = new SQLiteCommand(idQuery, connection))
                    {
                        idCommand.Parameters.AddWithValue("@Username", username);
                        int userId = Convert.ToInt32(idCommand.ExecuteScalar());

                        MainWindow mainWindow = new MainWindow(userId);
                        mainWindow.Show();
                        Application.Current.MainWindow.Close();
                        Application.Current.MainWindow = mainWindow;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при регистрации: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
