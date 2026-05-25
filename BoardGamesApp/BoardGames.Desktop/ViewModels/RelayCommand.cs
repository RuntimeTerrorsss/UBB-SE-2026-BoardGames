// <copyright file="RelayCommand.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BookingBoardGames.Src.ViewModels
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> executeAction;
        private readonly Predicate<T> canExecutePredicate;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            this.executeAction = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecutePredicate = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecutePredicate == null || this.canExecutePredicate((T)parameter);
        }

        public void Execute(object parameter)
        {
            this.executeAction((T)parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class RelayCommandNoParam : ICommand
    {
        private readonly Action? executeAct;
        private readonly Func<Task>? executeAsyncAct;
        private readonly Func<bool> canExecuteFunc;

        public RelayCommandNoParam(Action execute, Func<bool> canExecute = null)
        {
            this.executeAct = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecuteFunc = canExecute;
        }

        public RelayCommandNoParam(Func<Task> executeAsync, Func<bool> canExecute = null)
        {
            this.executeAsyncAct = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            this.canExecuteFunc = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecuteFunc == null || this.canExecuteFunc();
        }

        public void Execute(object parameter)
        {
            if (this.executeAsyncAct != null)
            {
                _ = this.RunAsync(this.executeAsyncAct);
                return;
            }

            this.executeAct!();
        }

        private async Task RunAsync(Func<Task> action)
        {
            try
            {
                await action().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RelayCommand async execution failed: {ex}");
            }
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
