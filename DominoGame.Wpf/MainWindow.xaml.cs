using System.Windows;
using DominoGame.Wpf.ViewModels;

namespace DominoGame.Wpf;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new GameViewModel();
    }
}
