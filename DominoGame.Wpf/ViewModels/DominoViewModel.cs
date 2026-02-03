using DominoGame.Core;

namespace DominoGame.Wpf.ViewModels;

public class DominoViewModel
{
    public Domino Domino { get; }

    public DominoViewModel(Domino domino)
    {
        Domino = domino;
    }

    public int Left => (int)Domino.LeftPip;
    public int Right => (int)Domino.RightPip;
}
