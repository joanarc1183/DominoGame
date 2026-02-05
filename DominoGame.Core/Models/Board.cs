namespace DominoGame.Core;

public class Board : IBoard
{
    /// Koleksi domino yang sudah ditempatkan di board, urut dari kiri ke kanan.
    public LinkedList<Domino> Dominoes { get; private set; } = new();
    /// Menandakan apakah board masih kosong (belum ada domino).
    public bool IsEmpty => Dominoes.Count == 0;
    /// Nilai pip di ujung kiri board (hanya valid bila board tidak kosong).
    public Dot LeftEnd => Dominoes.First!.Value.LeftPip;
    /// Nilai pip di ujung kanan board (hanya valid bila board tidak kosong).
    public Dot RightEnd => Dominoes.Last!.Value.RightPip;

    /// Mengecek apakah domino bisa ditempatkan di sisi mana pun pada board.
    public bool CanPlace(Domino domino)
    {
        if (IsEmpty) return true;
        return CanPlace(domino, BoardSide.Left) || CanPlace(domino, BoardSide.Right);
    }

    /// Mengecek apakah domino bisa ditempatkan pada sisi tertentu (kiri/kanan).
    public bool CanPlace(Domino domino, BoardSide side)
    {
        if (IsEmpty) return true;
        return side == BoardSide.Left
            ? domino.LeftPip == LeftEnd || domino.RightPip == LeftEnd
            : domino.LeftPip == RightEnd || domino.RightPip == RightEnd;
    }

    /// Menaruh domino pada sisi board yang dipilih, dengan flip bila diperlukan.
    public void Place(Domino domino, BoardSide side)
    {
        if (!CanPlace(domino, side))
            throw new InvalidOperationException("Invalid placement");

        if (IsEmpty)
        {
            Dominoes.AddFirst(domino);
            return;
        }

        if (side == BoardSide.Left)
        {
            if (domino.RightPip == LeftEnd)
                Dominoes.AddFirst(domino);
            else
                Dominoes.AddFirst(((Domino)domino).Flip());
        }
        else
        {
            if (domino.LeftPip == RightEnd)
                Dominoes.AddLast(domino);
            else
                Dominoes.AddLast(((Domino)domino).Flip());
        }
    }

    /// Mengosongkan board untuk memulai ronde baru.
    public void Reset() => Dominoes.Clear();
}