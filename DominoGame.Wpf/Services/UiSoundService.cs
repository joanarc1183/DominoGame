using System;
using System.IO;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media;

namespace DominoGame.Wpf.Services;

public static class UiSoundService
{
    private static readonly object _lock = new();
    private static MediaPlayer? _buttonPlayer;
    private static MediaPlayer? _dominoPlayer;
    private static MediaPlayer? _backgroundPlayer;

    public static void PlayButtonClick()
    {
        try
        {
            EnsureButtonPlayer();
            if (_buttonPlayer is null)
                return;

            _buttonPlayer.Position = TimeSpan.Zero;
            _buttonPlayer.Play();
        }
        catch
        {
            // Ignore sound failures to avoid blocking UI actions.
        }
    }

    public static void PlayDominoPlaced()
    {
        try
        {
            EnsureDominoPlayer();
            if (_dominoPlayer is null)
                return;

            _dominoPlayer.Position = TimeSpan.Zero;
            _dominoPlayer.Play();
        }
        catch
        {
            // Ignore sound failures to avoid blocking UI actions.
        }
    }

    public static void StartBackgroundMusic()
    {
        try
        {
            EnsureBackgroundPlayer();
            _backgroundPlayer?.Play();
        }
        catch
        {
            // Ignore sound failures to avoid blocking UI actions.
        }
    }

    public static void StopBackgroundMusic()
    {
        try
        {
            _backgroundPlayer?.Stop();
        }
        catch
        {
            // Ignore sound failures to avoid blocking UI actions.
        }
    }

    private static void EnsureButtonPlayer()
    {
        lock (_lock)
        {
            if (_buttonPlayer is not null)
                return;

            var path = GetSoundPath("Assets/Sounds/Button.mp3");
            if (path is null)
                return;

            var player = new MediaPlayer
            {
                Volume = 1.0
            };
            player.Open(new Uri(path, UriKind.Absolute));
            _buttonPlayer = player;
        }
    }

    private static void EnsureDominoPlayer()
    {
        lock (_lock)
        {
            if (_dominoPlayer is not null)
                return;

            var path = GetSoundPath("Assets/Sounds/Domino.mp3");
            if (path is null)
                return;

            var player = new MediaPlayer
            {
                Volume = 1.0
            };
            player.Open(new Uri(path, UriKind.Absolute));
            _dominoPlayer = player;
        }
    }

    private static void EnsureBackgroundPlayer()
    {
        lock (_lock)
        {
            if (_backgroundPlayer is not null)
                return;

            var path = GetSoundPath("Assets/Sounds/Music.mp3");
            if (path is null)
                return;

            void InitPlayer()
            {
                var player = new MediaPlayer();
                player.Open(new Uri(path, UriKind.Absolute));
                player.MediaEnded += (_, _) =>
                {
                    player.Position = TimeSpan.Zero;
                    player.Play();
                };
                player.Volume = 0.4;
                _backgroundPlayer = player;
            }

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher is not null)
            {
                if (dispatcher.CheckAccess())
                    InitPlayer();
                else
                    dispatcher.Invoke(InitPlayer);
            }
            else
            {
                InitPlayer();
            }
        }
    }

    private static string? GetSoundPath(string relativePath)
    {
        var baseDir = AppContext.BaseDirectory;
        var basePath = Path.Combine(baseDir, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(basePath))
            return basePath;

        var cwdPath = Path.GetFullPath(relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(cwdPath))
            return cwdPath;

        return null;
    }
}
