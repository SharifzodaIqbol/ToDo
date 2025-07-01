using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using TaskTurner.Models;
using System;
using System.Linq;

namespace TaskTurner.Views
{
    public partial class NewTaskWindow : Window
    {
        private int _userId;
        private readonly TaskModel _editingTask;

        public ObservableCollection<Project> Projects { get; set; }
        public ObservableCollection<Category> Categories { get; set; }
        public ObservableCollection<Tag> Tags { get; set; }

        public int? SelectedProjectId { get; set; }
        public int? SelectedCategoryId { get; set; }
        public int? SelectedTagId { get; set; }

        public NewTaskWindow(
            int userId,
            TaskModel taskToEdit = null,
            ObservableCollection<Project> projects = null,
            ObservableCollection<Category> categories = null,
            ObservableCollection<Tag> tags = null)
        {
            InitializeComponent();

            _userId = userId;
            _editingTask = taskToEdit;
            this.Title = _editingTask == null ? "Добавить задачу" : "Редактировать задачу";

            Projects = projects ?? new ObservableCollection<Project>();
            Categories = categories ?? new ObservableCollection<Category>();
            Tags = tags ?? new ObservableCollection<Tag>();

            // Добавляем элемент "Без тега" в начало списка, если его ещё нет
            if (!Tags.Any(t => t.Id == 0))
            {
                Tags.Insert(0, new Tag
                {
                    Id = 0,
                    Name = "Без тега"
                });
            }

            if (_editingTask != null)
            {
                // Подставляем данные в поля
                TaskTitleBox.Text = _editingTask.Title;
                TaskDescriptionBox.Text = _editingTask.Description;
                ImportanceBox.SelectedItem = _editingTask.TaskImportance;

                SelectedProjectId = _editingTask.ProjectId ?? 0;
                SelectedCategoryId = _editingTask.CategoryId ?? 0;
                SelectedTagId = _editingTask.TagId ?? 0;
            }
            else
            {
                SelectedProjectId = 0;
                SelectedCategoryId = 0;
                SelectedTagId = 0;
            }

            this.DataContext = this;
        }

        private void CreateTask_Click(object sender, RoutedEventArgs e)
        {
            string taskimportance = ImportanceBox.SelectedItem as string ?? "Без приоритета";

            int? selectedProjectId = SelectedProjectId;
            int? selectedCategoryId = SelectedCategoryId;
            int? selectedTagId = SelectedTagId;

            // Проверка на отсутствие изменений
            if (_editingTask != null &&
                TaskTitleBox.Text == _editingTask.Title &&
                TaskDescriptionBox.Text == _editingTask.Description &&
                taskimportance == _editingTask.TaskImportance &&
                selectedProjectId == (_editingTask.ProjectId ?? 0) &&
                selectedCategoryId == (_editingTask.CategoryId ?? 0) &&
                selectedTagId == (_editingTask.TagId ?? 0))
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

            int? finalProjectId = (selectedProjectId == 0) ? (int?)null : selectedProjectId;
            int? finalCategoryId = (selectedCategoryId == 0) ? (int?)null : selectedCategoryId;
            int? finalTagId = (selectedTagId == 0) ? (int?)null : selectedTagId;

            try
            {
                if (_editingTask == null)
                {
                    string insertQuery = @"
                        INSERT INTO Tasks
                        (UserId, Title, Description, IsCompleted, TaskImportance, ProjectId, CategoryId, TagId)
                        VALUES
                        (@userId, @title, @desc, 0, @taskImportance, @projectId, @categoryId, @tagId)";

                    using var command = new SQLiteCommand(insertQuery, connection);
                    command.Parameters.AddWithValue("@userId", _userId);
                    command.Parameters.AddWithValue("@title", title);
                    command.Parameters.AddWithValue("@desc", description);
                    command.Parameters.AddWithValue("@taskImportance", taskimportance);
                    command.Parameters.AddWithValue("@projectId", finalProjectId.HasValue ? (object)finalProjectId.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@categoryId", finalCategoryId.HasValue ? (object)finalCategoryId.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@tagId", finalTagId.HasValue ? (object)finalTagId.Value : DBNull.Value);
                    command.ExecuteNonQuery();
                }
                else
                {
                    string updateQuery = @"
                        UPDATE Tasks
                        SET Title = @title,
                            Description = @desc,
                            IsCompleted = @isCompleted,
                            TaskImportance = @taskImportance,
                            ProjectId = @projectId,
                            CategoryId = @categoryId,
                            TagId = @tagId
                        WHERE Id = @id";

                    using var command = new SQLiteCommand(updateQuery, connection);
                    command.Parameters.AddWithValue("@title", title);
                    command.Parameters.AddWithValue("@desc", description);
                    command.Parameters.AddWithValue("@isCompleted", _editingTask.IsCompleted);
                    command.Parameters.AddWithValue("@taskImportance", taskimportance);
                    command.Parameters.AddWithValue("@projectId", finalProjectId.HasValue ? (object)finalProjectId.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@categoryId", finalCategoryId.HasValue ? (object)finalCategoryId.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@tagId", finalTagId.HasValue ? (object)finalTagId.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@id", _editingTask.Id);
                    command.ExecuteNonQuery();
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении запроса: {ex.Message}");
            }
        }
    }
}
