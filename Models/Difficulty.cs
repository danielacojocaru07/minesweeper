namespace Minesweeper.Models;

public enum DifficultyLevel {
    Easy,
    Medium,
    Hard
}

public record DifficultySettings(int Rows, int Cols, int Mines, string Label) {
    public static DifficultySettings FromLevel(DifficultyLevel level) => level switch {
        DifficultyLevel.Easy   => new(9,  9,  10, "Easy"),
        DifficultyLevel.Medium => new(16, 16, 40, "Medium"),
        DifficultyLevel.Hard   => new(16, 30, 99, "Hard"),
        _ => throw new ArgumentOutOfRangeException(nameof(level))
    };
}