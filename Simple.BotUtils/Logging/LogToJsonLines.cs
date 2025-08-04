#if !NETSTANDARD1_0
namespace Simple.BotUtils.Logging;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class LogToJsonLines : ILogger
{
    private readonly string _filePath;
    private readonly object _lock = new();

    public LogToJsonLines(string filePath)
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
        WriteToFile(CreateJsonLog(LogEventLevel.Information, messageTemplate, propertyValues));
    }

    public void Warning(string messageTemplate, params object[] propertyValues)
    {
        WriteToFile(CreateJsonLog(LogEventLevel.Warning, messageTemplate, propertyValues));
    }

    public void Error(string messageTemplate, params object[] propertyValues)
    {
        WriteToFile(CreateJsonLog(LogEventLevel.Error, messageTemplate, propertyValues));
    }

    public void Error(Exception exception, string messageTemplate, params object[] propertyValues)
    {
        var jsonLog = CreateJsonLog(LogEventLevel.Error, messageTemplate, propertyValues);
        var logObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonLog);
        logObject["Exception"] = BuildExceptionDetails(exception);
        WriteToFile(JsonConvert.SerializeObject(logObject, Formatting.None));
    }

    public void Fatal(string messageTemplate, params object[] propertyValues)
    {
        WriteToFile(CreateJsonLog(LogEventLevel.Fatal, messageTemplate, propertyValues));
    }

    public void Debug(string messageTemplate, params object[] propertyValues)
    {
        WriteToFile(CreateJsonLog(LogEventLevel.Debug, messageTemplate, propertyValues));
    }

    private string CreateJsonLog(LogEventLevel level, string messageTemplate, params object[] propertyValues)
    {
        var message = Logger.MessageBuider(level, false, messageTemplate, propertyValues);
        var logObject = new Dictionary<string, object>
        {
            { "Timestamp", DateTime.UtcNow.ToString("o") },
            { "Level", level.ToString() },
            { "Message", message }
        };

        // Adiciona as propriedades de propertyValues, se houver
        if (propertyValues != null && propertyValues.Length > 0)
        {
            var properties = new Dictionary<string, object>();
            for (int i = 0; i < propertyValues.Length; i++)
            {
                properties[$"Property_{i}"] = propertyValues[i] ?? "null";
            }
            logObject["Properties"] = properties;
        }

        return JsonConvert.SerializeObject(logObject, Formatting.None);
    }

    private Dictionary<string, object> BuildExceptionDetails(Exception exception)
    {
        var details = new Dictionary<string, object>
        {
            { "Message", exception.Message },
            { "StackTrace", exception.StackTrace ?? "No stack trace available" }
        };

        // Adiciona inner exceptions, se houver
        if (exception.InnerException != null)
        {
            details["InnerException"] = BuildExceptionDetails(exception.InnerException);
        }

        return details;
    }
    private void WriteToFile(string jsonLine)
    {
        lock (_lock)
        {
            try
            {
                using var stream = new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using var writer = new StreamWriter(stream, Encoding.UTF8);
                writer.WriteLine(jsonLine);
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