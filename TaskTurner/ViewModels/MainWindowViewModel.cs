using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TaskTurner.Models;
using TaskTurner.Views;
using TaskTurner.DataService;
namespace TaskTurner.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<TaskModel> UserTasks { get; set; } = new ObservableCollection<TaskModel>();
        public ObservableCollection<TaskModel> CompletedTasks { get; set; } = new ObservableCollection<TaskModel>();
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly int _userId;

        public MainWindowViewModel(int userId)
        {
            DatabaseHelper.InitializeDatabase();
            _userId = userId;
            UserTasks = new ObservableCollection<TaskModel>();
            LoadTasks(userId);
        }
        public ICommand IOpenNewWindow => new RelayCommand(OpenNewWindow);
        public ICommand IDeleteTask => new RelayCommand<object>(DeleteTask);
        public ICommand IEditTask => new RelayCommand<object>(EditTask);
        public ICommand ICompleteTask => new RelayCommand<object>(CompleteTask);

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

            // Передаем задачу в окно редактирования
            var editWindow = new NewTaskWindow(_userId, task);
            if (editWindow.ShowDialog() == true)
            {
                LoadTasks(_userId); // Обновляем список задач после редактирования
            }
        }

        private void CompleteTask(object parameter)
        {
            if (!(parameter is TaskModel task)) return;

            task.IsCompleted = !task.IsCompleted; // Инвертируем статус

            using var connection = new SQLiteConnection("Data Source=users.db");
            connection.Open();

            var command = new SQLiteCommand(
                "UPDATE Tasks SET IsCompleted = @isCompleted WHERE Id = @id",
                connection);
            command.Parameters.AddWithValue("@isCompleted", task.IsCompleted);
            command.Parameters.AddWithValue("@id", task.Id);
            command.ExecuteNonQuery();

            // Перемещаем задачу между списками
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
            SelectedTask = null;
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
    }
}