using System.Text.Json;
namespace Minesweeper.Services;

public class HighScoreService : IDisposable {
    private const string FilePath = "highscore.json";
    private bool _disposed = false;

    public void Save(TimeSpan time) {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(time.TotalSeconds));
    }

    public TimeSpan? Load() {
        if (!File.Exists(FilePath)) return null;
        var seconds = JsonSerializer.Deserialize<double>(File.ReadAllText(FilePath));
        return TimeSpan.FromSeconds(seconds);
    }

    public void Dispose(){
        if (!_disposed) _disposed = true;
    }
}