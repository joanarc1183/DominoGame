using DominoGame.Core;

namespace DominoGame.Wpf.Models;

public class DominoTileViewModel
{
    public Domino Domino { get; }
    public Dot Left => Domino.LeftPip;
    public Dot Right => Domino.RightPip;

    public DominoTileViewModel(Domino domino) => Domino = domino;
}
