using System.Collections.ObjectModel;
using DominoGame.Wpf.Models;
using DominoGame.Core;
namespace DominoGame.Wpf
{
    public class MainViewModel
    {
        public ObservableCollection<DominoViewModel> Dominoes { get; }
            = new ObservableCollection<DominoViewModel>
            {
                new DominoViewModel { Left = 6, Right = 6 },
                new DominoViewModel { Left = 6, Right = 5 },
                new DominoViewModel { Left = 5, Right = 3 },
                new DominoViewModel { Left = 2, Right = 1 }
            };
    }
}
