#if !NETSTANDARD1_0
namespace Simple.BotUtils.Logging;

using System;
using System.IO;
using System.Text;

public class LogToFile : ILogger
{
    public enum RotateOptions
    {
        NoRotation = 0,
        Daily = 1,
        Monthly = 2,
        Yearly = 3,
    }

    private readonly string _filePath;
    private readonly object _lock = new();
    private readonly RotateOptions logRotate;

    public LogToFile(string filePath, RotateOptions logRotation = RotateOptions.NoRotation)
    {
        _filePath = string.IsNullOrWhiteSpace(filePath)
            ? throw new ArgumentException("File path cannot be null or empty.", nameof(filePath))
            : filePath;

        // Garante que o diretório exista
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        this.logRotate = logRotation;
    }

    public void Information(string messageTemplate, params object[] propertyValues)
    {
        WriteToFile(Logger.MessageBuider(LogEventLevel.Information, false, messageTemplate, propertyValues));
    }

    public void Warning(string messageTemplate, params object[] propertyValues)
    {
        WriteToFile(Logger.MessageBuider(LogEventLevel.Warning, false, messageTemplate, propertyValues));
    }

    public void Error(string messageTemplate, params object[] propertyValues)
    {
        WriteToFile(Logger.MessageBuider(LogEventLevel.Error, false, messageTemplate, propertyValues));
    }

    public void Error(Exception exception, string messageTemplate, params object[] propertyValues)
    {
        var message = Logger.MessageBuider(LogEventLevel.Error, false, messageTemplate, propertyValues);
        var fullMessage = $"{message}\nException: {exception.Message}\nStackTrace: {exception.StackTrace}";
        WriteToFile(fullMessage);
    }

    public void Fatal(string messageTemplate, params object[] propertyValues)
    {
        WriteToFile(Logger.MessageBuider(LogEventLevel.Fatal, false, messageTemplate, propertyValues));
    }

    public void Debug(string messageTemplate, params object[] propertyValues)
    {
        WriteToFile(Logger.MessageBuider(LogEventLevel.Debug, false, messageTemplate, propertyValues));
    }

    private void WriteToFile(string message)
    {
        string fName = _filePath;
        if (logRotate != RotateOptions.NoRotation)
        {
            var utcNow = DateTime.UtcNow;
            string append = logRotate switch
            {
                RotateOptions.Daily => $"_{utcNow:yyyyMMdd}",
                RotateOptions.Monthly => $"_{utcNow:yyyyMM}",
                RotateOptions.Yearly => $"_{utcNow:yyyy}",
                //RotateOptions.NoRotation => "",
                _ => "",
            };
            fName = Path.GetFileNameWithoutExtension(fName) + append + Path.GetExtension(fName);
        }

        lock (_lock)
        {
            try
            {
                using var stream = new FileStream(fName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using var writer = new StreamWriter(stream, Encoding.UTF8);
                writer.WriteLine(message);
                writer.Flush();
            }
            catch (IOException ex)
            {
                Console.WriteLine($"[ERROR] Failed to write log file: {ex}");
            }
        }
    }
}

#endif