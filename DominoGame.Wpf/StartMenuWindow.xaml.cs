using System.Windows;

namespace DominoGame.Wpf;

public partial class StartMenuWindow : Window
{
    /// Inisialisasi start menu window.
    public StartMenuWindow()
    {
        InitializeComponent();
    }

    /// Handler tombol Start: buka setup, buat game, tampilkan main window.
    private void Start_Click(object sender, RoutedEventArgs e)
    {
        var setup = new SetupWindow
        {
            Owner = this
        };

        if (setup.ShowDialog() == true)
        {
            var app = (App)Application.Current;
            var gameVm = new GameViewModel(setup.Players, setup.MaxScoreToWin, app.AppLoggerFactory);
            var main = new MainWindow(gameVm);
            Application.Current.MainWindow = main;
            main.Show();
            Close();
        }
    }

    /// Handler tombol Exit: keluar dari aplikasi.
    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}
