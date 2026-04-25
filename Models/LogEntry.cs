namespace LoggerApp.Models;

public record LogEntry(
    DateTime DateTime,
    LogLevel Level,
    string Message
)
{
    public override string ToString() =>
        $"{DateTime:yyyy-MM-dd HH:mm:ss.fff} - {Level} - {Message}";
}