namespace DominoGame.Core;

public class Board : IBoard
{
    public LinkedList<Domino> Dominoes { get; private set; } = new();
    public bool IsEmpty => Dominoes.Count == 0;
    public Dot LeftEnd => Dominoes.First!.Value.LeftPip;
    public Dot RightEnd => Dominoes.Last!.Value.RightPip;

    public bool CanPlace(Domino domino)
    {
        if (IsEmpty) return true;
        return CanPlace(domino, BoardSide.Left) || CanPlace(domino, BoardSide.Right);
    }

    public bool CanPlace(Domino domino, BoardSide side)
    {
        if (IsEmpty) return true;
        return side == BoardSide.Left
            ? domino.LeftPip == LeftEnd || domino.RightPip == LeftEnd
            : domino.LeftPip == RightEnd || domino.RightPip == RightEnd;
    }

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

    public void Reset() => Dominoes.Clear();
}