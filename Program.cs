using System.Diagnostics;
using Silk.NET.SDL;
using Minesweeper;
using Minesweeper.Models;

var sdl = Sdl.GetApi();
var sdlInitResult = sdl.Init(Sdl.InitVideo | Sdl.InitEvents | Sdl.InitTimer);
if (sdlInitResult < 0)
    throw new InvalidOperationException("Failed to initialize SDL.");

const int WinW = 800;
const int WinH = 700;
DateTime _lastClickTime = DateTime.MinValue;
int _lastClickRow = -1, _lastClickCol = -1;
IntPtr window;

unsafe {
    window = (IntPtr)sdl.CreateWindow(
        "Minesweeper", Sdl.WindowposUndefined, Sdl.WindowposUndefined, WinW, WinH,
        (uint)WindowFlags.Resizable | (uint)WindowFlags.AllowHighdpi);
    if (window == IntPtr.Zero) throw new Exception("Failed to create window.");
}

IntPtr renderer;
unsafe {
    renderer = (IntPtr)sdl.CreateRenderer((Window*)window, -1, (uint)RendererFlags.Accelerated);
    sdl.RenderSetVSync((Renderer*)renderer, 1);
    if (renderer == IntPtr.Zero) throw new Exception("Failed to create renderer.");
}

using var game = new GameLogic();
GameRenderer gameRenderer;
unsafe {gameRenderer = new GameRenderer(sdl, (Renderer*)renderer, WinW, WinH);}
var ev = new Event();
bool quit = false;

while (!quit) {
    while (sdl.PollEvent(ref ev) != 0) {
        switch (ev.Type) {
            case (uint)EventType.Quit:
                quit = true;
                break;
            case (uint)EventType.Mousebuttondown: {
                int mx = ev.Button.X;
                int my = ev.Button.Y;

                switch (game.Screen) {
                    case GameScreen.MainMenu:
                        var level = gameRenderer.GetMenuButtonClick(mx, my);
                        if (level != null) game.StartGame(level.Value);
                        break;
                    case GameScreen.Playing:
                        if (game.Board == null) break;
                        if (!gameRenderer.IsInsideBoard(game.Board, mx, my)) break;
                        var (row, col) = gameRenderer.GetCellFromMouse(game.Board, mx, my);
                        // AI-generated
                        if (ev.Button.Button == 1) {
                            // detect double click (within 300ms)
                            var now = DateTime.Now;
                            if ((now - _lastClickTime).TotalMilliseconds < 300 && _lastClickRow == row && _lastClickCol == col)
                                game.HandleDoubleClick(row, col);
                            else
                                game.HandleLeftClick(row, col);
                            _lastClickTime = now;
                            _lastClickRow  = row;
                            _lastClickCol  = col;
                        }
                        else if (ev.Button.Button == 3) game.HandleRightClick(row, col);
                        break;
                }
                break;
            }
            case (uint)EventType.Keydown: {
                int sc = (int)ev.Key.Keysym.Scancode;
                if (sc == 21) game.Restart(); // R = restart
                if (sc == 16) game.GoToMenu(); // M = go to menu
                if (sc == 41) quit = true; // Escape = quit
                break;
            }
        }
    }
    game.Update();
    unsafe {gameRenderer.Render(game);}
}

unsafe{ sdl.DestroyWindow((Window*)window);}
sdl.Quit();