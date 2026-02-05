using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DominoGame.Core;

namespace DominoGame.Wpf.ViewModels;

public class PlayerScoreViewModel : INotifyPropertyChanged
{
    // Model pemain yang menjadi sumber data.
    private readonly Player _player;
    // Menandakan apakah pemain ini sedang mendapatkan giliran.
    private bool _isCurrent;

    /// Membuat view model skor dari model pemain.
    public PlayerScoreViewModel(Player player)
    {
        _player = player;
    }

    /// Model pemain asli.
    public Player Player => _player;
    /// Nama pemain untuk ditampilkan.
    public string Name => _player.Name;
    /// Skor pemain untuk ditampilkan.
    public int Score => _player.Score;
    /// Tangan pemain dalam bentuk tile view model.
    public ObservableCollection<DominoTileViewModel> Hand { get; } = new();

    /// Status apakah pemain sedang mendapat giliran.
    public bool IsCurrent
    {
        get => _isCurrent;
        set
        {
            if (_isCurrent == value) return;
            _isCurrent = value;
            OnPropertyChanged(nameof(IsCurrent));
        }
    }

    /// Menyegarkan binding skor.
    public void Refresh()
    {
        OnPropertyChanged(nameof(Score));
    }

    /// Mengganti isi tangan pemain sesuai data terbaru.
    public void RefreshHand(IEnumerable<Domino> dominoes)
    {
        Hand.Clear();
        foreach (var domino in dominoes)
            Hand.Add(new DominoTileViewModel(domino));

        OnPropertyChanged(nameof(Hand));
    }

    /// Event notifikasi perubahan properti.
    public event PropertyChangedEventHandler? PropertyChanged;

    /// Memicu event PropertyChanged.
    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
