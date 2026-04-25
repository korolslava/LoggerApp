namespace LoggerApp.Models;

public record LogEntry(
    Guid Id,
    DateTime DateTime,
    LogLevel Level,
    string Message
)
{
    public static LogEntry Create(LogLevel level, string message) =>
        new(Guid.NewGuid(), DateTime.Now, level, message);

    public override string ToString() =>
        $"{DateTime:yyyy-MM-dd HH:mm:ss.fff} - {Level} - {Message}";
}