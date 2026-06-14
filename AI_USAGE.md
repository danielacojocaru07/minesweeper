# AI Usage Disclosure

## Tools used
- Claude Sonnet 4.6 -- chat-based code suggestions and debugging help

## How I used it
Used Claude to help debug SDL rendering issues, suggest the SkiaSharp text rendering approach, and fix C# compiler errors. So, most of the game logic, structure decisions, base classes, and code organization were done by me.

## AI-generated blocks

The following regions were largely AI-suggested and are marked in code with `// AI-generated`:

- `GameRender.cs` -- the `RenderText`, `IsInsideBoard`, and `GetCellFromMouse` method (SkiaSharp bitmap -> SDL texture). Note: I had never used SkiaSharp before. Needed help understanding how to render text -> bitmap and upload it as SDL texture. Also, needed a foundation for the UX elements. 

- `GameRender.cs` -- the `DrawHeader` and `DrawFilledRect` methods. 
  Note: Positioning the info from the header (timer, # of mines left, best time) and drawing an actual box for texts, cells, etc.

- `Models/Board.cs` -- `PlaceMines` and `CalculateNeighbors`. 
  Note: The first click is safe thing (placing the mines only after the first reveal). It was mandatory for the game logic.

- `Models/Board.cs` -- the flood-fill logic inside `Reveal`. 
  Note: Revealing the empty neighbors.

- `Program.cs` -- Note: The double-click detection. Using time between clicks and comparing row/col to detect a chord reveal. Something optional but which i wanted to have in my game -- like in the classical Minesweeper.

- `GameLogic.cs` -- `HandleDoubleClick`. Note: The chord reveal logic (counting flags around a cell and revealing neighbors if they match).

## Files fully written by me
- `Models/Cell.cs`
- `Models/GameState.cs`
- `Models/GameScreen.cs`
- `Models/Difficulty.cs`
- `Exceptions/InvalidMoveException.cs`
- `Services/HighScoreService.cs`