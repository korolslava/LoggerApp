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
                case "C":
                    HandleSearch();
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
        var level = SelectLogLevel();

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

    private static LogLevel SelectLogLevel()
    {
        var levels = Enum.GetValues<LogLevel>();
        var selectedIndex = 0;

        Console.WriteLine("Select log level:");

        while (true)
        {
            var startTop = Console.CursorTop;

            for (var i = 0; i < levels.Length; i++)
            {
                Console.SetCursorPosition(0, startTop + i);

                if (i == selectedIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"> {levels[i]}");
                    Console.ResetColor();
                }
                else
                {
                    Console.Write($"  {levels[i]}");
                }

                Console.Write(new string(' ', 20));
            }

            var key = Console.ReadKey(intercept: true).Key;

            if (key == ConsoleKey.UpArrow)
            {
                selectedIndex = selectedIndex == 0
                    ? levels.Length - 1
                    : selectedIndex - 1;
            }
            else if (key == ConsoleKey.DownArrow)
            {
                selectedIndex = selectedIndex == levels.Length - 1
                    ? 0
                    : selectedIndex + 1;
            }
            else if (key == ConsoleKey.Enter)
            {
                Console.SetCursorPosition(0, startTop + levels.Length);
                Console.WriteLine();
                return levels[selectedIndex];
            }

            Console.SetCursorPosition(0, startTop);
        }
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

    private void HandleSearch()
    {
        Console.Write("Keyword (leave empty for none): ");
        var keyword = Console.ReadLine();

        Console.Write("From date (yyyy-MM-dd or empty): ");
        var fromInput = Console.ReadLine();

        DateTime? from = null;

        if (!string.IsNullOrWhiteSpace(fromInput))
        {
            if (DateTime.TryParse(fromInput, out var parsedFrom))
            {
                from = parsedFrom;
            }
            else
            {
                Console.WriteLine("Invalid from date. Ignoring.");
            }
        }

        Console.Write("To date (yyyy-MM-dd or empty): ");
        var toInput = Console.ReadLine();

        DateTime? to = null;

        if (!string.IsNullOrWhiteSpace(toInput))
        {
            if (DateTime.TryParse(toInput, out var parsedTo))
            {
                to = parsedTo;
            }
            else
            {
                Console.WriteLine("Invalid to date. Ignoring.");
            }
        }

        Console.Write("Minimum level (Debug/Info/Warning/Error or ALL): ");
        var levelInput = Console.ReadLine();

        LogLevel? minLevel = null;

        if (!string.IsNullOrWhiteSpace(levelInput) &&
            !string.Equals(levelInput, "ALL", StringComparison.OrdinalIgnoreCase))
        {
            if (Enum.TryParse<LogLevel>(levelInput, ignoreCase: true, out var parsedLevel))
            {
                minLevel = parsedLevel;
            }
            else
            {
                Console.WriteLine("Invalid level. Ignoring.");
            }
        }

        var results = logger.SearchLogs(keyword, from, to, minLevel);

        if (results.Count == 0)
        {
            Console.WriteLine("No logs match the search criteria.");
            return;
        }

        Console.WriteLine($"\n--- {results.Count:N0} matching logs ---");

        foreach (var entry in results)
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
            ║  C - Search logs                     ║
            ║  S - Save logs to file               ║
            ║  R - Read logs from file             ║
            ║  L - Toggle console listener         ║
            ║  H - Show this help                  ║
            ║  Q - Quit                            ║
            ╚══════════════════════════════════════╝
            """);
    }
}