namespace LoggerApp.Listeners;

using System.Threading.Channels;
using LoggerApp.Models;

public class FileLogListener : ILogListener
{
    private const int FlushInterval = 5000;
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

    public void Notify(LogEntry entry) => _channel.Writer.TryWrite(entry);
    public void Complete() => _channel.Writer.Complete();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var writer = new StreamWriter(_filePath, append: true);
        var count = 0;

        await foreach (var entry in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            await writer.WriteLineAsync(entry.ToString());
            count++;

            if (count % FlushInterval == 0)
            {
                await writer.FlushAsync(cancellationToken);
            }
        }

        await writer.FlushAsync(cancellationToken);
    }
}