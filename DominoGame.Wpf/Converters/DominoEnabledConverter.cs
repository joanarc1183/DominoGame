using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using DominoGame.Wpf.Models;
using System.Collections.ObjectModel;

namespace DominoGame.Wpf.Converters
{
    public class DominoEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<DominoTileViewModel> playable && parameter is DominoTileViewModel tile)
                return playable.Contains(tile);
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
