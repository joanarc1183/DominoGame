using System.Windows;

namespace DominoGame.Wpf;

public partial class PauseWindow : Window
{
    public PauseWindow()
    {
        InitializeComponent();
    }

    private void Resume_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    // Menutup dialog, kembali ke setup menu.
    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
