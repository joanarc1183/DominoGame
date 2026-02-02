using System.Collections.Generic;

namespace DominoGame.Core;

public class GameController
{
    private readonly List<Player> _players;
    private readonly IBoard _board;
    private int _currentPlayerIndex;

    public IBoard Board => _board;
    public Player CurrentPlayer => _players[_currentPlayerIndex];

    public GameController(List<Player> players, IBoard board, int startPlayerIndex = 0)
    {
        _players = players;
        _board = board;
        _currentPlayerIndex = startPlayerIndex;
    }

    // Check if domino can be played on either side
    public bool CanPlay(Domino domino)
    {
        var leftEnd = _board.GetLeftEnd();
        var rightEnd = _board.GetRightEnd();

        return domino.LeftPip == leftEnd || domino.RightPip == leftEnd ||
               domino.LeftPip == rightEnd || domino.RightPip == rightEnd;
    }

    // Play domino, returns true if successful
    public bool PlayDomino(Player player, Domino domino, BoardSide side)
    {
        if (!_players.Contains(player) || !player.Hand.Contains(domino))
            return false;

        if (!CanPlay(domino))
            return false;

        _board.PlaceDomino(domino, side);
        player.Hand.Remove(domino);

        // Next turn
        _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
        return true;
    }

    public bool CanPlayOnSide(Domino domino, BoardSide side)
    {
        var leftEnd = _board.GetLeftEnd();
        var rightEnd = _board.GetRightEnd();

        return side switch
        {
            BoardSide.Left => domino.LeftPip == leftEnd || domino.RightPip == leftEnd,
            BoardSide.Right => domino.LeftPip == rightEnd || domino.RightPip == rightEnd,
            _ => false
        };
    }

    // Dapatkan semua domino valid untuk player
    public List<Domino> GetPlayableDominoes(Player player)
    {
        var list = new List<Domino>();
        foreach (var d in player.Hand)
            if (CanPlayOnSide(d, BoardSide.Left) || CanPlayOnSide(d, BoardSide.Right))
                list.Add(d);
        return list;
    }

}
