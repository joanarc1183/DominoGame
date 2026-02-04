using System.Windows;
using DominoGame.Core;

namespace DominoGame.Wpf;

public partial class MainWindow : Window
{
    private GameViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(GameViewModel viewModel) : this()
    {
        SetViewModel(viewModel);
    }

    private void SetViewModel(GameViewModel viewModel)
    {
        if (_viewModel is not null)
            _viewModel.GameEnded -= HandleGameEnded;

        _viewModel = viewModel;
        _viewModel.GameEnded += HandleGameEnded;
        DataContext = _viewModel;
    }

    private void HandleGameEnded(Player winner)
    {
        var result = MessageBox.Show(
            $"{winner.Name} menang! Main lagi?",
            "Game Over",
            MessageBoxButton.YesNo,
            MessageBoxImage.Information);

        if (result == MessageBoxResult.Yes)
        {
            Hide();
            var setup = new SetupWindow();
            if (setup.ShowDialog() == true)
            {
                var vm = new GameViewModel(setup.Players, maxScoreToWin: setup.MaxScoreToWin);
                var newWindow = new MainWindow(vm);
                Application.Current.MainWindow = newWindow;
                newWindow.Show();
                Close();
                return;
            }

            Close();
            return;
        }

        Close();
    }
}
