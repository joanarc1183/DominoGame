using System.ComponentModel;
using DominoGame.Core;

namespace DominoGame.Wpf.ViewModels;

public class PlayerScoreViewModel : INotifyPropertyChanged
{
    private readonly Player _player;

    public PlayerScoreViewModel(Player player)
    {
        _player = player;
    }

    public string Name => _player.Name;
    public int Score => _player.Score;

    public void Refresh()
    {
        OnPropertyChanged(nameof(Score));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
