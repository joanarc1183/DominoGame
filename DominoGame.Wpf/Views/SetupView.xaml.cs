using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DominoGame.Core;

namespace DominoGame.Wpf.Views;

public class SetupCompletedEventArgs : EventArgs
{
    /// <summary>
    /// Event args saat setup selesai berisi pemain dan target skor.
    /// </summary>
    public SetupCompletedEventArgs(List<Player> players, int maxScore)
    {
        Players = players;
        MaxScore = maxScore;
    }

    /// <summary>
    /// Daftar pemain yang diinput.
    /// </summary>
    public List<Player> Players { get; }
    /// <summary>
    /// Target skor untuk menang.
    /// </summary>
    public int MaxScore { get; }
}

public partial class SetupView : UserControl
{
    /// <summary>
    /// Event saat tombol Start ditekan.
    /// </summary>
    public event EventHandler<SetupCompletedEventArgs>? StartRequested;
    /// <summary>
    /// Event saat tombol Cancel ditekan.
    /// </summary>
    public event EventHandler? CancelRequested;

    /// <summary>
    /// Inisialisasi SetupView dan input default.
    /// </summary>
    public SetupView()
    {
        InitializeComponent();
        BuildPlayerInputs(2);
        PlayerCountCombo.SelectionChanged += PlayerCountCombo_SelectionChanged;
    }

    /// <summary>
    /// Handler perubahan jumlah pemain.
    /// </summary>
    private void PlayerCountCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        int count = GetSelectedPlayerCount();
        BuildPlayerInputs(count);
    }

    /// <summary>
    /// Mengambil jumlah pemain dari ComboBox.
    /// </summary>
    private int GetSelectedPlayerCount()
    {
        if (PlayerCountCombo.SelectedItem is ComboBoxItem item &&
            int.TryParse(item.Content?.ToString(), out int count))
            return count;

        return 2;
    }

    /// <summary>
    /// Mengambil target skor dari ComboBox.
    /// </summary>
    private int GetSelectedScoreTarget()
    {
        if (ScoreTargetCombo.SelectedItem is ComboBoxItem item &&
            int.TryParse(item.Content?.ToString(), out int score))
            return score;

        return 100;
    }

    /// <summary>
    /// Membuat input nama pemain sesuai jumlah.
    /// </summary>
    private void BuildPlayerInputs(int count)
    {
        PlayersPanel.Children.Clear();
        for (int i = 1; i <= count; i++)
        {
            var box = new TextBox
            {
                Margin = new Thickness(0, 0, 0, 8),
                Padding = new Thickness(8),
                Text = $"Player {i}"
            };
            PlayersPanel.Children.Add(box);
        }
    }

    /// <summary>
    /// Handler tombol Start: kumpulkan data dan kirim event.
    /// </summary>
    private void Start_Click(object sender, RoutedEventArgs e)
    {
        var players = new List<Player>();
        int maxScore = GetSelectedScoreTarget();
        int index = 1;

        foreach (var child in PlayersPanel.Children)
        {
            if (child is not TextBox box)
                continue;

            string name = string.IsNullOrWhiteSpace(box.Text)
                ? $"Player {index}"
                : box.Text.Trim();

            players.Add(new Player(name));
            index++;
        }

        StartRequested?.Invoke(this, new SetupCompletedEventArgs(players, maxScore));
    }

    /// <summary>
    /// Handler tombol Cancel: kirim event cancel.
    /// </summary>
    private void Cancel_Click(object sender, RoutedEventArgs e)
        => CancelRequested?.Invoke(this, EventArgs.Empty);
}
