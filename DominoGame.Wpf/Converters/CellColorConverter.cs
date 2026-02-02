using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DominoGame.Wpf.Converters;

public class CellColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int row && parameter is int col)
            return (row + col) % 2 == 0 ? Brushes.Bisque : Brushes.SaddleBrown;

        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
