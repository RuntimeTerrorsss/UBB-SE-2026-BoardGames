
using System;
using System.Windows.Input;

namespace BoardGames.Desktop.Commands
{
    public class RelayCommand(Action<object?> executeAction, Func<bool>? canExecuteFunction = null) : ICommand
    {
        private readonly Action<object?> executeAction = executeAction;
        private readonly Func<bool>? canExecuteFunction = canExecuteFunction;

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return this.canExecuteFunction?.Invoke() ?? true;
        }

        public void Execute(object? parameter)
        {
            this.executeAction(parameter);
        }

        public void NotifyCanExecuteChanged() =>
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
