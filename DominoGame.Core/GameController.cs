namespace DominoGame.Core;
using System;
using System.Collections.Generic;
using System.Linq;

public class GameController
{
    // ================= EVENTS =================
    // Dipicu saat giliran pemain berubah.
    public event Action<Player>? OnTurnChanged;
    // Dipicu saat domino berhasil ditempatkan.
    public event Action<Player, Domino, BoardSide>? OnDominoPlaced;
    // Dipicu saat pemain memilih pass.
    public event Action<Player>? OnPlayerPassed;
    // Dipicu saat ronde berakhir: winner (null jika seri), isBlocked, snapshot tangan semua pemain.
    public event Action<
        Player?,                 // winner (null kalau tie)
        bool,                    // isBlocked
        IReadOnlyDictionary<Player, IReadOnlyList<Domino>>
    >? OnRoundEnded;
    // Dipicu saat game berakhir dan pemenang ditentukan.
    public event Action<Player>? OnGameEnded;

    // ================= FIELDS =================
    // Daftar pemain dalam game.
    private readonly List<Player> _players;
    // Tangan domino setiap pemain.
    private readonly Dictionary<Player, List<Domino>> _dominoInHands;
    // Board tempat domino ditaruh.
    private readonly IBoard _board;
    // Boneyard sebagai sumber pengambilan domino.
    private Boneyard _boneyard;

    // Indeks pemain yang sedang mendapat giliran.
    private int _currentPlayerIndex;
    // private int _roundLeaderIndex = -1;
    // Jumlah pass berturut-turut (untuk mendeteksi buntu).
    private int _consecutivePasses;
    // Status apakah ronde sudah selesai.
    private bool _roundEnded;
    // Status apakah game sudah selesai.
    private bool _isGameEnded;
    // Pemenang game (jika sudah ada).
    private Player? _gameWinner;
    // Skor target untuk menang.
    private readonly int _maxScoreToWin;

    // ================= PROPERTIES =================
    // Daftar pemain (read-only).
    public IReadOnlyList<Player> Players => _players;
    // Pemain yang sedang mendapat giliran.
    public Player CurrentPlayer => _players[_currentPlayerIndex];
    // Board yang dipakai game.
    public IBoard Board => _board;
    // Status ronde selesai.
    public bool IsRoundEnded => _roundEnded;
    // Status game selesai.
    public bool IsGameEnded => _isGameEnded;
    // Pemenang game (null jika belum ada).
    public Player? GameWinner => _gameWinner;
    // public IEnumerable<Player> Players => _players;

    // ================= CONSTRUCTOR =================
    // Membuat controller game dengan daftar pemain, board, dan target skor.
    public GameController(List<Player> players, IBoard board, int maxScoreToWin)
    {
        _players = players;
        _board = board;
        _maxScoreToWin = maxScoreToWin;
        _dominoInHands = players.ToDictionary(p => p, _ => new List<Domino>());
        _boneyard = new Boneyard(GenerateFullSet());
    }

    // ================= GAME FLOW =================

    // Memulai ronde baru: reset, bagi kartu, dan set giliran pertama.
    public void StartRound()
    {
        ResetRound();
        // DecideFirstPlayer();
        DealInitialHands();
        OnTurnChanged?.Invoke(CurrentPlayer);
    }

    // Pindah ke giliran berikutnya, termasuk handle pass dan cek akhir ronde.
    public void NextTurn()
    {
        if (_roundEnded || _isGameEnded) return;

        var player = CurrentPlayer;

        if (!CanPlay(player))
        {
            PassTurn(player);
        }

        CheckRoundEnd();

        if (!_roundEnded)
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
            OnTurnChanged?.Invoke(CurrentPlayer);
        }
    }

    // ================= PLAYER ACTIONS =================
    // Mengambil daftar domino di tangan pemain (read-only).
    public IReadOnlyList<Domino> GetHands(Player player)
        => _dominoInHands[player].AsReadOnly();

    // Memainkan domino pada sisi tertentu.
    public bool PlayDomino(Player player, Domino domino, BoardSide side)
    {
        if (player != CurrentPlayer)
            return false;


        if (!_board.CanPlace(domino, side))
            return false;

        _board.Place(domino, side);
        _dominoInHands[player].Remove(domino);
        _consecutivePasses = 0;

        OnDominoPlaced?.Invoke(player, domino, side);
        
        return true;
    }

    // Melakukan pass untuk pemain saat ini.
    public bool PassTurn(Player player)
    {
        if (player != CurrentPlayer)
            return false;

        _consecutivePasses++;
        OnPlayerPassed?.Invoke(player);
        
        return true;
    }

    // Mengecek apakah pemain bisa bermain pada giliran ini.
    public bool CanPlay(Player player)
    {
        if (_board.IsEmpty)
            return _dominoInHands[player].Count > 0;

        return _dominoInHands[player]
            .Any(d =>
                _board.CanPlace(d, BoardSide.Left) ||
                _board.CanPlace(d, BoardSide.Right));
    }

    // Menjumlahkan total pip dari semua domino di tangan pemain.
    public int CountPips(Player player)
    {
        return _dominoInHands[player]
            .Sum(d => (int)d.LeftPip + (int)d.RightPip);
    }

    // ================= ROUND END =================
    // Mengecek apakah ronde berakhir karena ada yang habis kartu atau buntu.
    private void CheckRoundEnd()
    {
        // Normal win
        var emptyPlayer = _players.FirstOrDefault(p => _dominoInHands[p].Count == 0);
        
        if (emptyPlayer != null)
        {
            _roundEnded = true;
            HandleNormalWin(emptyPlayer);
            return;
        }

        // Blocked game
        if (_consecutivePasses >= _players.Count)
        {
            _roundEnded = true;
            HandleBlockedGame();
        }
    }

    // Menang normal: pemain habis kartu, dapat skor dari sisa pip lawan.
    private void HandleNormalWin(Player winner)
    {
        int score = _players
            .Where(p => p != winner)
            .SelectMany(p => _dominoInHands[p])
            .Sum(d => (int)d.LeftPip + (int)d.RightPip);

        winner.Score += score;
        OnRoundEnded?.Invoke(winner, false, SnapshotHands());
        CheckGameEnd();
    }

    // Menang karena buntu: pemain dengan total pip terendah menang, atau seri jika imbang.
    private void HandleBlockedGame()
    {
        var pipTotals = _players.ToDictionary(p => p, CountPips);

        int min = pipTotals.Min(x => x.Value);
        var lowestPlayers = pipTotals.Where(x => x.Value == min).ToList();

        // Tie → no winner
        if (lowestPlayers.Count > 1)
        {
            OnRoundEnded?.Invoke(null, true, SnapshotHands());
            return;
        }

        var winner = lowestPlayers.First().Key;

        int gain = pipTotals.Sum(x => x.Value) - pipTotals[winner];
        winner.Score += gain;

        OnRoundEnded?.Invoke(winner, true, SnapshotHands());
        CheckGameEnd();
    }

    // ================= GAME END =================
    // Mengecek apakah ada pemain yang mencapai target skor.
    private void CheckGameEnd()
    {
        _gameWinner = _players.FirstOrDefault(p => p.Score >= _maxScoreToWin);
        if (_gameWinner != null)
        {
            _isGameEnded = true;
            OnGameEnded?.Invoke(_gameWinner);
        }
    }

    // ================= INTERNAL HELPERS =================
    // Reset state ronde: board kosong, boneyard baru, pass reset, tangan dikosongkan.
    private void ResetRound()
    {
        _board.Reset();
        _boneyard = new Boneyard(GenerateFullSet());
        _consecutivePasses = 0;
        _roundEnded = false;

        foreach (var p in _players)
            _dominoInHands[p].Clear();

    }

    // Membagikan 7 domino ke setiap pemain.
    private void DealInitialHands()
    {
        for (int i = 0; i < 7; i++)
            foreach (var p in _players)
                _dominoInHands[p].Add(_boneyard.Draw());
    }
    // Membuat set domino lengkap 0-0 sampai 6-6.
    private IEnumerable<Domino> GenerateFullSet()
    {
        for (int i = 0; i <= 6; i++)
            for (int j = i; j <= 6; j++)
                yield return new Domino((Dot)i, (Dot)j);
    }

    // Membuat snapshot tangan semua pemain agar aman dipakai di event.
    private IReadOnlyDictionary<Player, IReadOnlyList<Domino>> SnapshotHands()
    {
        return _dominoInHands.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<Domino>)kvp.Value.AsReadOnly()
        );
    }

}
