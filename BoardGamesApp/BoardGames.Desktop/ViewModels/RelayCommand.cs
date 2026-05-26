// <copyright file="RelayCommand.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BoardGames.Desktop.ViewModels
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> executeAction;
        private readonly Predicate<T> canExecutePredicate;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            executeAction = execute ?? throw new ArgumentNullException(nameof(execute));
            canExecutePredicate = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecutePredicate == null || canExecutePredicate((T)parameter);
        }

        public void Execute(object parameter)
        {
            executeAction((T)parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class RelayCommandNoParam : ICommand
    {
        private readonly Action? executeAct;
        private readonly Func<Task>? executeAsyncAct;
        private readonly Func<bool> canExecuteFunc;

        public RelayCommandNoParam(Action execute, Func<bool> canExecute = null)
        {
            executeAct = execute ?? throw new ArgumentNullException(nameof(execute));
            canExecuteFunc = canExecute;
        }

        public RelayCommandNoParam(Func<Task> executeAsync, Func<bool> canExecute = null)
        {
            this.executeAsyncAct = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            this.canExecuteFunc = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecuteFunc == null || canExecuteFunc();
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
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
