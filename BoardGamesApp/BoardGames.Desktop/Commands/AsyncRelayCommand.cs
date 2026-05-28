namespace BoardGames.Desktop.Commands
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null) : ICommand
    {
        private readonly Func<Task> execute = execute;
        private readonly Func<bool>? canExecute = canExecute;

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;

        public async void Execute(object? parameter) => await execute();

        public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}