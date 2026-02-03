namespace DominoGame.Core;

public interface IBoard
{
    IReadOnlyList<Domino> Dominoes { get; }

    bool CanPlace(Domino domino, BoardSide side);
    void Place(Domino domino, BoardSide side);
}
