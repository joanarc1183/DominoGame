using System.Windows.Input;
using DominoGame.Core;
namespace DominoGame.Wpf.Commands;

public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool>? _canExecute;

    public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke((T)parameter!) ?? true;
    }

    public void Execute(object? parameter)
    {
        if (parameter is T value)
        {
            _execute(value);
            return;
        }

        if (parameter is null)
        {
            _execute(default!);
            return;
        }

        throw new ArgumentException(
            $"Invalid command parameter. Expected {typeof(T).Name}, got {parameter.GetType().Name}.");
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
