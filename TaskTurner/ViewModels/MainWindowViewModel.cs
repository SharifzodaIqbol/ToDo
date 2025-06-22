using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using TaskTurner.DataService;
using TaskTurner.Models;
using TaskTurner.Views;
namespace TaskTurner.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<TaskModel> UserTasks { get; set; } = new ObservableCollection<TaskModel>();
        public ObservableCollection<TaskModel> CompletedTasks { get; set; } = new ObservableCollection<TaskModel>();
        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (_isCompleted != value)
                {
                    _isCompleted = value;
                    OnPropertyChanged(nameof(IsCompleted));
                }
            }
        }
        private readonly int _userId;
        private ICollectionView _filteredTasksView;
        public MainWindowViewModel(int userId)
        {
            DatabaseHelper.InitializeDatabase();
            _userId = userId;
            LoadTasks(userId);
            _filteredTasksView = new CollectionViewSource { Source = UserTasks }.View;
        }
        public ICommand IOpenNewWindow => new RelayCommand(OpenNewWindow);
        public ICommand IDeleteTask => new RelayCommand<object>(DeleteTask);
        public ICommand IEditTask => new RelayCommand<object>(EditTask);
        public ICommand ICompleteTask => new RelayCommand<object>(CompleteTask);
        public ICollectionView FilteredTasksView => _filteredTasksView;
        private TaskModel _selectedTask;
        public TaskModel SelectedTask
        {
            get => _selectedTask;
            set
            {
                _selectedTask = value;
                OnPropertyChanged();
            }
        }
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterTasks();
            }
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
                return task.Title.ToLower().Contains(searchLower) ||
                       task.Description.ToLower().Contains(searchLower);
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
        }

        private void EditTask(object parameter)
        {
            if (!(parameter is TaskModel task)) return;

            var editWindow = new NewTaskWindow(_userId, task);
            if (editWindow.ShowDialog() == true)
            {
                LoadTasks(_userId);
            }
        }

        private void CompleteTask(object parameter)
        {
            if (!(parameter is TaskModel task)) return;

            if ((task.IsCompleted && CompletedTasks.Contains(task)) ||
                (!task.IsCompleted && UserTasks.Contains(task)))
            {
                return;
            }

            if (task.IsCompleted)
            {
                UserTasks.Remove(task);
                CompletedTasks.Add(task);
            }
            else
            {
                CompletedTasks.Remove(task);
                UserTasks.Add(task);
            }

            using var connection = new SQLiteConnection("Data Source=users.db");
            connection.Open();

            var command = new SQLiteCommand(
                "UPDATE Tasks SET IsCompleted = @isCompleted WHERE Id = @id",
                connection);
            command.Parameters.AddWithValue("@isCompleted", task.IsCompleted);
            command.Parameters.AddWithValue("@id", task.Id);
            command.ExecuteNonQuery();

            if (SelectedTask == task)
            {
                SelectedTask = null;
            }
        }

        private void OpenNewWindow()
        {
            var newTaskWindow = new NewTaskWindow(_userId);
            if (newTaskWindow.ShowDialog() == true)
            {
                LoadTasks(_userId);
            }
        }

        private void LoadTasks(int userId)
        {
            UserTasks.Clear();
            CompletedTasks.Clear();

            using var connection = new SQLiteConnection("Data Source=users.db");
            connection.Open();

            var command = new SQLiteCommand(
                "SELECT Id, Title, Description, IsCompleted FROM Tasks WHERE UserId = @userId",
                connection);
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var task = new TaskModel
                {
                    Id = reader.GetInt32(0),
                    UserId = userId,
                    Title = reader.GetString(1),
                    Description = reader.GetString(2),
                    IsCompleted = reader.GetBoolean(3)
                };

                if (task.IsCompleted)
                    CompletedTasks.Add(task);
                else
                    UserTasks.Add(task);
            }
        }
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}