namespace LoggerApp.Services;

using LoggerApp.Core;
using LoggerApp.Models;

public class LogGeneratorService(ILogger logger)
{
    private static readonly LogLevel[] Levels =
        [LogLevel.Debug, LogLevel.Info, LogLevel.Warning, LogLevel.Error];

    private static readonly string[] Messages =
    [
        "Application started successfully",
        "Connection established",
        "Request received from client",
        "Processing data chunk",
        "Cache miss, fetching from DB",
        "Query executed in {0}ms",
        "Retrying operation attempt {0}",
        "User {0} logged in",
        "File {0} not found",
        "Timeout occurred on thread {0}",
        "Memory usage at {0}%",
        "Disk write completed {0} bytes",
        "Unexpected null reference at step {0}",
        "Config loaded from environment",
        "Shutting down service {0}"
    ];

    public async Task GenerateAsync(int totalLogs, int threadCount, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Starting generation of {totalLogs:N0} logs across {threadCount} threads...");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var logsPerThread = totalLogs / threadCount;
        var random = new Random();

        var tasks = Enumerable.Range(0, threadCount).Select(threadIndex => Task.Run(() =>
        {
            var rng = new Random(random.Next());

            for (var i = 0; i < logsPerThread; i++)
            {
                if (cancellationToken.IsCancellationRequested) break;

                var level = Levels[rng.Next(Levels.Length)];
                var template = Messages[rng.Next(Messages.Length)];
                var message = string.Format(template, rng.Next(1, 9999));

                logger.AddLog(level, message);
            }
        }, cancellationToken));

        await Task.WhenAll(tasks);

        stopwatch.Stop();
        Console.WriteLine($"Generation complete: {totalLogs:N0} logs in {stopwatch.Elapsed.TotalSeconds:F2}s");
    }
}