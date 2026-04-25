namespace LoggerApp;

using LoggerApp.Core;
using LoggerApp.Listeners;
using LoggerApp.Services;

public class Application
{
    private const string LogFilePath = "logs.txt";
    private const int TotalLogs = 2_000_000;
    private const int GeneratorThreads = 8;

    public async Task RunAsync()
    {
        using var cts = new CancellationTokenSource();

        var fileListener = new FileLogListener(LogFilePath);
        var consoleListener = new ConsoleLogListener();
        var listeners = new List<ILogListener> { fileListener, consoleListener };

        var logger = new Logger(listeners);

        var generator = new LogGeneratorService(logger);
        var menu = new LogMenuService(logger, consoleListener);

        var fileListenerTask = fileListener.StartAsync(cts.Token);

        var generationTask = generator.GenerateAsync(TotalLogs, GeneratorThreads, cts.Token);

        await menu.RunAsync();

        await generationTask;
        fileListener.Complete();
        await fileListenerTask;

        Console.WriteLine("All done. Goodbye!");
    }
}