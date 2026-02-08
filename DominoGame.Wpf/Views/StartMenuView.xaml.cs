using System;
using System.Windows;
using System.Windows.Controls;

namespace DominoGame.Wpf.Views;

public partial class StartMenuView : UserControl
{
    /// Event saat tombol Start ditekan.
    public event EventHandler? StartRequested;
    /// Event saat tombol Exit ditekan.
    public event EventHandler? ExitRequested;

    /// Inisialisasi StartMenuView.
    public StartMenuView()
    {
        InitializeComponent();
    }

    /// Handler tombol Start: trigger event StartRequested.
    private void Start_Click(object sender, RoutedEventArgs e)
        => StartRequested?.Invoke(this, EventArgs.Empty);

    /// Handler tombol Exit: trigger event ExitRequested.
    private void Exit_Click(object sender, RoutedEventArgs e)
        => ExitRequested?.Invoke(this, EventArgs.Empty);
}
