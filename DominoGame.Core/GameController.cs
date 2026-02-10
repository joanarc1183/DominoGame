namespace DominoGame.Core;
using System;
using System.Collections.Generic;
using System.Linq;

public class GameController
{
    // ================= EVENTS =================
    // Dipicu saat giliran pemain berubah.
    public event Action<IPlayer>? OnTurnChanged;
    // Dipicu saat domino berhasil ditempatkan.
    public event Action<IPlayer, IDomino, BoardSide>? OnDominoPlaced;
    // Dipicu saat pemain memilih pass.
    public event Action<IPlayer>? OnPlayerPassed;
    // Dipicu saat ronde berakhir: winner (null jika seri), isBlocked, snapshot tangan semua pemain.
    public event Action<
        IPlayer?,                 // winner (null kalau tie)
        bool,                    // isBlocked
        IReadOnlyDictionary<IPlayer, IReadOnlyList<IDomino>>
    >? OnRoundEnded;
    // Dipicu saat game berakhir dan pemenang ditentukan.
    public event Action<IPlayer>? OnGameEnded;

    // ================= FIELDS =================
    // Daftar pemain dalam game.
    private readonly List<IPlayer> _players;
    // Tangan domino setiap pemain.
    private readonly Dictionary<IPlayer, List<IDomino>> _dominoInHands;
    // Board tempat domino ditaruh.
    private readonly IBoard _board;
    // Boneyard sebagai sumber pengambilan domino.
    private IBoneyard _boneyard;

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
    private IPlayer? _gameWinner;
    // Skor target untuk menang.
    private readonly int _maxScoreToWin;

    // ================= PROPERTIES =================
    // Daftar pemain (read-only).
    public IReadOnlyList<IPlayer> Players => _players;
    // Pemain yang sedang mendapat giliran.
    public IPlayer CurrentPlayer => _players[_currentPlayerIndex];
    // Board yang dipakai game.
    public IBoard Board => _board;
    // Status ronde selesai.
    public bool IsRoundEnded => _roundEnded;
    // Status game selesai.
    public bool IsGameEnded => _isGameEnded;
    // Pemenang game (null jika belum ada).
    public IPlayer? GameWinner => _gameWinner;
    // public IEnumerable<Player> Players => _players;

    // ================= CONSTRUCTOR =================
    // Membuat controller game dengan daftar pemain, board, dan target skor.
    public GameController(List<IPlayer> players, IBoard board, int maxScoreToWin)
    {
        _players = players;
        _board = board;
        _maxScoreToWin = maxScoreToWin;
        _dominoInHands = players.ToDictionary(p => p, _ => new List<IDomino>());
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

        IPlayer player = CurrentPlayer;

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
    public IReadOnlyList<IDomino> GetHands(IPlayer player)
        => _dominoInHands[player].AsReadOnly();

    // Memainkan domino pada sisi tertentu.
    public bool PlayDomino(IPlayer player, IDomino domino, BoardSide side)
    {
        if (player != CurrentPlayer)
            return false;

        if (domino is not Domino concrete)
            throw new InvalidOperationException("Domino implementation tidak dikenal.");

        if (!CanPlace(concrete, side))
            return false;

        Place(concrete, side);
        _dominoInHands[player].Remove(domino);
        _consecutivePasses = 0;

        OnDominoPlaced?.Invoke(player, domino, side);
        
        return true;
    }

    // Melakukan pass untuk pemain saat ini.
    public bool PassTurn(IPlayer player)
    {
        if (player != CurrentPlayer)
            return false;

        _consecutivePasses++;
        OnPlayerPassed?.Invoke(player);
        
        return true;
    }

    // Mengecek apakah pemain bisa bermain pada giliran ini.
    public bool CanPlay(IPlayer player)
    {
        List<IDomino> hand = _dominoInHands[player];

        foreach (IDomino d in hand)
        {
            if (d is not Domino)
                throw new InvalidOperationException("Domino implementation tidak dikenal.");
        }

        if (_board.IsEmpty)
        {
            bool canPlay = hand.Count > 0;
            return canPlay;
        }

        bool hasPlayableDomino = hand
            .Any(d =>
            {
                if (d is not Domino concrete)
                    throw new InvalidOperationException("Domino implementation tidak dikenal.");
                return CanPlace(concrete, BoardSide.Left) ||
                       CanPlace(concrete, BoardSide.Right);
            });
        return hasPlayableDomino;
    }

    // Menjumlahkan total pip dari semua domino di tangan pemain.
    public int CountPips(IPlayer player)
    {
        int totalPips = _dominoInHands[player]
            .Sum(d => (int)d.LeftPip + (int)d.RightPip);
        return totalPips;
    }

    // ================= ROUND END =================
    // Mengecek apakah ronde berakhir karena ada yang habis kartu atau buntu.
    private void CheckRoundEnd()
    {
        // Normal win
        IPlayer? emptyPlayer = _players.FirstOrDefault(p => _dominoInHands[p].Count == 0);
        
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
    private void HandleNormalWin(IPlayer winner)
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
        Dictionary<IPlayer, int> pipTotals = _players.ToDictionary(p => p, CountPips);

        int min = pipTotals.Min(x => x.Value);
        List<KeyValuePair<IPlayer, int>> lowestPlayers = pipTotals.Where(x => x.Value == min).ToList();

        // Tie → no winner
        if (lowestPlayers.Count > 1)
        {
            OnRoundEnded?.Invoke(null, true, SnapshotHands());
            return;
        }

        IPlayer winner = lowestPlayers.First().Key;

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
        ResetBoard();
        _boneyard = new Boneyard(GenerateFullSet());
        _consecutivePasses = 0;
        _roundEnded = false;

        foreach (IPlayer p in _players)
            _dominoInHands[p].Clear();

    }

    // ================= BOARD LOGIC =================
    // Mengecek apakah domino bisa ditempatkan pada sisi tertentu (kiri/kanan).
    public bool CanPlace(Domino domino, BoardSide side)
    {
        if (_board.IsEmpty) return true;
        return side == BoardSide.Left
            ? domino.LeftPip == _board.LeftEnd || domino.RightPip == _board.LeftEnd
            : domino.LeftPip == _board.RightEnd || domino.RightPip == _board.RightEnd;
    }

    // Menaruh domino pada sisi board yang dipilih, dengan flip bila diperlukan.
    public void Place(Domino domino, BoardSide side)
    {
        if (!CanPlace(domino, side))
            throw new InvalidOperationException("Invalid placement");

        if (_board.IsEmpty)
        {
            _board.Dominoes.AddFirst(domino);
            return;
        }

        if (side == BoardSide.Left)
        {
            if (domino.RightPip == _board.LeftEnd)
                _board.Dominoes.AddFirst(domino);
            else
                _board.Dominoes.AddFirst(domino.Flip());
        }
        else
        {
            if (domino.LeftPip == _board.RightEnd)
                _board.Dominoes.AddLast(domino);
            else
                _board.Dominoes.AddLast(domino.Flip());
        }
    }

    // Mengosongkan board untuk memulai ronde baru.
    public void ResetBoard() => _board.Dominoes.Clear();

    // Membagikan 7 domino ke setiap pemain.
    private void DealInitialHands()
    {
        for (int i = 0; i < 7; i++)
            foreach (IPlayer p in _players)
                _dominoInHands[p].Add(_boneyard.Draw());
    }
    // Membuat set domino lengkap 0-0 sampai 6-6.
    private IEnumerable<Domino> GenerateFullSet()
    {
        for (int i = 0; i <= 6; i++)
            for (int j = i; j <= 6; j++)
                // yield return membuat domino satu per satu secara lazy tanpa membuat list dulu.
                yield return new Domino((Dot)i, (Dot)j);
    }

    // Membuat snapshot tangan semua pemain agar aman dipakai di event.
    private IReadOnlyDictionary<IPlayer, IReadOnlyList<IDomino>> SnapshotHands()
    {
        Dictionary<IPlayer, IReadOnlyList<IDomino>> snapshot = _dominoInHands.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<IDomino>)kvp.Value.AsReadOnly()
        );
        return snapshot;
    }
}
