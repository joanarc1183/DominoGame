namespace DominoGame.Core;

public class Board
{
    public int Rows { get; }
    public int Cols { get; }

    public List<Cell> Cells { get; } = new();

    public Board(int rows, int cols)
    {
        Rows = rows;
        Cols = cols;

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                Cells.Add(new Cell(r, c));
    }
}
