namespace DominoGame.Core;

public class Board : IBoard
{
    /// Koleksi domino yang sudah ditempatkan di board, urut dari kiri ke kanan.
    public LinkedList<Domino> Dominoes { get; private set; }
    /// Menandakan apakah board masih kosong (belum ada domino).
    public bool IsEmpty => Dominoes.Count == 0;
    /// Nilai pip di ujung kiri board (hanya valid bila board tidak kosong).
    public Dot LeftEnd => Dominoes.First!.Value.LeftPip;
    /// Nilai pip di ujung kanan board (hanya valid bila board tidak kosong).
    public Dot RightEnd => Dominoes.Last!.Value.RightPip;
    
    public Board()
    {
        Dominoes = new LinkedList<Domino>();
    }
}
