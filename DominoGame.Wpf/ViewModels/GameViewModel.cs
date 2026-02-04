using System.Collections.ObjectModel;
using System.Collections.Generic;
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
    public ObservableCollection<PlayerScoreViewModel> Players { get; } = new();
    public event Action<Player>? GameEnded;

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

    public bool CanPlaceLeft => SelectedDomino is not null &&
        !_game.IsRoundEnded &&
        !_game.IsGameEnded &&
        _game.Board.CanPlace(SelectedDomino.Domino, BoardSide.Left);

    public bool CanPlaceRight => SelectedDomino is not null &&
        !_game.IsRoundEnded &&
        !_game.IsGameEnded &&
        _game.Board.CanPlace(SelectedDomino.Domino, BoardSide.Right);

    public bool CanPass => !_game.IsRoundEnded && !_game.IsGameEnded && !_game.CanPlay(_game.CurrentPlayer);

    public string CurrentPlayerName => _game.CurrentPlayer.Name;
    public int CurrentPlayerScore => _game.CurrentPlayer.Score;

    public RelayCommand<DominoTileViewModel> SelectDominoCommand { get; }
    public RelayCommand<object> PlaceLeftCommand { get; }
    public RelayCommand<object> PlaceRightCommand { get; }
    public RelayCommand<object> PassCommand { get; }

    public GameViewModel(List<Player> players, int maxScoreToWin)
    {
        _game = new GameController(players, new Board(), maxScoreToWin);

        foreach (var player in _game.Players)
            Players.Add(new PlayerScoreViewModel(player));

        _game.OnTurnChanged += HandleTurnChanged;
        _game.OnDominoPlaced += HandleDominoPlaced;
        _game.OnPlayerPassed += HandlePlayerPassed;
        _game.OnRoundEnded += HandleRoundEnded;
        _game.OnGameEnded += HandleGameEnded;

        _game.StartRound();
        LoadCurrentPlayerHand();

        SelectDominoCommand = new RelayCommand<DominoTileViewModel>(SelectDomino);
        PlaceLeftCommand = new RelayCommand<object>(_ => PlaceSelected(BoardSide.Left), _ => CanPlaceLeft);
        PlaceRightCommand = new RelayCommand<object>(_ => PlaceSelected(BoardSide.Right), _ => CanPlaceRight);
        PassCommand = new RelayCommand<object>(_ => PassTurn(), _ => CanPass);

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

        if (!_game.PlayDomino(_game.CurrentPlayer, SelectedDomino.Domino, side))
        {
            StatusMessage = "Could not place domino.";
            return;
        }

        PlayerHand.Remove(SelectedDomino);
        SelectedDomino = null;
        RefreshBoard();

        _game.NextTurn();
        UpdatePlayability();
    }

    private void PassTurn()
    {
        if (!CanPass)
            return;

        _game.NextTurn();
        UpdatePlayability();
    }

    private void RefreshBoard()
    {
        BoardDominoes.Clear();
        foreach (var domino in _game.Board.Dominoes)
            BoardDominoes.Add(new DominoTileViewModel(domino));
    }

    private void LoadCurrentPlayerHand()
    {
        PlayerHand.Clear();
        foreach (var domino in _game.GetHands(_game.CurrentPlayer))
            PlayerHand.Add(new DominoTileViewModel(domino));
    }

    private void UpdatePlayability()
    {
        bool canInteract = !_game.IsRoundEnded && !_game.IsGameEnded;

        foreach (var tile in PlayerHand)
            tile.IsPlayable = canInteract &&
                              (_game.Board.CanPlace(tile.Domino, BoardSide.Left) ||
                               _game.Board.CanPlace(tile.Domino, BoardSide.Right));

        OnPropertyChanged(nameof(CanPlaceLeft));
        OnPropertyChanged(nameof(CanPlaceRight));
        OnPropertyChanged(nameof(CanPass));
        OnPropertyChanged(nameof(CurrentPlayerName));
        OnPropertyChanged(nameof(CurrentPlayerScore));
        RefreshScores();
        CommandManager.InvalidateRequerySuggested();
    }

    private void HandleTurnChanged(Player player)
    {
        LoadCurrentPlayerHand();
        RefreshBoard();
        UpdatePlayability();
        StatusMessage = $"{player.Name}'s turn.";
    }

    private void HandleDominoPlaced(Player player, Domino domino, BoardSide side)
    {
        StatusMessage = $"{player.Name} placed {domino} on the {side}.";
        RefreshBoard();
    }

    private void HandlePlayerPassed(Player player)
    {
        StatusMessage = $"{player.Name} passed.";
        UpdatePlayability();
    }

    private void HandleRoundEnded(
        Player? winner,
        bool isBlocked,
        IReadOnlyDictionary<Player, IReadOnlyList<Domino>> hands)
    {
        StatusMessage = winner is null
            ? "Round ended in a tie."
            : $"{winner.Name} wins the round.";

        RefreshScores();

        if (!_game.IsGameEnded)
            _game.StartRound();

        UpdatePlayability();
    }

    private void HandleGameEnded(Player winner)
    {
        StatusMessage = $"{winner.Name} wins the game!";
        UpdatePlayability();
        GameEnded?.Invoke(winner);
    }

    private void RefreshScores()
    {
        foreach (var player in Players)
            player.Refresh();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
