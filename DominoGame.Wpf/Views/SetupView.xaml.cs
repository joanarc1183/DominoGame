using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DominoGame.Core;

namespace DominoGame.Wpf.Views;

public class SetupCompletedEventArgs : EventArgs
{
    /// Event args saat setup selesai berisi pemain dan target skor.
    public SetupCompletedEventArgs(List<Player> players, int maxScore)
    {
        Players = players;
        MaxScore = maxScore;
    }

    /// Daftar pemain yang diinput.
    public List<Player> Players { get; }
    /// Target skor untuk menang.
    public int MaxScore { get; }
}

public partial class SetupView : UserControl
{
    /// Event saat tombol Start ditekan.
    public event EventHandler<SetupCompletedEventArgs>? StartRequested;
    /// Event saat tombol Cancel ditekan.
    public event EventHandler? CancelRequested;

    /// Inisialisasi SetupView dan input default.
    public SetupView()
    {
        InitializeComponent();
        BuildPlayerInputs(2);
        PlayerCountCombo.SelectionChanged += PlayerCountCombo_SelectionChanged;
    }

    /// Handler perubahan jumlah pemain.
    private void PlayerCountCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        int count = GetSelectedPlayerCount();
        BuildPlayerInputs(count);
    }

    /// Mengambil jumlah pemain dari ComboBox.
    private int GetSelectedPlayerCount()
    {
        if (PlayerCountCombo.SelectedItem is ComboBoxItem item &&
            int.TryParse(item.Content?.ToString(), out int count))
            return count;

        return 2;
    }

    /// Mengambil target skor dari ComboBox.
    private int GetSelectedScoreTarget()
    {
        if (ScoreTargetCombo.SelectedItem is ComboBoxItem item &&
            int.TryParse(item.Content?.ToString(), out int score))
            return score;

        return 100;
    }

    /// Membuat input nama pemain sesuai jumlah.
    private void BuildPlayerInputs(int count)
    {
        PlayersPanel.Children.Clear();
        for (int i = 1; i <= count; i++)
        {
            var box = new TextBox
            {
                Margin = new Thickness(0, 0, 0, 8),
                Padding = new Thickness(8),
                MinHeight = 32,
                Text = $"Player {i}"
            };
            if (Application.Current.TryFindResource("InputTextBoxStyle") is Style style)
                box.Style = style;
            PlayersPanel.Children.Add(box);
        }
    }

    /// Handler tombol Start: kumpulkan data dan kirim event.
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

    /// Handler tombol Cancel: kirim event cancel.
    private void Cancel_Click(object sender, RoutedEventArgs e)
        => CancelRequested?.Invoke(this, EventArgs.Empty);
}
