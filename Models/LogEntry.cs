namespace LoggerApp.Models;

using System.Text.Json;

public record LogEntry(
    Guid Id,
    DateTime DateTime,
    LogLevel Level,
    string Message
)
{
    public static string FormatTemplate { get; set; } =
        "{DateTime:yyyy-MM-dd HH:mm:ss.fff} - {Level} - {Message}";

    public static LogEntry Create(LogLevel level, string message) =>
        new(Guid.NewGuid(), DateTime.Now, level, message);

    public string ToJson() =>
        JsonSerializer.Serialize(this);

    public string ToFormattedString(string? template = null)
    {
        var result = template ?? FormatTemplate;

        result = result.Replace("{Id}", Id.ToString());
        result = result.Replace("{DateTime}", DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        result = result.Replace("{Level}", Level.ToString());
        result = result.Replace("{Message}", Message);

        return result;
    }

    public override string ToString() => ToFormattedString();
}