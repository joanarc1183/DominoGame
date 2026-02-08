using System;
using System.Windows;
using System.Windows.Controls;
using DominoGame.Wpf.ViewModels;

namespace DominoGame.Wpf.Views;

public partial class GameOverView : UserControl
{
    /// Event saat tombol Main Lagi ditekan.
    public event EventHandler? PlayAgainRequested;
    /// Event saat tombol Kembali ke Menu ditekan.
    public event EventHandler? BackToMenuRequested;

    /// Inisialisasi GameOverView.
    public GameOverView()
    {
        InitializeComponent();
    }

    /// Mengisi data hasil akhir ke DataContext.
    public void SetResults(System.Collections.ObjectModel.ObservableCollection<PlayerScoreViewModel> players, string winnerName)
    {
        DataContext = new GameOverViewModel(players, $"Pemenang: {winnerName}");
    }

    /// Handler tombol Main Lagi: trigger event PlayAgainRequested.
    private void PlayAgain_Click(object sender, RoutedEventArgs e)
        => PlayAgainRequested?.Invoke(this, EventArgs.Empty);

    /// Handler tombol Kembali ke Menu: trigger event BackToMenuRequested.
    private void BackToMenu_Click(object sender, RoutedEventArgs e)
        => BackToMenuRequested?.Invoke(this, EventArgs.Empty);
}
