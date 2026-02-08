using System.Collections.ObjectModel;

namespace DominoGame.Wpf.ViewModels;

public class GameOverViewModel
{
    /// Menyimpan daftar pemain dan nama pemenang untuk layar game over.
    public GameOverViewModel(ObservableCollection<PlayerScoreViewModel> players, string winnerName)
    {
        Players = players;
        WinnerName = winnerName;
    }

    /// Daftar pemain beserta skor akhir.
    public ObservableCollection<PlayerScoreViewModel> Players { get; }
    /// Nama pemenang yang ditampilkan di UI.
    public string WinnerName { get; }
}
