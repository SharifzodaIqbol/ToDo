using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using TaskTurner.DataService;
using TaskTurner.Models;
using TaskTurner.Views;
using System; // Required for ArgumentNullException if used in RelayCommand constructor

namespace TaskTurner.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<TaskModel> UserTasks { get; set; } = new ObservableCollection<TaskModel>();
        public ObservableCollection<TaskModel> CompletedTasks { get; set; } = new ObservableCollection<TaskModel>();

        public ObservableCollection<Project> Projects { get; set; } = new ObservableCollection<Project>();
        public ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();
        public ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();

        public ICommand IOpenNewWindow => new RelayCommand(OpenNewWindow);
        public ICommand IDeleteTask => new RelayCommand<object>(DeleteTask);
        public ICommand IEditTask => new RelayCommand<object>(EditTask);
        public ICommand ICompleteTask => new RelayCommand<object>(CompleteTask);
        public ICommand ClearSearchCommand { get; }

        private string _newProjectName;
        public string NewProjectName
        {
            get => _newProjectName;
            set { _newProjectName = value; OnPropertyChanged(); }
        }

        private string _newCategoryName;
        public string NewCategoryName
        {
            get => _newCategoryName;
            set { _newCategoryName = value; OnPropertyChanged(); }
        }

        private string _newTagName;
        public string NewTagName
        {
            get => _newTagName;
            set { _newTagName = value; OnPropertyChanged(); }
        }

        public ICommand AddProjectCommand { get; }
        public ICommand AddCategoryCommand { get; }
        public ICommand AddTagCommand { get; }

        public ICollectionView FilteredTasksView => _filteredTasksView;

        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (_isCompleted != value) { _isCompleted = value; OnPropertyChanged(nameof(IsCompleted)); }
            }
        }

        private readonly int _userId;
        private ICollectionView _filteredTasksView;

        public MainWindowViewModel(int userId)
        {
            DatabaseHelper.InitializeDatabase();
            _userId = userId;
            LoadInitialData(userId);
            ClearSearchCommand = new RelayCommand(() => SearchText = "");
            _filteredTasksView = new CollectionViewSource { Source = UserTasks }.View;

            AddProjectCommand = new RelayCommand(AddProject, () => !string.IsNullOrWhiteSpace(NewProjectName));
            AddCategoryCommand = new RelayCommand(AddCategory, () => !string.IsNullOrWhiteSpace(NewCategoryName));
            AddTagCommand = new RelayCommand(AddTag, () => !string.IsNullOrWhiteSpace(NewTagName));
        }

        private TaskModel _selectedTask;
        public TaskModel SelectedTask
        {
            get => _selectedTask;
            set { _selectedTask = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); FilterTasks(); }
        }

        private void FilterTasks()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                _filteredTasksView.Filter = null;
                return;
            }

            var searchLower = SearchText.ToLower();
            _filteredTasksView.Filter = item =>
            {
                var task = item as TaskModel;
                // Убедитесь, что Title и Description не null перед вызовом ToLower()
                return (task.Title?.ToLower().Contains(searchLower) ?? false) ||
                       (task.Description?.ToLower().Contains(searchLower) ?? false);
            };
        }

        private void DeleteTask(object parameter)
        {
            if (!(parameter is TaskModel task)) return;

            using var connection = new SQLiteConnection("Data Source=users.db");
            connection.Open();

            var command = new SQLiteCommand("DELETE FROM Tasks WHERE Id = @id", connection);
            command.Parameters.AddWithValue("@id", task.Id);
            command.ExecuteNonQuery();

            UserTasks.Remove(task);
            CompletedTasks.Remove(task); // Убедитесь, что задача удаляется из обеих коллекций
        }

        private void EditTask(object parameter)
        {
            if (!(parameter is TaskModel task)) return;

            var editWindow = new NewTaskWindow(_userId, task, Projects, Categories);
            if (editWindow.ShowDialog() == true)
            {
                LoadInitialData(_userId);
            }
        }

        private void CompleteTask(object parameter)
        {
            if (!(parameter is TaskModel task)) return;

            // Обновляем статус IsCompleted в базе данных
            using var connection = new SQLiteConnection("Data Source=users.db");
            connection.Open();

            var command = new SQLiteCommand("UPDATE Tasks SET IsCompleted = @isCompleted WHERE Id = @id", connection);
            command.Parameters.AddWithValue("@isCompleted", task.IsCompleted);
            command.Parameters.AddWithValue("@id", task.Id);
            command.ExecuteNonQuery();

            // Обновляем ObservableCollections
            if (task.IsCompleted)
            {
                if (UserTasks.Contains(task)) UserTasks.Remove(task);
                if (!CompletedTasks.Contains(task)) CompletedTasks.Add(task);
            }
            else
            {
                if (CompletedTasks.Contains(task)) CompletedTasks.Remove(task);
                if (!UserTasks.Contains(task)) UserTasks.Add(task);
            }

            if (SelectedTask == task)
            {
                SelectedTask = null;
            }
        }

        private void OpenNewWindow()
        {
            var newTaskWindow = new NewTaskWindow(_userId, null, Projects, Categories, Tags);
            if (newTaskWindow.ShowDialog() == true)
            {
                LoadInitialData(_userId);
            }
        }

        private void AddProject()
        {
            using var connection = new SQLiteConnection("Data Source=users.db");
            connection.Open();

            var command = new SQLiteCommand("INSERT INTO Projects (Name, UserId) VALUES (@name, @userId)", connection);
            command.Parameters.AddWithValue("@name", NewProjectName);
            command.Parameters.AddWithValue("@userId", _userId);
            command.ExecuteNonQuery();

            NewProjectName = string.Empty;
            LoadInitialData(_userId); // Перезагружаем данные, чтобы обновить список проектов
        }

        private void AddCategory()
        {
            using var connection = new SQLiteConnection("Data Source=users.db");
            connection.Open();

            var command = new SQLiteCommand("INSERT INTO Categories (Name, UserId) VALUES (@name, @userId)", connection);
            command.Parameters.AddWithValue("@name", NewCategoryName);
            command.Parameters.AddWithValue("@userId", _userId);
            command.ExecuteNonQuery();

            NewCategoryName = string.Empty;
            LoadInitialData(_userId); // Перезагружаем данные, чтобы обновить список категорий
        }

        private void AddTag()
        {
            using var connection = new SQLiteConnection("Data Source=users.db");
            connection.Open();

            var command = new SQLiteCommand("INSERT INTO Tags (Name, UserId) VALUES (@name, @userId)", connection);
            command.Parameters.AddWithValue("@name", NewTagName);
            command.Parameters.AddWithValue("@userId", _userId);
            command.ExecuteNonQuery();

            NewTagName = string.Empty;
            LoadInitialData(_userId); // Перезагружаем данные, чтобы обновить список тегов
        }

        private void LoadInitialData(int userId)
        {
            UserTasks.Clear();
            CompletedTasks.Clear();
            Projects.Clear();
            Categories.Clear();
            Tags.Clear();

            using var connection = new SQLiteConnection("Data Source=users.db");
            connection.Open();

            // Добавляем фиктивный элемент "Без проекта" в начало списка
            Projects.Add(new Project { Id = 0, Name = "Без проекта", UserId = userId }); // Используем Id=0 для "None"
            var projectsCommand = new SQLiteCommand("SELECT Id, Name FROM Projects WHERE UserId = @userId", connection);
            projectsCommand.Parameters.AddWithValue("@userId", userId);
            using (var reader = projectsCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    Projects.Add(new Project { Id = reader.GetInt32(0), Name = reader.GetString(1), UserId = userId });
                }
            }

            // Добавляем фиктивный элемент "Без категории" в начало списка
            Categories.Add(new Category { Id = 0, Name = "Без категории", UserId = userId }); // Используем Id=0 для "None"
            var categoriesCommand = new SQLiteCommand("SELECT Id, Name FROM Categories WHERE UserId = @userId", connection);
            categoriesCommand.Parameters.AddWithValue("@userId", userId);
            using (var reader = categoriesCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    Categories.Add(new Category { Id = reader.GetInt32(0), Name = reader.GetString(1), UserId = userId });
                }
            }

            Tags.Add(new Tag { Id = 0, Name = "Без тегов", UserId = userId }); // Используем Id=0 для "None"
            var tagsCommand = new SQLiteCommand("SELECT Id, Name FROM Tags WHERE UserId = @userId", connection);
            tagsCommand.Parameters.AddWithValue("@userId", userId);
            using (var reader = tagsCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    Tags.Add(new Tag { Id = reader.GetInt32(0), Name = reader.GetString(1), UserId = userId });
                }
            }

            // ОБНОВЛЕННЫЙ ЗАПРОС ДЛЯ ЗАДАЧ С JOIN
            var tasksCommand = new SQLiteCommand(
                 @"SELECT
                    t.Id, t.UserId, t.Title, t.Description, t.IsCompleted, t.TaskImportance, t.ProjectId, t.CategoryId, t.TagId,
                    p.Name AS ProjectName,
                    c.Name AS CategoryName,
                    t_i.Name As TagName
                  FROM Tasks t
                  LEFT JOIN Projects p ON t.ProjectId = p.Id
                  LEFT JOIN Categories c ON t.CategoryId = c.Id
                  LEFT JOIN Tags t_i ON t.TagId = t_i.Id
                  WHERE t.UserId = @userId",
        connection);
            tasksCommand.Parameters.AddWithValue("@userId", userId);

            using (var reader = tasksCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    var task = new TaskModel
                    {
                        Id = reader.GetInt32(0),
                        UserId = reader.GetInt32(1), // <<<< ИСПРАВЛЕНО: Читаем UserId из ридера
                        Title = reader.GetString(2), // <<<< ИСПРАВЛЕНО: Смещено на +1
                        Description = reader.GetString(3), // <<<< ИСПРАВЛЕНО: Смещено на +1
                        IsCompleted = reader.GetBoolean(4), // <<<< ИСПРАВЛЕНО: Смещено на +1
                        TaskImportance = reader.IsDBNull(5) ? "Без приоритета" : reader.GetString(5), // <<<< ИСПРАВЛЕНО: Смещено на +1
                        ProjectId = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6), // <<<< ИСПРАВЛЕНО: Смещено на +1
                        CategoryId = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7), // <<<< ИСПРАВЛЕНО: Смещено на +1
                        ProjectName = reader.IsDBNull(9) ? "Без проекта" : reader.GetString(9), // <<<< ИСПРАВЛЕНО: Теперь 9 (ProjectName)
                        CategoryName = reader.IsDBNull(10) ? "Без категории" : reader.GetString(10), // <<<< ИСПРАВЛЕНО: Теперь 10 (CategoryName)
                        TagId = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8), // <<<< ИСПРАВЛЕНО: Теперь 8 (TagId)
                        TagName = reader.IsDBNull(11) ? "Без тега" : reader.GetString(11) // <<<< ИСПРАВЛЕНО: Теперь 11 (TagName)
                    };

                    if (task.IsCompleted)
                        CompletedTasks.Add(task);
                    else
                        UserTasks.Add(task);
                }
            }
        }
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}