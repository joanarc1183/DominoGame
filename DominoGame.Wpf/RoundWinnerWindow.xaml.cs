using System.Windows;
using DominoGame.Wpf.ViewModels;

namespace DominoGame.Wpf;

public partial class RoundWinnerWindow : Window
{
    public RoundWinnerWindow(string message)
    {
        InitializeComponent();
        DataContext = new RoundWinnerViewModel(message);
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
