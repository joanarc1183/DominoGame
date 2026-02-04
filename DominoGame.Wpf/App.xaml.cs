using System.Configuration;
using System.Data;
using System.Windows;

namespace DominoGame.Wpf;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var setup = new SetupWindow();
        if (setup.ShowDialog() == true)
        {
            var gameVm = new GameViewModel(setup.Players, maxScoreToWin: setup.MaxScoreToWin);
            var window = new MainWindow(gameVm);
            MainWindow = window;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            window.Show();
            return;
        }

        Shutdown();
    }
}
