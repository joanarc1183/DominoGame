using System.Collections.ObjectModel;
using System.Windows.Input;
using DominoGame.Core;
using DominoGame.Wpf.Commands;
using DominoGame.Wpf.Models;

namespace DominoGame.Wpf.ViewModels;

public class GameViewModel
{
    private readonly GameController _game;

    public ObservableCollection<DominoTileViewModel> Board { get; } = new();
    public ObservableCollection<DominoTileViewModel> Hand { get; } = new();

    public ICommand PlayDominoCommand { get; }

    public List<DominoTileViewModel> PlayableTiles { get; private set; } = new();

    public GameViewModel()
    {
        var players = new List<Player>
        {
            new Player("Player 1"),
            new Player("Player 2")
        };

        var board = new Board();

        _game = new GameController(players, board);
        PlayDominoCommand = new PlayDominoCommand(this);

        LoadBoard();
        LoadHand();
    }

    private void LoadBoard()
    {
        Board.Clear();
        foreach (var d in _game.Board.GetPlacedDominoes())
            Board.Add(new DominoTileViewModel(d));
    }

    private void LoadHand()
    {
        Hand.Clear();
        foreach (var d in _game.CurrentPlayer.Hand)
            Hand.Add(new DominoTileViewModel(d));
    }

    public void PlayDomino(DominoTileViewModel tile)
    {
        // Always try to play on the right (can add left/right choice later)
        if (_game.PlayDomino(_game.CurrentPlayer, tile.Domino, BoardSide.Right))
        {
            LoadBoard();
            LoadHand();
            RefreshPlayableTiles();
        }
    }

    private void RefreshPlayableTiles()
    {
        PlayableTiles.Clear();
        foreach (var d in _game.GetPlayableDominoes(_game.CurrentPlayer))
            PlayableTiles.Add(new DominoTileViewModel(d));
    }

}
