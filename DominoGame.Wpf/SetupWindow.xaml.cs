using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DominoGame.Core;

namespace DominoGame.Wpf;

public partial class SetupWindow : Window
{
    // Diisi dari UI dan dibaca oleh pemanggil saat dialog selesai.
    public List<Player> Players { get; } = new();
    public int MaxScoreToWin { get; private set; } = 100;

    public SetupWindow()
    {
        InitializeComponent();
        // Default 2 pemain dan sinkronkan input nama dengan pilihan jumlah pemain.
        BuildPlayerInputs(2);
        PlayerCountCombo.SelectionChanged += PlayerCountCombo_SelectionChanged;
    }

    // Menangani perubahan jumlah pemain dari combo box.
    private void PlayerCountCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        int count = GetSelectedPlayerCount();
        BuildPlayerInputs(count);
    }

    // Mengambil jumlah pemain yang dipilih pada UI.
    private int GetSelectedPlayerCount()
    {
        // Baca jumlah pemain dari combo box, default ke 2.
        if (PlayerCountCombo.SelectedItem is ComboBoxItem item &&
            int.TryParse(item.Content?.ToString(), out int count))
            return count;

        return 2;
    }

    // Mengambil target skor kemenangan yang dipilih pada UI.
    private int GetSelectedScoreTarget()
    {
        // Baca target skor dari combo box, default ke 100.
        if (ScoreTargetCombo.SelectedItem is ComboBoxItem item &&
            int.TryParse(item.Content?.ToString(), out int score))
            return score;

        return 100;
    }

    // Membuat ulang textbox input nama pemain di panel.
    private void BuildPlayerInputs(int count)
    {
        // Bangun ulang input nama berdasarkan jumlah pemain terpilih.
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

    // Menyimpan data pemain dan menutup dialog dengan hasil sukses.
    private void Start_Click(object sender, RoutedEventArgs e)
    {
        // Kumpulkan nama dan pengaturan sebelum menutup dialog dengan sukses.
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

    // Menutup dialog dengan hasil batal.
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        // Beri sinyal pembatalan ke pemanggil.
        DialogResult = false;
        Close();
    }
}
