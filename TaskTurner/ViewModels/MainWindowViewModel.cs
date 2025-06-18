using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TaskTurner.Views;

namespace TaskTurner.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private readonly int _userId;

        public MainWindowViewModel(int userId)
        {
            _userId = userId;
        }

        public ICommand IOpenNewWindow => new RelayCommand(OpenNewWindow);

        private void OpenNewWindow()
        {
            var newTaskWindow = new NewTaskWindow(_userId);
            newTaskWindow.Show();
        }
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
