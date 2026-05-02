namespace LoggerApp.Services;

using LoggerApp.Core;
using LoggerApp.Listeners;
using LoggerApp.Models;

public class LogMenuService(ILogger logger, ConsoleLogListener consoleListener)
{
    private const string LogFilePath = "logs_save.txt";

    public async Task RunAsync()
    {
        PrintHelp();

        while (true)
        {
            Console.Write("\n> ");
            var input = Console.ReadLine()?.Trim().ToUpper();

            switch (input)
            {
                case "A":
                    HandleAdd();
                    break;
                case "D":
                    HandleDelete();
                    break;
                case "F":
                    HandleFilter();
                    break;
                case "P":
                    HandlePrint();
                    break;
                case "S":
                    await logger.SaveToFileAsync(LogFilePath);
                    break;
                case "R":
                    await logger.LoadFromFileAsync(LogFilePath);
                    break;
                case "L":
                    consoleListener.Toggle();
                    Console.WriteLine($"Console listener: {(consoleListener.IsEnabled ? "ON" : "OFF")}");
                    break;
                case "H":
                    PrintHelp();
                    break;
                case "Q":
                    Console.WriteLine("Goodbye!");
                    return;
                default:
                    Console.WriteLine("Unknown command. Press H for help.");
                    break;
            }
        }
    }

    private void HandleAdd()
    {
        Console.Write("Level (Debug/Info/Warning/Error): ");
        if (!Enum.TryParse<LogLevel>(Console.ReadLine(), ignoreCase: true, out var level))
        {
            Console.WriteLine("Invalid level.");
            return;
        }

        Console.Write("Message: ");
        var message = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(message))
        {
            Console.WriteLine("Message cannot be empty.");
            return;
        }

        logger.AddLog(level, message);
        Console.WriteLine("Log added.");
    }

    private void HandleDelete()
    {
        Console.Write("Log ID to delete: ");
        if (!Guid.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Invalid ID.");
            return;
        }
        logger.DeleteLog(id);
        Console.WriteLine("Log deleted.");
    }

    private void HandleFilter()
    {
        Console.Write("Filter level (Debug/Info/Warning/Error) or ALL: ");
        var input = Console.ReadLine()?.Trim();

        if (string.Equals(input, "ALL", StringComparison.OrdinalIgnoreCase))
        {
            logger.SetFilterLevel(null);
            return;
        }

        if (!Enum.TryParse<LogLevel>(input, ignoreCase: true, out var level))
        {
            Console.WriteLine("Invalid level.");
            return;
        }

        logger.SetFilterLevel(level);
    }

    private static void WriteColoredLog(LogEntry entry)
    {
        var color = entry.Level switch
        {
            LogLevel.Debug => ConsoleColor.Gray,
            LogLevel.Info => ConsoleColor.Cyan,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            _ => ConsoleColor.White
        };

        Console.ForegroundColor = color;
        Console.WriteLine($"[{entry.Id}] {entry}");
        Console.ResetColor();
    }

    private void HandlePrint()
    {
        var logs = logger.GetLogs();

        if (logs.Count == 0)
        {
            Console.WriteLine("No logs to display.");
            return;
        }

        Console.WriteLine($"\n--- {logs.Count:N0} logs ---");

        foreach (var entry in logs)
        {
            WriteColoredLog(entry);
        }

        Console.WriteLine("---");
    }

    private static void PrintHelp()
    {
        Console.WriteLine("""

            ╔══════════════════════════════════════╗
            ║           LOGGER APP MENU            ║
            ╠══════════════════════════════════════╣
            ║  A - Add log manually                ║
            ║  D - Delete log by index             ║
            ║  F - Change filter level             ║
            ║  P - Print logs                      ║
            ║  S - Save logs to file               ║
            ║  R - Read logs from file             ║
            ║  L - Toggle console listener         ║
            ║  H - Show this help                  ║
            ║  Q - Quit                            ║
            ╚══════════════════════════════════════╝
            """);
    }
}