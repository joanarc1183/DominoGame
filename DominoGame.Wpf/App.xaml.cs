using System.Windows;

namespace DominoGame.Wpf;

public partial class App : Application
{
    /// Entry point aplikasi WPF: buka setup, buat view model, tampilkan window utama.
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ShutdownMode = ShutdownMode.OnMainWindowClose;
        var window = new StartMenuWindow();
        MainWindow = window;
        window.Show();
    }
}
