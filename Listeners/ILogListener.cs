namespace LoggerApp.Listeners;

using LoggerApp.Models;

public interface ILogListener
{
    Task StartAsync(CancellationToken cancellationToken);
    void Notify(LogEntry entry);
}