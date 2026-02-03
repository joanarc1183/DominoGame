using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using DominoGame.Core;
using DominoGame.Wpf.Commands;
using DominoGame.Wpf.ViewModels;

namespace DominoGame.Wpf;

public class GameViewModel : INotifyPropertyChanged
{
    private readonly GameController _game;
    private DominoTileViewModel? _selectedDomino;
    private string _statusMessage = "Select a domino, then choose Place Left or Place Right.";

    public ObservableCollection<DominoTileViewModel> PlayerHand { get; } = new();
    public ObservableCollection<DominoTileViewModel> BoardDominoes { get; } = new();

    public DominoTileViewModel? SelectedDomino
    {
        get => _selectedDomino;
        set
        {
            if (_selectedDomino == value) return;
            if (_selectedDomino is not null)
                _selectedDomino.IsSelected = false;

            _selectedDomino = value;

            if (_selectedDomino is not null)
                _selectedDomino.IsSelected = true;

            UpdatePlayability();
            OnPropertyChanged(nameof(SelectedDomino));
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage == value) return;
            _statusMessage = value;
            OnPropertyChanged(nameof(StatusMessage));
        }
    }

    public int BoneyardCount => _game.Boneyard.Dominoes.Count;

    public bool CanPlaceLeft => SelectedDomino is not null &&
        _game.Board.CanPlace(SelectedDomino.Domino, BoardSide.Left);

    public bool CanPlaceRight => SelectedDomino is not null &&
        _game.Board.CanPlace(SelectedDomino.Domino, BoardSide.Right);

    public bool CanDraw => !_game.Boneyard.IsEmpty;

    public RelayCommand<DominoTileViewModel> SelectDominoCommand { get; }
    public RelayCommand<object> PlaceLeftCommand { get; }
    public RelayCommand<object> PlaceRightCommand { get; }
    public RelayCommand<object> DrawCommand { get; }

    public GameViewModel()
    {
        _game = new GameController();

        foreach (var domino in _game.Player.Hand)
            PlayerHand.Add(new DominoTileViewModel(domino));

        SelectDominoCommand = new RelayCommand<DominoTileViewModel>(SelectDomino);
        PlaceLeftCommand = new RelayCommand<object>(_ => PlaceSelected(BoardSide.Left), _ => CanPlaceLeft);
        PlaceRightCommand = new RelayCommand<object>(_ => PlaceSelected(BoardSide.Right), _ => CanPlaceRight);
        DrawCommand = new RelayCommand<object>(_ => DrawDomino(), _ => CanDraw);

        RefreshBoard();
        UpdatePlayability();
    }

    private void SelectDomino(DominoTileViewModel tile)
    {
        SelectedDomino = tile;
        StatusMessage = $"Selected {tile}.";
    }

    private void PlaceSelected(BoardSide side)
    {
        if (SelectedDomino is null)
            return;

        if (!_game.Board.CanPlace(SelectedDomino.Domino, side))
        {
            StatusMessage = "That domino cannot be placed on that side.";
            return;
        }

        _game.Play(SelectedDomino.Domino, side);
        PlayerHand.Remove(SelectedDomino);
        SelectedDomino = null;
        RefreshBoard();

        StatusMessage = "Domino placed.";
        OnPropertyChanged(nameof(BoneyardCount));
        OnPropertyChanged(nameof(CanDraw));
    }

    private void DrawDomino()
    {
        if (_game.Boneyard.IsEmpty)
            return;

        var domino = _game.Boneyard.Draw();
        var vm = new DominoTileViewModel(domino);
        PlayerHand.Add(vm);

        StatusMessage = $"Drew {vm}.";
        OnPropertyChanged(nameof(BoneyardCount));
        OnPropertyChanged(nameof(CanDraw));
        UpdatePlayability();
    }

    private void RefreshBoard()
    {
        BoardDominoes.Clear();
        foreach (var domino in _game.Board.Dominoes)
            BoardDominoes.Add(new DominoTileViewModel(domino));
    }

    private void UpdatePlayability()
    {
        foreach (var tile in PlayerHand)
            tile.IsPlayable = _game.Board.CanPlace(tile.Domino, BoardSide.Left) ||
                              _game.Board.CanPlace(tile.Domino, BoardSide.Right);

        OnPropertyChanged(nameof(CanPlaceLeft));
        OnPropertyChanged(nameof(CanPlaceRight));
        CommandManager.InvalidateRequerySuggested();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
