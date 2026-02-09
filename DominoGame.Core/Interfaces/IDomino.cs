namespace DominoGame.Core;

public interface IDomino
{
    /// Nilai pip di sisi kiri domino.
    Dot LeftPip { get; }
    /// Nilai pip di sisi kanan domino.
    Dot RightPip { get; }
    /// Orientasi domino untuk keperluan tampilan.
    DominoOrientation Orientation { get; set; }
    /// Mengembalikan domino baru dengan sisi kiri dan kanan ditukar.
    Domino Flip();
}
