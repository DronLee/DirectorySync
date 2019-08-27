using System;
using System.Windows.Input;

namespace DirectorySync.ViewModels
{
    internal class Command : ICommand
    {
        private readonly Action<object> execute;

        public Command(Action<object> createdItem)
        {
            execute = createdItem;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            execute(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
    }

    internal class Command<T> : ICommand where T : class
    {
        private readonly Action<T> execute;

        public Command(Action<T> createdItem)
        {
            execute = createdItem;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            execute(parameter as T);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
    }
}