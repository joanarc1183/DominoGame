using System.ComponentModel;
using DominoGame.Core;

namespace DominoGame.Wpf.ViewModels;

public class DominoTileViewModel : INotifyPropertyChanged
{
    // Menandakan apakah tile sedang dipilih di UI.
    private bool _isSelected;
    // Menandakan apakah tile bisa dimainkan pada giliran ini.
    private bool _isPlayable;

    /// Model domino yang dibungkus oleh view model ini.
    public Domino Domino { get; }
    /// Nilai pip sisi kiri untuk keperluan binding.
    public Dot Left => Domino.LeftPip;
    /// Nilai pip sisi kanan untuk keperluan binding.
    public Dot Right => Domino.RightPip;

    /// Membuat view model dari model domino.
    public DominoTileViewModel(Domino domino) => Domino = domino;

    /// Koleksi titik (pip) kiri dalam bentuk boolean untuk di-render sebagai ellipse.
    public List<bool> LeftPips => GeneratePips(Domino.LeftPip);
    /// Koleksi titik (pip) kanan dalam bentuk boolean untuk di-render sebagai ellipse.
    public List<bool> RightPips => GeneratePips(Domino.RightPip);

    /// Status apakah tile sedang dipilih.
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

    /// Status apakah tile bisa dimainkan.
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

    /// Menghasilkan jumlah pip dalam bentuk list boolean.
    private static List<bool> GeneratePips(Dot dot)
    {
        int n = (int)dot;
        return Enumerable.Range(0, n).Select(_ => true).ToList();
    }

    /// Representasi teks sederhana untuk status/log.
    public override string ToString() => $"{Left}|{Right}";

    /// Event notifikasi perubahan properti.
    public event PropertyChangedEventHandler? PropertyChanged;

    /// Memicu event PropertyChanged.
    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
