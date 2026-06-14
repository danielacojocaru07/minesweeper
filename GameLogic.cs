using Minesweeper.Models;
using Minesweeper.Services;
using Minesweeper.Exceptions;

namespace Minesweeper;

public class GameLogic : IDisposable {
    public Board? Board { get; private set; }
    public GameState State { get; private set; }
    public GameScreen Screen { get; private set; }
    public TimeSpan Elapsed { get; private set; }
    public TimeSpan? BestTime { get; private set; }
    public DifficultyLevel Difficulty { get; private set; }

    private readonly HighScoreService _highScoreService;
    private DateTime _startTime;
    private bool _timerStarted = false;
    private bool _disposed = false;

    public GameLogic() {
        _highScoreService = new HighScoreService();
        BestTime = _highScoreService.Load();
        Screen = GameScreen.MainMenu;
        State = GameState.Playing;
        Difficulty = DifficultyLevel.Easy;
    }

    public void StartGame(DifficultyLevel level) {
        Difficulty = level;
        var settings = DifficultySettings.FromLevel(level);
        Board = new Board(settings.Rows, settings.Cols, settings.Mines);
        State = GameState.Playing;
        Screen = GameScreen.Playing;
        _timerStarted = false;
        Elapsed = TimeSpan.Zero;
    }

    public void Restart() {
        if (Board != null)
            StartGame(Difficulty);
    }

    public void GoToMenu() {
        Screen = GameScreen.MainMenu;
        Board = null;
    }

    public void Update() {
        if (Screen == GameScreen.Playing && State == GameState.Playing && _timerStarted)
            Elapsed = DateTime.Now - _startTime;
    }

    public void HandleLeftClick(int row, int col) {
        if (State != GameState.Playing || Board == null) return;
        if (!_timerStarted) {
            _timerStarted = true;
            _startTime = DateTime.Now;
        }

        bool hitMine = Board.Reveal(row, col);
        // bool hitMine;
        // try { hitMine = Board.Reveal(row, col);} 
        // // cell already revealed, ignore the click
        // catch (InvalidMoveException) { return; }

        if (hitMine) {
            Board.RevealAllMines();
            State = GameState.Lost;
            Screen = GameScreen.GameOver;
        } else if (Board.CheckWin()) {
            State = GameState.Won;
            Screen = GameScreen.GameOver;
            Elapsed = DateTime.Now - _startTime;

            if (BestTime == null || Elapsed < BestTime) {
                BestTime = Elapsed;
                _highScoreService.Save(Elapsed);
            }
        }
    }

    public void HandleRightClick(int row, int col) {
        if (State != GameState.Playing || Board == null) return;
        Board.ToggleFlag(row, col);
    }

    public void Dispose() {
        if (!_disposed) {
            _highScoreService.Dispose();
            _disposed = true;
        }
    }

// AI-generated
    public void HandleDoubleClick(int row, int col) {
        if (State != GameState.Playing || Board == null) return;
        var cell = Board.Cells[row, col];
        if (!cell.IsRevealed || cell.NeighborMineCount == 0) return;

        // count flags around this cell
        int flagsAround = Board.GetNeighborCoords(row, col).Count(pos => Board.Cells[pos.row, pos.col].IsFlagged);

        // only chord if flags match the number
        if (flagsAround != cell.NeighborMineCount) return;

        // reveal all non-flagged neighbors
        foreach (var (nr, nc) in Board.GetNeighborCoords(row, col)) {
            var neighbor = Board.Cells[nr, nc];
            if (!neighbor.IsFlagged && !neighbor.IsRevealed) {
                bool hitMine = Board.Reveal(nr, nc);
                if (hitMine)
                {
                    Board.RevealAllMines();
                    State = GameState.Lost;
                    Screen = GameScreen.GameOver;
                    return;
                }
            }
        }

        if (Board.CheckWin()) {
            State = GameState.Won;
            Screen = GameScreen.GameOver;
            Elapsed = DateTime.Now - _startTime;
            if (BestTime == null || Elapsed < BestTime) {
                BestTime = Elapsed;
                _highScoreService.Save(Elapsed);
            }
        }
    }
}