namespace LoggerApp.Core;

using LoggerApp.Models;

public interface ILogger
{
    void AddLog(LogLevel level, string message);
    void DeleteLog(Guid id);
    void SetFilterLevel(LogLevel? level);
    IReadOnlyList<LogEntry> GetLogs(LogLevel? filterLevel = null);
    Task SaveToFileAsync(string filePath);
    Task LoadFromFileAsync(string filePath);
}