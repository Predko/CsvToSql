using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace CSVtoDataBase
{
    public class OperationCommands:ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        private Func<object, bool> canExecute;
        private Action<object> execute;

        public OperationCommands(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.canExecute = canExecute;
            this.execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            execute?.Invoke(parameter);
        }
    }
}
