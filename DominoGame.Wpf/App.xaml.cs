using System.Windows;
using System.Windows.Controls;
using DominoGame.Wpf.Services;

namespace DominoGame.Wpf;

public partial class App : Application
{
    /// Entry point aplikasi WPF: buka setup, buat view model, tampilkan window utama.
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        EventManager.RegisterClassHandler(
            typeof(Button),
            Button.ClickEvent,
            new RoutedEventHandler(AnyButton_Click),
            true);

        UiSoundService.StartBackgroundMusic();

        ShutdownMode = ShutdownMode.OnMainWindowClose;
        var window = new StartMenuWindow();
        MainWindow = window;
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        UiSoundService.StopBackgroundMusic();
        base.OnExit(e);
    }

    private void AnyButton_Click(object sender, RoutedEventArgs e)
    {
        UiSoundService.PlayButtonClick();
    }
}
