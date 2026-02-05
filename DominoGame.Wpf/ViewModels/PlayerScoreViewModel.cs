using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DominoGame.Core;

namespace DominoGame.Wpf.ViewModels;

public class PlayerScoreViewModel : INotifyPropertyChanged
{
    private readonly Player _player;
    private bool _isCurrent;

    public PlayerScoreViewModel(Player player)
    {
        _player = player;
    }

    public Player Player => _player;
    public string Name => _player.Name;
    public int Score => _player.Score;
    public ObservableCollection<DominoTileViewModel> Hand { get; } = new();

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

    public void Refresh()
    {
        OnPropertyChanged(nameof(Score));
    }

    public void RefreshHand(IEnumerable<Domino> dominoes)
    {
        Hand.Clear();
        foreach (var domino in dominoes)
            Hand.Add(new DominoTileViewModel(domino));

        OnPropertyChanged(nameof(Hand));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
