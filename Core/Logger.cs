namespace LoggerApp.Core;

using System.Collections.Concurrent;
using LoggerApp.Listeners;
using LoggerApp.Models;

public class Logger : ILogger
{
    private readonly List<LogEntry> _logs = [];
    private readonly Lock _lock = new();
    private readonly List<ILogListener> _listeners;
    private LogLevel? _filterLevel = null;

    public Logger(List<ILogListener> listeners)
    {
        _listeners = listeners;
    }

    public void AddLog(LogLevel level, string message)
    {
        var entry = LogEntry.Create(level, message);

        lock (_lock)
        {
            _logs.Add(entry);
        }

        foreach (var listener in _listeners)
        {
            listener.Notify(entry);
        }
    }

    public void DeleteLog(Guid id)
    {
        lock (_lock)
        {
            var entry = _logs.FirstOrDefault(l => l.Id == id);
            if (entry is null)
            {
                Console.WriteLine("Log not found.");
                return;
            }
            _logs.Remove(entry);
        }
    }

    public void SetFilterLevel(LogLevel? level)
    {
        _filterLevel = level;
        Console.WriteLine($"Filter set to: {(level.HasValue ? level.ToString() : "All")}");
    }

    public IReadOnlyList<LogEntry> GetLogs(LogLevel? filterLevel = null)
    {
        var level = filterLevel ?? _filterLevel;

        lock (_lock)
        {
            return level.HasValue
                ? _logs.Where(l => l.Level >= level.Value).ToList()
                : _logs.ToList();
        }
    }

    public async Task SaveToFileAsync(string filePath)
    {
        List<LogEntry> snapshot;

        lock (_lock)
        {
            snapshot = [.. _logs];
        }

        await using var writer = new StreamWriter(filePath, append: false);

        foreach (var entry in snapshot)
        {
            await writer.WriteLineAsync(entry.ToString());
        }

        Console.WriteLine($"Saved {snapshot.Count} logs to {filePath}");
    }

    public async Task LoadFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found.");
            return;
        }

        var lines = await File.ReadAllLinesAsync(filePath);
        var loaded = new List<LogEntry>();

        foreach (var line in lines)
        {
            var entry = TryParseLine(line);
            if (entry is not null) loaded.Add(entry);
        }

        lock (_lock)
        {
            _logs.Clear();
            _logs.AddRange(loaded);
        }

        Console.WriteLine($"Loaded {loaded.Count} logs from {filePath}");
    }

    private static LogEntry? TryParseLine(string line)
    {
        var parts = line.Split(" - ", 3);
        if (parts.Length != 3) return null;

        if (!DateTime.TryParse(parts[0], out var dt)) return null;
        if (!Enum.TryParse<LogLevel>(parts[1], out var level)) return null;

        return new LogEntry(Guid.NewGuid(), dt, level, parts[2]);
    }
}