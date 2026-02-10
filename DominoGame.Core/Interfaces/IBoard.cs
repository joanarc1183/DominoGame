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
}
