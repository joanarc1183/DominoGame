using System.IO;
using System.Windows;
using System.Windows.Controls;
using DominoGame.Wpf.Services;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;

namespace DominoGame.Wpf;

public partial class App : Application
{
    public ILoggerFactory AppLoggerFactory { get; private set; } = LoggerFactory.Create(_ => { });

    /// Entry point aplikasi WPF: buka setup, buat view model, tampilkan window utama.
    protected override void OnStartup(StartupEventArgs e)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                new CompactJsonFormatter(),
                Path.Combine(AppContext.BaseDirectory, "logs", "domino-.json"),
                rollingInterval: RollingInterval.Day)

            .CreateLogger();
        Log.Information("Application started");

        AppLoggerFactory = LoggerFactory.Create(builder =>
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
        AppLoggerFactory.Dispose();
        base.OnExit(e);

        Log.CloseAndFlush();
    }

    private void AnyButton_Click(object sender, RoutedEventArgs e)
    {
        UiSoundService.PlayButtonClick();
    }
}
