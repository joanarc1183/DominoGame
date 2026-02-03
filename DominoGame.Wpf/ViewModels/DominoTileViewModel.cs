using System.ComponentModel;
using DominoGame.Core;

namespace DominoGame.Wpf.ViewModels;

public class DominoTileViewModel : INotifyPropertyChanged
{
    private bool _isSelected;
    private bool _isPlayable;

    public Domino Domino { get; }
    public Dot Left => Domino.LeftPip;
    public Dot Right => Domino.RightPip;

    public DominoTileViewModel(Domino domino) => Domino = domino;

    public List<bool> LeftPips => GeneratePips(Domino.LeftPip);
    public List<bool> RightPips => GeneratePips(Domino.RightPip);

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged(nameof(IsSelected));
        }
    }

    public bool IsPlayable
    {
        get => _isPlayable;
        set
        {
            if (_isPlayable == value) return;
            _isPlayable = value;
            OnPropertyChanged(nameof(IsPlayable));
        }
    }

    private static List<bool> GeneratePips(Dot dot)
    {
        int n = (int)dot;
        return Enumerable.Range(0, n).Select(_ => true).ToList();
    }

    public override string ToString() => $"{Left}|{Right}";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
