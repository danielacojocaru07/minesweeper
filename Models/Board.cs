using Minesweeper.Exceptions;
namespace Minesweeper.Models;

public class Board {
    public int Rows {get;}
    public int Cols {get;}
    public Cell[,] Cells {get;}
    public int MineCount {get; private set;}
    private bool _firstMove = true;

    public Board(int rows, int cols, int mineCount) {
        Rows = rows;
        Cols = cols;
        MineCount = mineCount;
        Cells = new Cell[rows, cols];

        // Initialization of all cells of the board
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                Cells[r, c] = new Cell();
    }

    // AI-generated
    private void PlaceMines(int safeRow, int safeCol) {
        var rng = new Random();
        int placed = 0;
        while (placed < MineCount) {
            int r = rng.Next(Rows);
            int c = rng.Next(Cols);
            if (!Cells[r, c].IsMine && !(r == safeRow && c == safeCol)) {
                Cells[r, c].IsMine = true;
                placed++;
            }
        }
        CalculateNeighbors();
    }

    // AI-generated
    private void CalculateNeighbors() {
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                if (!Cells[r, c].IsMine)
                    Cells[r, c].NeighborMineCount = GetNeighbors(r, c).Count(n => n.IsMine);
    }

    public IEnumerable<(int row, int col)> GetNeighborCoords(int row, int col) {
        for (int dr = -1; dr <= 1; dr++)
            for (int dc = -1; dc <= 1; dc++) {
                if (dr == 0 && dc == 0) continue;
                int nr = row + dr, nc = col + dc;
                if (nr >= 0 && nr < Rows && nc >= 0 && nc < Cols)
                    yield return (nr, nc);
            }
    }

    public IEnumerable<Cell> GetNeighbors(int row, int col) =>
        GetNeighborCoords(row, col).Select(pos => Cells[pos.row, pos.col]);

    public bool Reveal(int row, int col) {
        var cell = Cells[row, col];
        if (cell.IsFlagged || cell.IsRevealed) return false;
        // if (cell.IsFlagged) return false;
        // if (cell.IsRevealed)
        //     throw new InvalidMoveException($"Cell ({row},{col}) is already revealed.");

        // AI-generated
        if (_firstMove) {
            _firstMove = false;
            PlaceMines(row, col);
        }

        cell.IsRevealed = true;
        if (cell.IsMine) return true; // hit a mine

        if (cell.NeighborMineCount == 0)
            foreach (var (nr, nc) in GetNeighborCoords(row, col))
                Reveal(nr, nc);

        return false;
    }

    public void ToggleFlag(int row, int col) {
        var cell = Cells[row, col];
        if (!cell.IsRevealed)
            cell.IsFlagged = !cell.IsFlagged;
    }

    public bool CheckWin() =>
        Cells.Cast<Cell>().All(c => c.IsMine || c.IsRevealed);

    public void RevealAllMines() {
        foreach (var cell in Cells.Cast<Cell>().Where(c => c.IsMine))
            cell.IsRevealed = true;
    }

    public int FlagCount() =>
        Cells.Cast<Cell>().Count(c => c.IsFlagged);
}