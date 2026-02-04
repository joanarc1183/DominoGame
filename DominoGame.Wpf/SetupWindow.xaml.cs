using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DominoGame.Core;

namespace DominoGame.Wpf;

public partial class SetupWindow : Window
{
    public List<Player> Players { get; } = new();
    public int MaxScoreToWin { get; private set; } = 100;

    public SetupWindow()
    {
        InitializeComponent();
        BuildPlayerInputs(2);
        PlayerCountCombo.SelectionChanged += PlayerCountCombo_SelectionChanged;
    }

    private void PlayerCountCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        int count = GetSelectedPlayerCount();
        BuildPlayerInputs(count);
    }

    private int GetSelectedPlayerCount()
    {
        if (PlayerCountCombo.SelectedItem is ComboBoxItem item &&
            int.TryParse(item.Content?.ToString(), out int count))
            return count;

        return 2;
    }

    private int GetSelectedScoreTarget()
    {
        if (ScoreTargetCombo.SelectedItem is ComboBoxItem item &&
            int.TryParse(item.Content?.ToString(), out int score))
            return score;

        return 100;
    }

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

    private void Start_Click(object sender, RoutedEventArgs e)
    {
        Players.Clear();
        MaxScoreToWin = GetSelectedScoreTarget();
        int index = 1;

        foreach (var child in PlayersPanel.Children)
        {
            if (child is TextBox box)
            {
                string name = string.IsNullOrWhiteSpace(box.Text)
                    ? $"Player {index}"
                    : box.Text.Trim();

                Players.Add(new Player(name));
                index++;
            }
        }

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
