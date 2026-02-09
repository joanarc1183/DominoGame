using System.Collections.ObjectModel;
using System.Windows;
using DominoGame.Wpf.ViewModels;

namespace DominoGame.Wpf;

public partial class GameOverWindow : Window
{
    /// Inisialisasi window game over dan set data pemenang.
    public GameOverWindow(ObservableCollection<PlayerScoreViewModel> players, string winnerName)
    {
        InitializeComponent();
        DataContext = new GameOverViewModel(players, $"Pemenang: {winnerName}");
    }

    /// Handler tombol Main Lagi: tutup dialog dengan hasil true.
    private void PlayAgain_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    /// Handler tombol Exit: keluar dari aplikasi.
    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}
