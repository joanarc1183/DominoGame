using System;
using System.Windows;
using System.Windows.Controls;
using DominoGame.Wpf.ViewModels;

namespace DominoGame.Wpf.Views;

public partial class GameOverView : UserControl
{
    /// <summary>
    /// Event saat tombol Main Lagi ditekan.
    /// </summary>
    public event EventHandler? PlayAgainRequested;
    /// <summary>
    /// Event saat tombol Kembali ke Menu ditekan.
    /// </summary>
    public event EventHandler? BackToMenuRequested;

    /// <summary>
    /// Inisialisasi GameOverView.
    /// </summary>
    public GameOverView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Mengisi data hasil akhir ke DataContext.
    /// </summary>
    public void SetResults(System.Collections.ObjectModel.ObservableCollection<PlayerScoreViewModel> players, string winnerName)
    {
        DataContext = new GameOverViewModel(players, $"Pemenang: {winnerName}");
    }

    /// <summary>
    /// Handler tombol Main Lagi: trigger event PlayAgainRequested.
    /// </summary>
    private void PlayAgain_Click(object sender, RoutedEventArgs e)
        => PlayAgainRequested?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Handler tombol Kembali ke Menu: trigger event BackToMenuRequested.
    /// </summary>
    private void BackToMenu_Click(object sender, RoutedEventArgs e)
        => BackToMenuRequested?.Invoke(this, EventArgs.Empty);
}
