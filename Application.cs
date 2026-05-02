namespace LoggerApp;

using LoggerApp.Core;
using LoggerApp.Listeners;
using LoggerApp.Services;

public class Application
{
    private const string SaveFilePath = "../../../Data/logs_save.json";
    private const string StreamFilePath = "../../../Data/logs_stream.txt";
    private const int TotalLogs = 2_000_000;
    private const int GeneratorThreads = 8;

    public async Task RunAsync()
    {
        using var cts = new CancellationTokenSource();

        var fileListener = new FileLogListener(StreamFilePath);
        var consoleListener = new ConsoleLogListener();
        var listeners = new List<ILogListener> { fileListener, consoleListener };

        var logger = new Logger(listeners);

        await logger.StartAsync(cts.Token);

        var generator = new LogGeneratorService(logger);
        var menu = new LogMenuService(logger, consoleListener);

        var fileListenerTask = fileListener.StartAsync(cts.Token);

        var generationTask = generator.GenerateAsync(TotalLogs, GeneratorThreads, cts.Token);

        await menu.RunAsync();

        cts.Cancel();

        try
        {
            await generationTask;
        }
        catch (OperationCanceledException)
        {
            // Generation was cancelled when user quit the app.
        }

        await logger.StopAsync();

        fileListener.Complete();

        try
        {
            await fileListenerTask;
        }
        catch (OperationCanceledException)
        {
            // File listener was cancelled during shutdown.
        }

        Console.WriteLine("All done. Goodbye!");
    }
}