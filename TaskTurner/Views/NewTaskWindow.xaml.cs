using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using TaskTurner.Models;

namespace TaskTurner.Views
{
    public partial class NewTaskWindow : Window
    {
        private int _userId;

        private readonly TaskModel _editingTask;
        public NewTaskWindow(int userId, TaskModel taskToEdit = null)
        {
            InitializeComponent();
            _userId = userId;
            _editingTask = taskToEdit;
            this.Title = _editingTask == null ? "Добавить задачу" : "Редактировать задачу";
            // Если передана задача для редактирования - заполняем поля
            if (_editingTask != null)
            {
                TaskTitleBox.Text = _editingTask.Title;
                TaskDescriptionBox.Text = _editingTask.Description;
                ImportanceBox.Text = _editingTask.Description;
            }
        }
        private void CreateTask_Click(object sender, RoutedEventArgs e)
        {
            if (_editingTask != null &&
                TaskTitleBox.Text == _editingTask.Title &&
                TaskDescriptionBox.Text == _editingTask.Description &&
                ImportanceBox.Text == _editingTask.TaskImportance)  // Используем ComboBox вместо StackPanel
            {
                this.DialogResult = false;
                this.Close();
                return;
            }

            string title = TaskTitleBox.Text.Trim();
            string description = TaskDescriptionBox.Text.Trim();
            string taskimportance = ImportanceBox.SelectedValue?.ToString();

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Введите название задачи");
                return;
            }

            using var connection = new SQLiteConnection("Data Source=users.db");
            connection.Open();

            if (_editingTask == null)
            {
                string insertQuery = "INSERT INTO Tasks (UserId, Title, Description, IsCompleted, TaskImportance) VALUES (@userId, @title, @desc, 0, @taskImportance)";
                using var command = new SQLiteCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@userId", _userId);
                command.Parameters.AddWithValue("@title", title);
                command.Parameters.AddWithValue("@desc", description);
                command.Parameters.AddWithValue("@taskImportance", taskimportance);
                command.ExecuteNonQuery();
            }
            else
            {
                string updateQuery = @"
            UPDATE Tasks 
            SET Title = @title,
                Description = @desc,
                IsCompleted = @isCompleted,
                TaskImportance = @taskImportance
            WHERE Id = @id";

                using var command = new SQLiteCommand(updateQuery, connection);
                command.Parameters.AddWithValue("@title", title);
                command.Parameters.AddWithValue("@desc", description);
                command.Parameters.AddWithValue("@isCompleted", _editingTask.IsCompleted);
                command.Parameters.AddWithValue("@taskImportance", taskimportance);  // Используем новое значение
                command.Parameters.AddWithValue("@id", _editingTask.Id);
                command.ExecuteNonQuery();
            }

            this.DialogResult = true;
            this.Close();
        }
    }
}