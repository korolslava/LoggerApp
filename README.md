# LoggerApp

![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet)
![C#](https://img.shields.io/badge/C%23-12.0-239120)
![Console](https://img.shields.io/badge/App-Console-blue)
![Async](https://img.shields.io/badge/Async-Channel%3CT%3E-orange)
![JSON](https://img.shields.io/badge/Format-JSON-green)
![File I/O](https://img.shields.io/badge/Storage-File%20I%2FO-yellow)
![Status](https://img.shields.io/badge/Status-Active-brightgreen)

**LoggerApp** is a lightweight C# console application for creating, storing, filtering, searching, and displaying logs with colored terminal output.

---

## Features

- Add logs manually
- Select log level with keyboard arrows
- Print logs with colors by severity
- Search logs by keyword, date range, and level
- Save and load logs from file
- JSON serialization support
- Custom log formatting with `FormatTemplate`
- Async log processing with `Channel<LogEntry>`

---

## Log Levels

| Level | Color |
|---|---|
| `Debug` | Gray |
| `Info` | Cyan |
| `Warning` | Yellow |
| `Error` | Red |

---

## Commands

| Command | Description |
|---|---|
| `A` | Add log manually |
| `D` | Delete log by ID |
| `F` | Change filter level |
| `P` | Print logs |
| `C` | Search logs |
| `S` | Save logs |
| `R` | Read logs |
| `L` | Toggle console listener |
| `H` | Show help menu |
| `Q` | Quit application |

---

## Project Structure

```text
LoggerApp/
├── Core/
│   ├── ILogger.cs
│   └── Logger.cs
├── Listeners/
│   ├── ILogListener.cs
│   ├── ConsoleLogListener.cs
│   └── FileLogListener.cs
├── Models/
│   ├── LogEntry.cs
│   └── LogLevel.cs
├── Services/
│   ├── LogGeneratorService.cs
│   └── LogMenuService.cs
├── Application.cs
└── Program.cs
