using Silk.NET.SDL;
using Silk.NET.Maths;
using Minesweeper.Models;
using SkiaSharp;

namespace Minesweeper;

public unsafe class GameRenderer {
    private readonly Sdl _sdl;
    private readonly Renderer* _renderer;

    private const int CellSize = 40;
    private const int HeaderHeight = 70;
    private const int Padding = 20;
    private const int ButtonWidth = 320;
    private const int ButtonHeight = 60;

    public int WindowWidth  { get; private set; }
    public int WindowHeight { get; private set; }

    public GameRenderer(Sdl sdl, Renderer* renderer, int windowWidth, int windowHeight)
    {
        _sdl = sdl;
        _renderer = renderer;
        WindowWidth  = windowWidth;
        WindowHeight = windowHeight;
    }

    // Public entry pont
    public void Render(GameLogic game) {
        _sdl.SetRenderDrawColor(_renderer, 30, 30, 30, 255);
        _sdl.RenderClear(_renderer);

        switch (game.Screen) {
            case GameScreen.MainMenu: RenderMainMenu(game); break;
            case GameScreen.Playing:  RenderGame(game); break;
            case GameScreen.GameOver: RenderGame(game); RenderGameOver(game); break;
        }
        _sdl.RenderPresent(_renderer);
    }

    // The start main menu
    private void RenderMainMenu(GameLogic game) {
        RenderText("MINESWEEPER", WindowWidth / 2 - 140, 80, 48, new SKColor(255, 220, 50));
        RenderText("Select Difficulty", WindowWidth / 2 - 100, 150, 24, new SKColor(180, 180, 180));

        // best time display
        if (game.BestTime != null)
            RenderText($"Best: {game.BestTime.Value.TotalSeconds:F1}s", WindowWidth / 2 - 60, 190, 20, new SKColor(100, 220, 100));

        // buttons for choosing difficulty
        var levels = new[] { DifficultyLevel.Easy, DifficultyLevel.Medium, DifficultyLevel.Hard };
        var colors = new[] { new SKColor(50, 160, 80), new SKColor(200, 140, 30), new SKColor(180, 50, 50) };
        var labels = new[] { "Easy: 9x9 - 10 mines", "Medium: 16x16 - 40 mines", "Hard: 16x30 - 99 mines" };

        // drawing the buttons
        for (int i = 0; i < 3; i++) {
            int bx = WindowWidth / 2 - ButtonWidth / 2;
            int by = 250 + i * 90;
            DrawFilledRect(bx, by, ButtonWidth, ButtonHeight, colors[i].Red, colors[i].Green, colors[i].Blue);
            RenderText(labels[i], bx + 15, by + 18, 18, new SKColor(255, 255, 255));
        }

        RenderText("Click a button to start", WindowWidth / 2 - 110, 540, 18, new SKColor(120, 120, 120));
    }

    // render the effective game
    private void RenderGame(GameLogic game) {
        if (game.Board == null) return;
        DrawHeader(game);
        DrawBoard(game.Board);
    }

    // render the header
    // AI-generated
    private void DrawHeader(GameLogic game) {
        if (game.Board == null) return;

        // mines left
        int minesLeft = game.Board.MineCount - game.Board.FlagCount();
        DrawFilledRect(Padding, 10, 110, 44, 50, 50, 50);
        RenderText($"{minesLeft}", Padding + 8, 20, 22, new SKColor(220, 80, 80));

        // timer
        string time = $"{game.Elapsed.Minutes:D2}:{game.Elapsed.Seconds:D2}";
        int tw = MeasureText(time, 22);
        RenderText(time, WindowWidth / 2 - tw / 2, 20, 22, new SKColor(255, 220, 50));

        // best time
        if (game.BestTime != null) {
            string best = $"Best {game.BestTime.Value.TotalSeconds:F1}s";
            int bw = MeasureText(best, 18);
            RenderText(best, WindowWidth - Padding - bw, 22, 18, new SKColor(100, 220, 100));
        }

        // difficulty label
        string diff = DifficultySettings.FromLevel(game.Difficulty).Label;
        RenderText(diff, WindowWidth / 2 - 25, 50, 14, new SKColor(140, 140, 140));
    }

    // drawing the board
    private void DrawBoard(Board board) {
        int cellSize = GetCellSize(board);
        // Adaptive font size for difficulty levels -> not to overwrite the cell
        int fontSize = cellSize switch {
            28 => 14, // hard
            40 => 18, // medium
            _  => 22  // easy
        };

        for (int r = 0; r < board.Rows; r++) {
            for (int c = 0; c < board.Cols; c++) {
                var cell = board.Cells[r, c];
                int x = Padding + c * cellSize;
                int y = HeaderHeight + r * cellSize;
                int inner = cellSize - 2;

                if (!cell.IsRevealed) {
                    DrawFilledRect(x + 1, y + 1, inner, inner, 100, 100, 110);
                    if (cell.IsFlagged) {
                        // red square in center as flag
                        int sq = cellSize / 3;
                        DrawFilledRect(x + cellSize/2 - sq/2, y + cellSize/2 - sq/2, sq, sq, 220, 50, 50);
                    }
                }
                else if (cell.IsMine) {
                    DrawFilledRect(x + 1, y + 1, inner, inner, 180, 30, 30);
                    // dark square for mine
                    int sq = cellSize / 3;
                    DrawFilledRect(x + cellSize/2 - sq/2, y + cellSize/2 - sq/2, sq, sq, 20, 20, 20);
                }
                else {
                    DrawFilledRect(x + 1, y + 1, inner, inner, 60, 60, 65);
                    if (cell.NeighborMineCount > 0) {
                        // for different numbers -> different colors
                        var color = cell.NeighborMineCount switch {
                            1 => new SKColor(100, 149, 237),
                            2 => new SKColor(50,  205,  50),
                            3 => new SKColor(220,  80,  80),
                            4 => new SKColor(80,   80, 200),
                            5 => new SKColor(180,  50,  50),
                            6 => new SKColor(50,  180, 180),
                            7 => new SKColor(180,  50, 180),
                            _ => new SKColor(160, 160, 160)
                        };
                        RenderText(cell.NeighborMineCount.ToString(), x + cellSize/2 - fontSize/2, y + cellSize/2 - fontSize/2, fontSize, color);
                    }
                }
                DrawRectOutline(x, y, cellSize, cellSize, 20, 20, 20);
            }
        }
    }

    // Game over box
    private void RenderGameOver(GameLogic game) {
        // center dark box
        int boxW = 400, boxH = 260;
        int boxX = WindowWidth / 2 - boxW / 2;
        int boxY = WindowHeight / 2 - boxH / 2;

        // semi-transparent background — draw a dark filled rect
        DrawFilledRect(boxX, boxY, boxW, boxH, 20, 20, 20);
        DrawRectOutline(boxX, boxY, boxW, boxH, 100, 100, 100);

        if (game.State == GameState.Won) {
            RenderText("YOU WON!", boxX + 110, boxY + 30, 36, new SKColor(50, 220, 50));
            RenderText($"Time: {game.Elapsed.TotalSeconds:F1}s", boxX + 120, boxY + 90, 26, new SKColor(255, 220, 50));
            if (game.BestTime.HasValue && 
                Math.Abs(game.BestTime.Value.TotalSeconds - game.Elapsed.TotalSeconds) < 0.1)
                RenderText("New Best Time!", boxX + 100, boxY + 130, 22, new SKColor(100, 220, 100));
        } else { RenderText("GAME OVER", boxX + 110, boxY + 30, 36, new SKColor(220, 50, 50)); }
        // Options for return
        RenderText("R - Restart", boxX + 40,  boxY + 190, 18, new SKColor(180, 180, 180));
        RenderText("M - Menu",    boxX + 160, boxY + 190, 18, new SKColor(180, 180, 180));
        RenderText("Esc - Quit",  boxX + 270, boxY + 190, 18, new SKColor(180, 180, 180));
    }

    // function for rendering the text
    // AI-generated
    private void RenderText(string text, int x, int y, int fontSize, SKColor color) {
        using var typeface = SKTypeface.Default;
        using var font = new SKFont(typeface, fontSize);
        using var paint = new SKPaint {Color = color, IsAntialias = true,};

        font.MeasureText(text, out var bounds, paint);
        int w = Math.Max((int)bounds.Width + 10, 1);
        int h = Math.Max(fontSize + 10, 1);

        using var bitmap = new SKBitmap(w, h);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);
        canvas.DrawText(text, 0, fontSize, SKTextAlign.Left, font, paint);

        var pixels = bitmap.Pixels;
        var surface = _sdl.CreateRGBSurface(0, w, h, 32, 0x00FF0000, 0x0000FF00, 0x000000FF, 0xFF000000);

        if (surface == null) return;

        unsafe {
            for (int py = 0; py < h; py++)
                for (int px = 0; px < w; px++) {
                    var p = pixels[py * w + px];
                    ((uint*)surface->Pixels)[py * w + px] =
                        ((uint)p.Alpha << 24) | ((uint)p.Red << 16) |
                        ((uint)p.Green << 8) | p.Blue;
                }
        }

        var texture = _sdl.CreateTextureFromSurface(_renderer, surface);
        _sdl.FreeSurface(surface);
        if (texture == null) return;
        _sdl.SetTextureBlendMode(texture, BlendMode.Blend);
        var src = new Rectangle<int>(0, 0, w, h);
        var dst = new Rectangle<int>(x, y, w, h);
        _sdl.RenderCopy(_renderer, texture, ref src, ref dst);
        _sdl.DestroyTexture(texture);
    }

    private int MeasureText(string text, int fontSize) => text.Length * (fontSize / 2);

    // helper functions
    // to draw a rectangle box
    // AI-generated
    private void DrawFilledRect(int x, int y, int w, int h, byte r, byte g, byte b) {
        _sdl.SetRenderDrawColor(_renderer, r, g, b, 255);
        var rect = new Rectangle<int>(x, y, w, h);
        _sdl.RenderFillRect(_renderer, ref rect);
    }
// outline
    private void DrawRectOutline(int x, int y, int w, int h, byte r, byte g, byte b) {
        _sdl.SetRenderDrawColor(_renderer, r, g, b, 255);
        var rect = new Rectangle<int>(x, y, w, h);
        _sdl.RenderDrawRect(_renderer, ref rect);
    }

    // checks which difficulty button was clicked, returns null if none
    public DifficultyLevel? GetMenuButtonClick(int mouseX, int mouseY) {
        var levels = new[] { DifficultyLevel.Easy, DifficultyLevel.Medium, DifficultyLevel.Hard };
        for (int i = 0; i < 3; i++) {
            int bx = WindowWidth / 2 - ButtonWidth / 2; // centered horizontally
            int by = 250 + i * 90; // each button 90px lower

            // is the click inside this button?
            if (mouseX >= bx && mouseX <= bx + ButtonWidth && mouseY >= by && mouseY <= by + ButtonHeight)
                return levels[i];
        }
        return null;
    }

    // converts mouse pixel position to grid row/col
    // AI-generated
    public (int row, int col) GetCellFromMouse(Board board, int mouseX, int mouseY) {
        int cellSize = GetCellSize(board);
        int col = (mouseX - Padding) / cellSize;
        int row = (mouseY - HeaderHeight) / cellSize;
        return (row, col);
    }

    // returns true if the mouse is actually over a cell
    // AI-generated
    public bool IsInsideBoard(Board board, int mouseX, int mouseY) {
        int cellSize = GetCellSize(board);
        int col = (mouseX - Padding) / cellSize;
        int row = (mouseY - HeaderHeight) / cellSize;
        return row >= 0 && row < board.Rows && col >= 0 && col < board.Cols;
    }

    // smaller cells for bigger boards so everything fits
    private int GetCellSize(Board board) {
        return board.Cols switch {
            30 => 28,  // Hard
            16 => 40,  // Medium
            _  => 50   // Easy
        };
    }
}