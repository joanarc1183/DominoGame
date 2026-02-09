namespace DominoGame.Core;

public interface IBoard
{
    /// Koleksi domino yang sudah ditempatkan di board.
    LinkedList<Domino> Dominoes { get; }
    /// Menandakan apakah board kosong.
    bool IsEmpty { get; }
    /// Nilai pip di ujung kiri board.
    Dot LeftEnd { get; }
    /// Nilai pip di ujung kanan board.
    Dot RightEnd { get; }
    /// Mengecek apakah domino bisa ditempatkan di sisi mana pun.
    bool CanPlace(Domino domino);
    /// Mengecek apakah domino bisa ditempatkan pada sisi tertentu.
    bool CanPlace(Domino domino, BoardSide side);
    /// Menaruh domino pada sisi tertentu.
    void Place(Domino domino, BoardSide side);
    /// Mengosongkan board.
    void Reset();
}
