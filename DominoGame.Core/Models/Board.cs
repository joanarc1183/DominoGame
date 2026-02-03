namespace DominoGame.Core;

public class Board : IBoard
{
    private readonly LinkedList<Domino> _dominoes = new();

    public IReadOnlyList<Domino> Dominoes => _dominoes.ToList();

    public bool CanPlace(Domino domino, BoardSide side)
    {
        if (_dominoes.Count == 0)
            return true;

        var leftValue = _dominoes.First!.Value.LeftPip;
        var rightValue = _dominoes.Last!.Value.RightPip;

        return side switch
        {
            BoardSide.Left =>
                domino.LeftPip == leftValue || domino.RightPip == leftValue,

            BoardSide.Right =>
                domino.LeftPip == rightValue || domino.RightPip == rightValue,

            _ => false
        };
    }

    public void Place(Domino domino, BoardSide side)
    {
        if (!CanPlace(domino, side))
            throw new InvalidOperationException("Invalid domino placement");

        if (_dominoes.Count == 0)
        {
            _dominoes.AddFirst(domino);
            return;
        }

        if (side == BoardSide.Left)
        {
            var target = _dominoes.First!.Value.LeftPip;

            if (domino.RightPip != target)
                domino = domino.Flip();

            _dominoes.AddFirst(domino);
        }
        else
        {
            var target = _dominoes.Last!.Value.RightPip;

            if (domino.LeftPip != target)
                domino = domino.Flip();

            _dominoes.AddLast(domino);
        }
    }
}
