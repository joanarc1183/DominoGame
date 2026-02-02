using System;
using System.Windows.Input;
using DominoGame.Wpf.Models;
using DominoGame.Wpf.ViewModels;

namespace DominoGame.Wpf.Commands;

public class PlayDominoCommand : ICommand
{
    private readonly GameViewModel _vm;

    public PlayDominoCommand(GameViewModel vm) => _vm = vm;

    public bool CanExecute(object? parameter) => parameter is DominoTileViewModel;
    public void Execute(object? parameter)
    {
        if (parameter is DominoTileViewModel tile)
            _vm.PlayDomino(tile);
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
