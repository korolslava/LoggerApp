namespace LoggerApp.Core;

using System.Text.Json;
using System.Threading.Channels;
using LoggerApp.Listeners;
using LoggerApp.Models;

public class Logger : ILogger
{
    private readonly List<LogEntry> _logs = [];
    private readonly Lock _lock = new();
    private readonly List<ILogListener> _listeners;

    private readonly Channel<LogEntry> _channel;
    private Task? _dispatcherTask;

    private LogLevel? _filterLevel = null;

    public Logger(List<ILogListener> listeners)
    {
        _listeners = listeners;

        _channel = Channel.CreateUnbounded<LogEntry>(new UnboundedChannelOptions
        {
            SingleReader = true
        });
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_dispatcherTask is not null)
        {
            return Task.CompletedTask;
        }

        _dispatcherTask = Task.Run(ProcessQueueAsync, cancellationToken);

        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        _channel.Writer.TryComplete();

        if (_dispatcherTask is not null)
        {
            await _dispatcherTask;
        }
    }

    public void AddLog(LogLevel level, string message)
    {
        var entry = LogEntry.Create(level, message);

        _channel.Writer.TryWrite(entry);
    }

    private async Task ProcessQueueAsync()
    {
        await foreach (var entry in _channel.Reader.ReadAllAsync())
        {
            lock (_lock)
            {
                _logs.Add(entry);
            }

            foreach (var listener in _listeners)
            {
                listener.Notify(entry);
            }
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

    public IReadOnlyList<LogEntry> SearchLogs(
        string? keyword,
        DateTime? from = null,
        DateTime? to = null,
        LogLevel? minLevel = null
    )
    {
        lock (_lock)
        {
            IEnumerable<LogEntry> query = _logs;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(l =>
                    l.Message.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            if (from.HasValue)
            {
                query = query.Where(l => l.DateTime >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(l => l.DateTime <= to.Value);
            }

            if (minLevel.HasValue)
            {
                query = query.Where(l => l.Level >= minLevel.Value);
            }

            return query.ToList();
        }
    }

    public async Task SaveToFileAsync(string filePath)
    {
        List<LogEntry> snapshot;

        lock (_lock)
        {
            snapshot = [.. _logs];
        }

        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        if (extension == ".json")
        {
            var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(filePath, json);
            Console.WriteLine($"Saved {snapshot.Count} logs to {filePath} as JSON");
            return;
        }

        await using var writer = new StreamWriter(filePath, append: false);

        foreach (var entry in snapshot)
        {
            await writer.WriteLineAsync(entry.ToFormattedString());
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

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var loaded = new List<LogEntry>();

        if (extension == ".json")
        {
            var json = await File.ReadAllTextAsync(filePath);

            var entries = JsonSerializer.Deserialize<List<LogEntry>>(json);

            if (entries is not null)
            {
                loaded.AddRange(entries);
            }
        }
        else
        {
            var lines = await File.ReadAllLinesAsync(filePath);

            foreach (var line in lines)
            {
                var entry = TryParseLine(line);

                if (entry is not null)
                {
                    loaded.Add(entry);
                }
            }
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