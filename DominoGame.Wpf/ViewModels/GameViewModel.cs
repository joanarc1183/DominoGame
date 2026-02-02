using DominoGame.Core;
using DominoGame.Wpf.Commands;

namespace DominoGame.Wpf.ViewModels;

public class GameViewModel
{
    public Board Board { get; }

    public RelayCommand<Cell> CellClickCommand { get; }

    public GameViewModel()
    {
        Board = new Board(8, 8);
        CellClickCommand = new RelayCommand<Cell>(OnCellClicked);
    }

    private void OnCellClicked(Cell cell)
    {
        Console.WriteLine($"Clicked cell: {cell.Row}, {cell.Col}");
    }
}
