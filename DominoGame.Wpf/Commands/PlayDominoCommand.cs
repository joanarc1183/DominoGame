using System;
using System.Windows.Input;
using DominoGame.Wpf.ViewModels;

namespace DominoGame.Wpf
{
    public class PlayDominoCommand : ICommand
    {
        private readonly GameViewModel _game;

        public PlayDominoCommand(GameViewModel game)
        {
            _game = game;
        }

        public bool CanExecute(object parameter)
            => parameter is DominoTileViewModel tile && tile.IsPlayable;

        public void Execute(object parameter)
        {
            if (parameter is DominoTileViewModel tile)
            {
                _game.PlayDomino(tile);
            }
        }

        public event EventHandler? CanExecuteChanged;
    }
}
