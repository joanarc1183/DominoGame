using System.Windows;
using System.Windows.Controls;
using DominoGame.Wpf.Services;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DominoGame.Wpf;

public partial class App : Application
{
    /// Entry point aplikasi WPF: buka setup, buat view model, tampilkan window utama.
    protected override void OnStartup(StartupEventArgs e)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File("logs/domino-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(Log.Logger, dispose: true);
        });
        
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

        Log.CloseAndFlush();
    }

    private void AnyButton_Click(object sender, RoutedEventArgs e)
    {
        UiSoundService.PlayButtonClick();
    }
}
