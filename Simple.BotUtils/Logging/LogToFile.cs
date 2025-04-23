#if !NETSTANDARD1_0
namespace Simple.BotUtils.Logging;

using System;
using System.IO;
using System.Text;

public class LogToFile : ILogger
{
    private readonly string _filePath;
    private readonly object _lock = new();

    public LogToFile(string filePath)
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
    }

    public void Information(string messageTemplate, params object[] propertyValues)
    {
        WriteToFile(Logger.MessageBuider(LogEventLevel.Information, messageTemplate, propertyValues));
    }

    public void Warning(string messageTemplate, params object[] propertyValues)
    {
        WriteToFile(Logger.MessageBuider(LogEventLevel.Warning, messageTemplate, propertyValues));
    }

    public void Error(string messageTemplate, params object[] propertyValues)
    {
        WriteToFile(Logger.MessageBuider(LogEventLevel.Error, messageTemplate, propertyValues));
    }

    public void Error(Exception exception, string messageTemplate, params object[] propertyValues)
    {
        var message = Logger.MessageBuider(LogEventLevel.Error, messageTemplate, propertyValues);
        var fullMessage = $"{message}\nException: {exception.Message}\nStackTrace: {exception.StackTrace}";
        WriteToFile(fullMessage);
    }

    public void Fatal(string messageTemplate, params object[] propertyValues)
    {
        WriteToFile(Logger.MessageBuider(LogEventLevel.Fatal, messageTemplate, propertyValues));
    }

    public void Debug(string messageTemplate, params object[] propertyValues)
    {
        WriteToFile(Logger.MessageBuider(LogEventLevel.Debug, messageTemplate, propertyValues));
    }

    private void WriteToFile(string message)
    {
        lock (_lock)
        {
            try
            {
                using var stream = new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using var writer = new StreamWriter(stream, Encoding.UTF8);
                writer.WriteLine(message);
                writer.Flush();
            }
            catch (IOException ex)
            {
                Console.WriteLine($"[ERROR] Falha ao escrever no arquivo de log: {ex.Message}");
            }
        }
    }
}

#endif