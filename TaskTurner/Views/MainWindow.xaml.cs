using System.Data.SQLite;
using System.Windows;
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
        }
    }
}