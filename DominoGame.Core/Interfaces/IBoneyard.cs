namespace DominoGame.Core;

public interface IBoneyard
{
    /// Daftar domino yang masih tersisa di boneyard.
    List<Domino> Dominoes { get; }
    /// Menandakan apakah boneyard sudah habis.
    bool IsEmpty { get; }
}
