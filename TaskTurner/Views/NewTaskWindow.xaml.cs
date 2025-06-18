using System.Data.SQLite;
using System.Windows;

namespace TaskTurner.Views
{
    public partial class NewTaskWindow : Window
    {
        private int _userId;

        public NewTaskWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
            EnsureDatabase();
        }

        private void EnsureDatabase()
        {
            using var connection = new SQLiteConnection("Data Source=users.db");
            connection.Open();
            string createTable = @"
                        CREATE TABLE IF NOT EXISTS Tasks (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER,
                        Title TEXT NOT NULL,
                        Description TEXT,
                        FOREIGN KEY(UserId) REFERENCES Users(Id)
                    );";

            using var command = new SQLiteCommand(createTable, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTask_Click(object sender, RoutedEventArgs e)
        {
            string title = TaskTitleBox.Text;
            string description = TaskDescriptionBox.Text;

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Введите название задачи");
                return;
            }
            using var connection = new SQLiteConnection("Data Source=users.db");
            connection.Open();

            string insertQuery = "INSERT INTO Tasks (UserId, Title, Description) VALUES (@userId, @title, @desc)";
            using var command = new SQLiteCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@userId", _userId);
            command.Parameters.AddWithValue("@title", title);
            command.Parameters.AddWithValue("@desc", description);
            command.ExecuteNonQuery();

            this.Close();
        }
    }
}
