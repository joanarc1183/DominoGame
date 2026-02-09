using System.Windows;
using DominoGame.Core;
using DominoGame.Wpf.ViewModels;
using DominoGame.Wpf.Views;

namespace DominoGame.Wpf;

public partial class MainWindow : Window
{
    // View model utama game yang diikat ke UI.
    private GameViewModel _viewModel;

    /// Inisialisasi main window dengan view model dan event handler.
    public MainWindow(GameViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        GameHost.SetViewModel(_viewModel);

        _viewModel.GameEnded += HandleGameEnded;
        _viewModel.RoundEnded += HandleRoundEnded;
    }

    /// Handler saat ronde selesai: tampilkan pesan info.
    private void HandleRoundEnded(string message)
    {
        MessageBox.Show(
            message,
            "Ronde Selesai",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    /// Handler saat game selesai: tampilkan dialog game over.
    private void HandleGameEnded(IPlayer winner)
    {
        var gameOver = new GameOverWindow(_viewModel.Players, winner.Name)
        {
            Owner = this
        };

        bool? playAgain = gameOver.ShowDialog();
        if (playAgain == true)
        {
            ShowSetupAndRestart();
            return;
        }

        BackToMenu();
    }

    /// Menampilkan setup untuk restart permainan.
    private void ShowSetupAndRestart()
    {
        var setup = new SetupWindow
        {
            Owner = this
        };

        if (setup.ShowDialog() == true)
        {
            var gameVm = new GameViewModel(setup.Players, setup.MaxScoreToWin);
            var window = new MainWindow(gameVm);
            Application.Current.MainWindow = window;
            window.Show();
            Close();
            return;
        }

        BackToMenu();
    }

    /// Kembali ke menu awal dan menutup window saat ini.
    private void BackToMenu()
    {
        var startMenu = new StartMenuWindow();
        Application.Current.MainWindow = startMenu;
        startMenu.Show();
        Close();
    }
}
