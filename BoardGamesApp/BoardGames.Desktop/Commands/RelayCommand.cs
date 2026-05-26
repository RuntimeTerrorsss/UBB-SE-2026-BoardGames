// <copyright file="RelayCommand.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BoardGames.Desktop.Commands
{
    public class RelayCommand(Action<object?> executeAction, Func<bool>? canExecuteFunction = null) : ICommand
    {
        private readonly Action<object?> executeAction = executeAction;
        private readonly Func<bool>? canExecuteFunction = canExecuteFunction;

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return canExecuteFunction?.Invoke() ?? true;
        }

        public void Execute(object? parameter)
        {
            executeAction(parameter);
        }

        public void NotifyCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
