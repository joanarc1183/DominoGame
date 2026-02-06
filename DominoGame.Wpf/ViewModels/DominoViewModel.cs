using DominoGame.Core;

namespace DominoGame.Wpf.ViewModels;

public class DominoViewModel
{
    /// Model domino yang dibungkus view model ini.
    public Domino Domino { get; }

    /// Membuat view model dari model domino.
    public DominoViewModel(Domino domino)
    {
        Domino = domino;
    }

    /// Nilai pip kiri dalam bentuk int untuk binding.
    public int Left => (int)Domino.LeftPip;
    /// Nilai pip kanan dalam bentuk int untuk binding.
    public int Right => (int)Domino.RightPip;
}
