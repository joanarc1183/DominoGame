using System;
using System.Windows;
using System.Windows.Controls;

namespace DominoGame.Wpf.Views;

public partial class StartMenuView : UserControl
{
    /// <summary>
    /// Event saat tombol Start ditekan.
    /// </summary>
    public event EventHandler? StartRequested;
    /// <summary>
    /// Event saat tombol Exit ditekan.
    /// </summary>
    public event EventHandler? ExitRequested;

    /// <summary>
    /// Inisialisasi StartMenuView.
    /// </summary>
    public StartMenuView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handler tombol Start: trigger event StartRequested.
    /// </summary>
    private void Start_Click(object sender, RoutedEventArgs e)
        => StartRequested?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Handler tombol Exit: trigger event ExitRequested.
    /// </summary>
    private void Exit_Click(object sender, RoutedEventArgs e)
        => ExitRequested?.Invoke(this, EventArgs.Empty);
}
