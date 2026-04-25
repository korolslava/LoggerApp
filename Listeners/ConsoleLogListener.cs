namespace LoggerApp.Listeners;

using LoggerApp.Models;

public class ConsoleLogListener : ILogListener
{
    private static readonly Dictionary<LogLevel, ConsoleColor> Colors = new()
    {
        { LogLevel.Debug,   ConsoleColor.Gray },
        { LogLevel.Info,    ConsoleColor.Cyan },
        { LogLevel.Warning, ConsoleColor.Yellow },
        { LogLevel.Error,   ConsoleColor.Red }
    };

    private bool _isEnabled = false;

    public void Toggle() => _isEnabled = !_isEnabled;
    public bool IsEnabled => _isEnabled;

    public void Notify(LogEntry entry)
    {
        if (!_isEnabled) return;

        Console.ForegroundColor = Colors[entry.Level];
        Console.WriteLine(entry.ToString());
        Console.ResetColor();
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}