using System;
using System.IO;

namespace ReunionWelcomeApp.Services;

public static class LoggingService
{
    private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "app.log");
    private static readonly object _lock = new();

    static LoggingService()
    {
        var logDir = Path.GetDirectoryName(LogPath);
        if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
            Directory.CreateDirectory(logDir);
    }

    public static void Log(string message, string level = "INFO")
    {
        lock (_lock)
        {
            try
            {
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                File.AppendAllText(LogPath, logEntry + Environment.NewLine);
                // Also output to console for debugging
                Console.WriteLine(logEntry);
            }
            catch { }
        }
    }

    public static void LogError(string message, Exception? ex = null)
    {
        Log(ex != null ? $"{message}: {ex.Message}\n{ex.StackTrace}" : message, "ERROR");
    }

    public static void LogWarning(string message) => Log(message, "WARN");
}