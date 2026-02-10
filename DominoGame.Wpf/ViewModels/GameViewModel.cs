using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using DominoGame.Core;
using DominoGame.Wpf.Commands;
using DominoGame.Wpf.Services;
using DominoGame.Wpf.ViewModels;

namespace DominoGame.Wpf;

public class GameViewModel : INotifyPropertyChanged
{
    // Mesin game utama yang memegang aturan dan state ronde/game.
    private readonly GameController _game;
    // Lookup cepat dari model Player ke view model skor.
    private readonly Dictionary<IPlayer, PlayerScoreViewModel> _playerLookup = new();
    // Snapshot skor sebelumnya untuk menghitung poin yang didapat per ronde.
    private readonly Dictionary<IPlayer, int> _scoreSnapshot = new();
    // Domino yang sedang dipilih oleh pemain saat ini.
    private DominoTileViewModel? _selectedDomino;
    // Pesan status yang tampil di UI.
    private string _statusMessage = "Geser kartu ke kiri atau kanan untuk bermain.";
    // Flag untuk mencegah auto-advance re-entrancy.
    private bool _autoAdvanceInProgress;
    // Flag agar pesan pass ditampilkan saat giliran berikutnya.
    private bool _pendingPassMessage;
    // Nama pemain yang terakhir pass untuk kebutuhan pesan.
    private string? _lastPassPlayerName;

    /// Domino yang sedang ada di board (untuk ditampilkan di UI).
    public ObservableCollection<DominoTileViewModel> BoardDominoes { get; } = new();
    /// Daftar pemain dalam bentuk view model skor.
    public ObservableCollection<PlayerScoreViewModel> Players { get; } = new();

    /// Pemain yang duduk di posisi bawah layar (pemain utama).
    public PlayerScoreViewModel? BottomPlayer { get; private set; }
    /// Pemain yang duduk di posisi kiri layar.
    public PlayerScoreViewModel? LeftPlayer { get; private set; }
    /// Pemain yang duduk di posisi atas layar.
    public PlayerScoreViewModel? TopPlayer { get; private set; }
    /// Pemain yang duduk di posisi kanan layar.
    public PlayerScoreViewModel? RightPlayer { get; private set; }

    /// Event ketika game selesai (pemenang sudah ditentukan).
    public event Action<IPlayer>? GameEnded;
    /// Event ketika ronde selesai (untuk menampilkan pesan).
    public event Action<string>? RoundEnded;

    /// Domino yang dipilih pemain untuk dimainkan.
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

    /// Pesan status yang muncul di banner UI.
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

    /// Menandakan apakah domino terpilih bisa ditempatkan di sisi kiri.
    public bool CanPlaceLeft => SelectedDomino is not null &&
        !_game.IsRoundEnded &&
        !_game.IsGameEnded &&
        _game.CanPlace(SelectedDomino.Domino, BoardSide.Left);

    /// Menandakan apakah domino terpilih bisa ditempatkan di sisi kanan.
    public bool CanPlaceRight => SelectedDomino is not null &&
        !_game.IsRoundEnded &&
        !_game.IsGameEnded &&
        _game.CanPlace(SelectedDomino.Domino, BoardSide.Right);

    /// Menandakan apakah pemain saat ini boleh pass.
    public bool CanPass => !_game.IsRoundEnded && !_game.IsGameEnded && !_game.CanPlay(_game.CurrentPlayer);

    /// Nama pemain yang sedang mendapat giliran.
    public string CurrentPlayerName => _game.CurrentPlayer.Name;
    /// Skor pemain yang sedang mendapat giliran.
    public int CurrentPlayerScore => _game.CurrentPlayer.Score;

    /// Command untuk memilih domino dari tangan.
    public RelayCommand<DominoTileViewModel> SelectDominoCommand { get; }
    /// Command untuk menaruh domino di sisi kiri.
    public RelayCommand<object> PlaceLeftCommand { get; }
    /// Command untuk menaruh domino di sisi kanan.
    public RelayCommand<object> PlaceRightCommand { get; }
    /// Command untuk pass giliran.
    public RelayCommand<object> PassCommand { get; }

    /// Menyiapkan game baru, binding data, dan event listener.
    public GameViewModel(IEnumerable<IPlayer> players, int maxScoreToWin)
    {
        var playerList = players.ToList();
        _game = new GameController(playerList, new Board(), maxScoreToWin);

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

    /// Mengecek apakah tile tertentu bisa ditempatkan pada sisi tertentu.
    public bool CanPlaceDomino(DominoTileViewModel tile, BoardSide side)
    {
        return tile is not null &&
               !_game.IsRoundEnded &&
               !_game.IsGameEnded &&
               _game.CanPlace(tile.Domino, side);
    }

    /// Mencoba menaruh domino melalui aksi drag-drop dari UI.
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

    /// ViewModel pemain yang sedang mendapat giliran (jika ada).
    private PlayerScoreViewModel? CurrentPlayerViewModel
        => _playerLookup.TryGetValue(_game.CurrentPlayer, out var vm) ? vm : null;

    /// Menentukan posisi kursi pemain (atas/bawah/kiri/kanan) berdasarkan jumlah pemain.
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

    /// Mengambil snapshot skor awal ronde untuk menghitung skor yang didapat.
    private void InitializeScoreSnapshot()
    {
        _scoreSnapshot.Clear();
        foreach (var player in _game.Players)
            _scoreSnapshot[player] = player.Score;
    }

    /// Menangani aksi memilih domino dari tangan.
    private void SelectDomino(DominoTileViewModel tile)
    {
        SelectedDomino = tile;
        StatusMessage = $"Pilih {tile}.";
    }

    /// Menaruh domino terpilih ke sisi yang dipilih.
    private void PlaceSelected(BoardSide side)
    {
        if (SelectedDomino is null)
            return;

        if (!_game.CanPlace(SelectedDomino.Domino, side))
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

    /// Melewati giliran jika pemain tidak bisa bermain.
    private void PassTurn()
    {
        if (!CanPass)
            return;

        _game.NextTurn();
        UpdatePlayability();
    }

    /// Menyegarkan daftar domino yang tampil di board.
    private void RefreshBoard()
    {
        BoardDominoes.Clear();
        foreach (var domino in _game.Board.Dominoes)
            BoardDominoes.Add(new DominoTileViewModel(domino));
    }

    /// Memuat ulang tangan semua pemain dari model game.
    private void LoadAllHands()
    {
        foreach (var player in _game.Players)
        {
            if (_playerLookup.TryGetValue(player, out var vm))
                vm.RefreshHand(_game.GetHands(player));
        }

        SelectedDomino = null;
    }

    /// Menghitung ulang status playable, skor, dan command state untuk UI.
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
                                  (_game.CanPlace(tile.Domino, BoardSide.Left) ||
                                   _game.CanPlace(tile.Domino, BoardSide.Right));
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

    /// Otomatis melewati pemain yang tidak bisa bermain agar game tidak macet.
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

    /// Handler saat giliran pemain berubah.
    private void HandleTurnChanged(IPlayer player)
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

    /// Handler saat pemain menaruh domino.
    private void HandleDominoPlaced(IPlayer player, IDomino domino, BoardSide side)
    {
        StatusMessage = $"{player.Name} menaruh {domino} di {side}.";
        RefreshBoard();
        UiSoundService.PlayDominoPlaced();
    }

    /// Handler saat pemain pass.
    private void HandlePlayerPassed(IPlayer player)
    {
        _pendingPassMessage = true;
        _lastPassPlayerName = player.Name;
        StatusMessage = $"{player.Name} pass.";
        UpdatePlayability();
    }

    /// Handler saat ronde berakhir (menang, seri, atau buntu).
    private void HandleRoundEnded(
        IPlayer? winner,
        bool isBlocked,
        IReadOnlyDictionary<IPlayer, IReadOnlyList<IDomino>> hands)
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
            message = $"{winner.Name} won the round and earned {gained} points.";
        }

        StatusMessage = message;
        RoundEnded?.Invoke(message);

        RefreshScores();
        UpdateScoreSnapshot();

        if (!_game.IsGameEnded)
            _game.StartRound();

        UpdatePlayability();
    }

    /// Handler saat game berakhir (pemenang final).
    private void HandleGameEnded(IPlayer winner)
    {
        StatusMessage = $"Game selesai! {winner.Name} menang.";
        UpdatePlayability();
        GameEnded?.Invoke(winner);
    }

    /// Menyegarkan tampilan skor semua pemain.
    private void RefreshScores()
    {
        foreach (var player in Players)
            player.Refresh();
    }

    /// Memperbarui snapshot skor setelah ronde selesai.
    private void UpdateScoreSnapshot()
    {
        foreach (var player in _game.Players)
            _scoreSnapshot[player] = player.Score;
    }

    /// Event notifikasi perubahan properti untuk binding WPF.
    public event PropertyChangedEventHandler? PropertyChanged;

    /// Memicu event PropertyChanged.
    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
