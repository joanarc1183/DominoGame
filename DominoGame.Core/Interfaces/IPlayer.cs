namespace DominoGame.Core;

public interface IPlayer
{
    /// Nama pemain.
    string Name { get; }
    /// Skor pemain saat ini.
    int Score { get; set; }
}
