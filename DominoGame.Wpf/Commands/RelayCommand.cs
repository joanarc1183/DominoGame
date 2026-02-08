using System.Windows.Input;
using DominoGame.Core;
namespace DominoGame.Wpf.Commands;

public class RelayCommand<T> : ICommand
{
    // Aksi utama command.
    private readonly Action<T> _execute;
    // Predicate untuk menentukan apakah command bisa dieksekusi.
    private readonly Func<T, bool>? _canExecute;

    /// <summary>
    /// Membuat relay command dengan aksi execute dan optional canExecute.
    /// </summary>
    public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    /// <summary>
    /// Mengecek apakah command boleh dijalankan.
    /// </summary>
    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke((T)parameter!) ?? true;
    }

    /// <summary>
    /// Menjalankan command dengan parameter yang diberikan.
    /// </summary>
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

    /// <summary>
    /// Event yang memberi tahu WPF agar mengecek ulang CanExecute.
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
