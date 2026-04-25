namespace LoggerApp.Listeners;

using System.Threading.Channels;
using LoggerApp.Models;

public class FileLogListener : ILogListener
{
    private readonly string _filePath;
    private readonly Channel<LogEntry> _channel;

    public FileLogListener(string filePath)
    {
        _filePath = filePath;
        _channel = Channel.CreateUnbounded<LogEntry>(new UnboundedChannelOptions
        {
            SingleReader = true
        });
    }

    public void Notify(LogEntry entry)
    {
        _channel.Writer.TryWrite(entry);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var writer = new StreamWriter(_filePath, append: true);

        await foreach (var entry in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            await writer.WriteLineAsync(entry.ToString());
            await writer.FlushAsync(cancellationToken);
        }
    }

    public void Complete() => _channel.Writer.Complete();
}