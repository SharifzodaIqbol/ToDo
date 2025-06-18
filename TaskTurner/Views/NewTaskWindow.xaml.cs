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
        }
        private void CreateTask_Click(object sender, RoutedEventArgs e)
        {
            string title = TaskTitleBox.Text.Trim();
            string description = TaskDescriptionBox.Text.Trim();

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

            this.DialogResult = true;
            this.Close();
        }
    }
}