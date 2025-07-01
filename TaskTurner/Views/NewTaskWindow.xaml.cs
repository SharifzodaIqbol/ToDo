using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using TaskTurner.Models;
using System; // Добавьте это для DBNull

namespace TaskTurner.Views
{
    public partial class NewTaskWindow : Window
    {
        private int _userId;
        private readonly TaskModel _editingTask;

        public ObservableCollection<Project> Projects { get; set; }
        public ObservableCollection<Category> Categories { get; set; }
        public ObservableCollection<Tag> Tags { get; set; } // Add this property

        public int? SelectedProjectId { get; set; }
        public int? SelectedCategoryId { get; set; }
        public int? SelectedTagId { get; set; } // Ensure this property exists to bind SelectedValue

        public NewTaskWindow(int userId, TaskModel taskToEdit = null, ObservableCollection<Project> projects = null, ObservableCollection<Category> categories = null, ObservableCollection<Tag> tags = null) // Add tags parameter
        {
            InitializeComponent();
            _userId = userId;
            _editingTask = taskToEdit;
            this.Title = _editingTask == null ? "Добавить задачу" : "Редактировать задачу";

            Projects = projects ?? new ObservableCollection<Project>();
            Categories = categories ?? new ObservableCollection<Category>();
            Tags = tags ?? new ObservableCollection<Tag>(); // Initialize Tags collection

            this.DataContext = this;

            if (_editingTask != null)
            {
                TaskTitleBox.Text = _editingTask.Title;
                TaskDescriptionBox.Text = _editingTask.Description;

                foreach (ComboBoxItem item in ImportanceBox.Items)
                {
                    if (item.Content.ToString() == _editingTask.TaskImportance)
                    {
                        ImportanceBox.SelectedItem = item;
                        break;
                    }
                }

                SelectedProjectId = _editingTask.ProjectId;
                SelectedCategoryId = _editingTask.CategoryId;
                SelectedTagId = _editingTask.TagId;
            }
        }

        private void CreateTask_Click(object sender, RoutedEventArgs e)
        {
            // Получаем текстовое содержимое выбранного ComboBoxItem для важности
            string taskimportance = (ImportanceBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Получаем выбранные значения для Project, Category и Tag
            int? selectedProjectId = SelectedProjectId;
            int? selectedCategoryId = SelectedCategoryId;
            int? selectedTagId = SelectedTagId; // <<<<<< ДОБАВЛЕНО: получаем выбранный TagId

            // Проверка на изменения, если задача редактируется
            if (_editingTask != null &&
                TaskTitleBox.Text == _editingTask.Title &&
                TaskDescriptionBox.Text == _editingTask.Description &&
                taskimportance == _editingTask.TaskImportance && // Сравниваем текстовое значение
                selectedProjectId == _editingTask.ProjectId &&
                selectedCategoryId == _editingTask.CategoryId &&
                selectedTagId == _editingTask.TagId) // <<<<<< ДОБАВЛЕНО: сравниваем TagId
            {
                this.DialogResult = false;
                this.Close();
                return;
            }

            string title = TaskTitleBox.Text.Trim();
            string description = TaskDescriptionBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Введите название задачи");
                return;
            }

            using var connection = new SQLiteConnection("Data Source=users.db");
            connection.Open();

            // Преобразуем 0 в null для базы данных, если выбрано "Без проекта/категории/тега"
            int? finalProjectId = (selectedProjectId == 0) ? (int?)null : selectedProjectId;
            int? finalCategoryId = (selectedCategoryId == 0) ? (int?)null : selectedCategoryId;
            int? finalTagId = (selectedTagId == 0) ? (int?)null : selectedTagId; // <<<<<< ДОБАВЛЕНО: для тегов


            if (_editingTask == null)
            {
                // <<<<<< ИЗМЕНЕНО: Добавлены TagId в INSERT-запрос
                string insertQuery = "INSERT INTO Tasks (UserId, Title, Description, IsCompleted, TaskImportance, ProjectId, CategoryId, TagId) VALUES (@userId, @title, @desc, 0, @taskImportance, @projectId, @categoryId, @tagId)";
                using var command = new SQLiteCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@userId", _userId);
                command.Parameters.AddWithValue("@title", title);
                command.Parameters.AddWithValue("@desc", description);
                command.Parameters.AddWithValue("@taskImportance", taskimportance);
                command.Parameters.AddWithValue("@projectId", finalProjectId.HasValue ? (object)finalProjectId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@categoryId", finalCategoryId.HasValue ? (object)finalCategoryId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@tagId", finalTagId.HasValue ? (object)finalTagId.Value : DBNull.Value); // <<<<<< ДОБАВЛЕНО
                command.ExecuteNonQuery();
            }
            else
            {
                // <<<<<< ИЗМЕНЕНО: Добавлены TagId в UPDATE-запрос
                string updateQuery = @"
                    UPDATE Tasks
                    SET Title = @title,
                        Description = @desc,
                        IsCompleted = @isCompleted,
                        TaskImportance = @taskimportance,
                        ProjectId = @projectId,
                        CategoryId = @categoryId,
                        TagId = @tagId -- <<<<<< ДОБАВЛЕНО
                    WHERE Id = @id";

                using var command = new SQLiteCommand(updateQuery, connection);
                command.Parameters.AddWithValue("@title", title);
                command.Parameters.AddWithValue("@desc", description);
                command.Parameters.AddWithValue("@isCompleted", _editingTask.IsCompleted);
                command.Parameters.AddWithValue("@taskimportance", taskimportance);
                command.Parameters.AddWithValue("@projectId", finalProjectId.HasValue ? (object)finalProjectId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@categoryId", finalCategoryId.HasValue ? (object)finalCategoryId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@tagId", finalTagId.HasValue ? (object)finalTagId.Value : DBNull.Value); // <<<<<< ДОБАВЛЕНО
                command.Parameters.AddWithValue("@id", _editingTask.Id);
                command.ExecuteNonQuery();
            }
            this.DialogResult = true;
            this.Close();
        }
    }
}