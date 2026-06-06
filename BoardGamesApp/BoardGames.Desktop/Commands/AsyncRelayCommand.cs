// <copyright file="AsyncRelayCommand.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BoardGames.Desktop.Commands
{
    public class AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null) : ICommand
    {
        private readonly Func<Task> execute = execute;
        private readonly Func<bool>? canExecute = canExecute;

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => this.canExecute?.Invoke() ?? true;

        public async void Execute(object? parameter) => await this.execute();

        public void NotifyCanExecuteChanged() => this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
