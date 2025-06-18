using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TaskTurner.Models;
using TaskTurner.Views;

namespace TaskTurner.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<TaskModel> UserTasks { get; set; } = new ObservableCollection<TaskModel>();
        private readonly int _userId;

        public MainWindowViewModel(int userId)
        {
            _userId = userId;
            UserTasks = new ObservableCollection<TaskModel>();
            LoadTasks(userId);
        }

        public ICommand IOpenNewWindow => new RelayCommand(OpenNewWindow);

        private void OpenNewWindow()
        {
            var newTaskWindow = new NewTaskWindow(_userId);
            bool? result = newTaskWindow.ShowDialog();

            if (result == true)
            {
                LoadTasks(_userId);
            }
        }
        private void LoadTasks(int userId)
        {
            using var connection = new SQLiteConnection("Data Source=users.db");
            connection.Open();

            var command = new SQLiteCommand("SELECT Id, Title, Description FROM Tasks WHERE UserId = @userId", connection);
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                UserTasks.Add(new TaskModel
                {
                    Id = reader.GetInt32(0),
                    UserId = userId,
                    Title = reader.GetString(1),
                    Description = reader.GetString(2)
                });
            }
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
