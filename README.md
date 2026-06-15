# 🚩 Minesweeper

A classic Minesweeper game built in C# with .NET 10, using Silk.NET for SDL2 windowed rendering and SkiaSharp for text.

## How to play

- **Left click** = reveal a cell
- **Right click** = place/remove a flag
- **Double click** = chord reveal (if enough flags around a number)
- **R** = restart, **M** = back to menu, **Esc** = quit

## Difficulty levels

| Level  | Board   | Mines |
|--------|---------|-------|
| Easy   | 9×9     | 10    |
| Medium | 16×16   | 40    |
| Hard   | 16×30   | 99    |

## Build & run

```bash
dotnet run
```

Requires .NET 10 SDK. Clone and run from the project root.

## Notes

- Best time is saved to `highscore.json` between sessions.
- First click is always safe - mines are placed after the first reveal.
