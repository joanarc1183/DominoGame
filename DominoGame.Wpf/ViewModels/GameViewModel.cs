using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using DominoGame.Core;
using DominoGame.Wpf.Commands;
using DominoGame.Wpf.ViewModels;

namespace DominoGame.Wpf;

public class GameViewModel : INotifyPropertyChanged
{
    private readonly GameController _game;
    private readonly Dictionary<Player, PlayerScoreViewModel> _playerLookup = new();
    private readonly Dictionary<Player, int> _scoreSnapshot = new();
    private DominoTileViewModel? _selectedDomino;
    private string _statusMessage = "Geser kartu ke kiri atau kanan untuk bermain.";
    private bool _autoAdvanceInProgress;
    private bool _pendingPassMessage;
    private string? _lastPassPlayerName;

    public ObservableCollection<DominoTileViewModel> BoardDominoes { get; } = new();
    public ObservableCollection<PlayerScoreViewModel> Players { get; } = new();

    public PlayerScoreViewModel? BottomPlayer { get; private set; }
    public PlayerScoreViewModel? LeftPlayer { get; private set; }
    public PlayerScoreViewModel? TopPlayer { get; private set; }
    public PlayerScoreViewModel? RightPlayer { get; private set; }

    public event Action<Player>? GameEnded;
    public event Action<string>? RoundEnded;

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
        {
            var vm = new PlayerScoreViewModel(player);
            Players.Add(vm);
            _playerLookup[player] = vm;
        }

        AssignSeats();
        InitializeScoreSnapshot();

        _game.OnTurnChanged += HandleTurnChanged;
        _game.OnDominoPlaced += HandleDominoPlaced;
        _game.OnPlayerPassed += HandlePlayerPassed;
        _game.OnRoundEnded += HandleRoundEnded;
        _game.OnGameEnded += HandleGameEnded;

        _game.StartRound();
        LoadAllHands();

        SelectDominoCommand = new RelayCommand<DominoTileViewModel>(SelectDomino);
        PlaceLeftCommand = new RelayCommand<object>(_ => PlaceSelected(BoardSide.Left), _ => CanPlaceLeft);
        PlaceRightCommand = new RelayCommand<object>(_ => PlaceSelected(BoardSide.Right), _ => CanPlaceRight);
        PassCommand = new RelayCommand<object>(_ => PassTurn(), _ => CanPass);

        RefreshBoard();
        UpdatePlayability();
    }

    public bool CanPlaceDomino(DominoTileViewModel tile, BoardSide side)
    {
        return tile is not null &&
               !_game.IsRoundEnded &&
               !_game.IsGameEnded &&
               _game.Board.CanPlace(tile.Domino, side);
    }

    public bool TryPlaceDominoFromDrag(DominoTileViewModel tile, BoardSide side)
    {
        var currentVm = CurrentPlayerViewModel;
        if (currentVm is null || !currentVm.Hand.Contains(tile))
            return false;

        if (!CanPlaceDomino(tile, side))
        {
            StatusMessage = "Kartu tidak bisa ditempatkan di sisi itu.";
            return false;
        }

        if (!_game.PlayDomino(_game.CurrentPlayer, tile.Domino, side))
        {
            StatusMessage = "Tidak bisa menaruh kartu.";
            return false;
        }

        currentVm.Hand.Remove(tile);
        SelectedDomino = null;
        RefreshBoard();

        _game.NextTurn();
        UpdatePlayability();
        return true;
    }

    private PlayerScoreViewModel? CurrentPlayerViewModel
        => _playerLookup.TryGetValue(_game.CurrentPlayer, out var vm) ? vm : null;

    private void AssignSeats()
    {
        BottomPlayer = Players.Count > 0 ? Players[0] : null;
        LeftPlayer = null;
        TopPlayer = null;
        RightPlayer = null;

        if (Players.Count == 2)
        {
            TopPlayer = Players[1];
        }
        else if (Players.Count == 3)
        {
            LeftPlayer = Players[1];
            RightPlayer = Players[2];
        }
        else if (Players.Count >= 4)
        {
            LeftPlayer = Players[1];
            TopPlayer = Players[2];
            RightPlayer = Players[3];
        }

        OnPropertyChanged(nameof(BottomPlayer));
        OnPropertyChanged(nameof(LeftPlayer));
        OnPropertyChanged(nameof(TopPlayer));
        OnPropertyChanged(nameof(RightPlayer));
    }

    private void InitializeScoreSnapshot()
    {
        _scoreSnapshot.Clear();
        foreach (var player in _game.Players)
            _scoreSnapshot[player] = player.Score;
    }

    private void SelectDomino(DominoTileViewModel tile)
    {
        SelectedDomino = tile;
        StatusMessage = $"Pilih {tile}.";
    }

    private void PlaceSelected(BoardSide side)
    {
        if (SelectedDomino is null)
            return;

        if (!_game.Board.CanPlace(SelectedDomino.Domino, side))
        {
            StatusMessage = "Kartu tidak bisa ditempatkan di sisi itu.";
            return;
        }

        if (!_game.PlayDomino(_game.CurrentPlayer, SelectedDomino.Domino, side))
        {
            StatusMessage = "Tidak bisa menaruh kartu.";
            return;
        }

        CurrentPlayerViewModel?.Hand.Remove(SelectedDomino);
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

    private void LoadAllHands()
    {
        foreach (var player in _game.Players)
        {
            if (_playerLookup.TryGetValue(player, out var vm))
                vm.RefreshHand(_game.GetHands(player));
        }

        SelectedDomino = null;
    }

    private void UpdatePlayability()
    {
        bool canInteract = !_game.IsRoundEnded && !_game.IsGameEnded;

        foreach (var vm in Players)
            vm.IsCurrent = vm.Player == _game.CurrentPlayer;

        foreach (var vm in Players)
        {
            foreach (var tile in vm.Hand)
            {
                tile.IsPlayable = vm.IsCurrent &&
                                  canInteract &&
                                  (_game.Board.CanPlace(tile.Domino, BoardSide.Left) ||
                                   _game.Board.CanPlace(tile.Domino, BoardSide.Right));
            }
        }

        OnPropertyChanged(nameof(CanPlaceLeft));
        OnPropertyChanged(nameof(CanPlaceRight));
        OnPropertyChanged(nameof(CanPass));
        OnPropertyChanged(nameof(CurrentPlayerName));
        OnPropertyChanged(nameof(CurrentPlayerScore));
        RefreshScores();
        CommandManager.InvalidateRequerySuggested();
    }

    private void AutoAdvanceIfStuck()
    {
        if (_autoAdvanceInProgress)
            return;

        _autoAdvanceInProgress = true;
        try
        {
            while (!_game.IsRoundEnded && !_game.IsGameEnded && !_game.CanPlay(_game.CurrentPlayer))
            {
                _game.NextTurn();
            }
        }
        finally
        {
            _autoAdvanceInProgress = false;
        }
    }

    private void HandleTurnChanged(Player player)
    {
        LoadAllHands();
        RefreshBoard();
        UpdatePlayability();

        if (_pendingPassMessage && _lastPassPlayerName is not null)
            StatusMessage = $"{_lastPassPlayerName} pass. Giliran {player.Name}.";
        else
            StatusMessage = $"Giliran {player.Name}.";

        _pendingPassMessage = false;
        _lastPassPlayerName = null;

        AutoAdvanceIfStuck();
    }

    private void HandleDominoPlaced(Player player, Domino domino, BoardSide side)
    {
        StatusMessage = $"{player.Name} menaruh {domino} di {side}.";
        RefreshBoard();
    }

    private void HandlePlayerPassed(Player player)
    {
        _pendingPassMessage = true;
        _lastPassPlayerName = player.Name;
        StatusMessage = $"{player.Name} pass.";
        UpdatePlayability();
    }

    private void HandleRoundEnded(
        Player? winner,
        bool isBlocked,
        IReadOnlyDictionary<Player, IReadOnlyList<Domino>> hands)
    {
        string message;

        if (winner is null)
        {
            message = isBlocked
                ? "Ronde berakhir seri karena buntu."
                : "Ronde berakhir seri.";
        }
        else
        {
            int pointsBefore = _scoreSnapshot.TryGetValue(winner, out var prev) ? prev : winner.Score;
            int gained = winner.Score - pointsBefore;
            message = $"{winner.Name} menang ronde dan mendapat {gained} poin.";
        }

        StatusMessage = message;
        RoundEnded?.Invoke(message);

        RefreshScores();
        UpdateScoreSnapshot();

        if (!_game.IsGameEnded)
            _game.StartRound();

        UpdatePlayability();
    }

    private void HandleGameEnded(Player winner)
    {
        StatusMessage = $"Game selesai! {winner.Name} menang.";
        UpdatePlayability();
        GameEnded?.Invoke(winner);
    }

    private void RefreshScores()
    {
        foreach (var player in Players)
            player.Refresh();
    }

    private void UpdateScoreSnapshot()
    {
        foreach (var player in _game.Players)
            _scoreSnapshot[player] = player.Score;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
