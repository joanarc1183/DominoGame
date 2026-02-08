using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DominoGame.Core;

namespace DominoGame.Wpf.Converters;

public class PipColorConverter : IValueConverter
{
    // Mengubah nilai pip (Dot/int) menjadi warna sesuai aturan.
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int pip = value switch
        {
            Dot dot => (int)dot,
            int i => i,
            _ => 0
        };

        return pip switch
        {
            1 => Brushes.Red,
            2 => new SolidColorBrush(Color.FromRgb(0xE0, 0x7A, 0x00)), // deeper orange
            3 => new SolidColorBrush(Color.FromRgb(0xE3, 0xB9, 0x00)), // brighter deep yellow
            4 => new SolidColorBrush(Color.FromRgb(0x1E, 0x8E, 0x3E)), // deep green
            5 => new SolidColorBrush(Color.FromRgb(0x1E, 0x5A, 0xB6)), // deep blue
            6 => Brushes.Purple,
            _ => Brushes.LightGray
        };
    }

    // ConvertBack tidak dipakai untuk warna UI.
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
