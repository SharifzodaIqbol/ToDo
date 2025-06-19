using System.Data.SQLite;
using System.Windows;
using TaskTurner.Models;
using TaskTurner.ViewModels;

namespace TaskTurner
{
    public partial class MainWindow : Window
    {
        public int UserId { get; }
        public MainWindow(int userId)
        {
            InitializeComponent();
            UserId = userId;
            DataContext = new MainWindowViewModel(UserId);
            UserTasksListView.SelectionChanged += (s, e) =>
            {
                var vm = DataContext as MainWindowViewModel;
                vm.SelectedTask = UserTasksListView.SelectedItem as TaskModel;
            };
        }
    }
}