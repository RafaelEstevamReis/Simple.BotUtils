#if !NETSTANDARD1_0
namespace Simple.BotUtils.Logging;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public enum LogEventLevel
{
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Fatal = 5
}
public class Logger : ILogger
{
    LogEventLevel minLevel;
    ILogger[] loggers;

    internal Logger(ILogger[] loggers, LogEventLevel minLevel)
    {
        this.loggers = loggers;
        this.minLevel = minLevel;
    }

    private static readonly Regex regex = new(@"\{([^}]+)\}");
    internal static string MessageBuider(LogEventLevel level, bool timeOnly, string messageTemplate, params object[] propertyValues)
    {
        var strLevel = level switch
        {
            LogEventLevel.Debug => "[DBG]",
            LogEventLevel.Information => "[INF]",
            LogEventLevel.Warning => "[WRN]",
            LogEventLevel.Error => "[ERR]",
            LogEventLevel.Fatal => "[FTL]",
            _ => "[-]",
        };
        var formattedMessage = FormatMessage(messageTemplate, propertyValues);
        if(timeOnly) return $"[{DateTime.Now:HH:mm:ss} {strLevel}] {formattedMessage}";
        return $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff zzz} {strLevel}] {formattedMessage}";
    }
    private static string FormatMessage(string messageTemplate, object[] propertyValues)
    {
        if (string.IsNullOrEmpty(messageTemplate))
            return string.Empty;

        string result = messageTemplate;
        var placeholders = regex.Matches(messageTemplate);

        if (placeholders.Count > propertyValues.Length)
        {
            return $"Error: Insuficient values for template '{messageTemplate}'";
        }

        for (int i = 0; i < placeholders.Count; i++)
        {
            string placeholder = placeholders[i].Value;
            string propertyName = placeholders[i].Groups[1].Value;
            object value = propertyValues[i];

            string formattedValue;
            if (propertyName.StartsWith('@') && value != null)
            {
                formattedValue = value.GetType().IsPrimitive || value is string
                    ? value.ToString()
                    : $"[{value.GetType().Name}] {Newtonsoft.Json.JsonConvert.SerializeObject(value)}";
            }
            else
            {
                formattedValue = value?.ToString() ?? "null";
            }

            result = result.Replace(placeholder, formattedValue);
        }

        return result;
    }

    public void Debug(string messageTemplate, params object[] propertyValues)
    {
        if (minLevel > LogEventLevel.Debug) return;

        foreach (var l in loggers) l.Debug(messageTemplate, propertyValues);
    }
    public void Information(string messageTemplate, params object[] propertyValues)
    {
        if (minLevel > LogEventLevel.Information) return;

        foreach (var l in loggers) l.Information(messageTemplate, propertyValues);
    }
    public void Warning(string messageTemplate, params object[] propertyValues)
    {
        if (minLevel > LogEventLevel.Warning) return;

        foreach (var l in loggers) l.Warning(messageTemplate, propertyValues);
    }
    public void Error(Exception exception, string messageTemplate, params object[] propertyValues)
    {
        if (minLevel > LogEventLevel.Error) return;

        foreach (var l in loggers) l.Error(exception, messageTemplate, propertyValues);
    }
    public void Error(string messageTemplate, params object[] propertyValues)
    {
        if (minLevel > LogEventLevel.Error) return;

        foreach (var l in loggers) l.Error(messageTemplate, propertyValues);
    }
    public void Fatal(string messageTemplate, params object[] propertyValues)
    {
        if (minLevel > LogEventLevel.Fatal) return;

        foreach (var l in loggers) l.Fatal(messageTemplate, propertyValues);
    }
}
public class LoggerBuilder
{
    LogEventLevel minLevel = LogEventLevel.Information;
    List<ILogger> loggers = [];

    public LoggerBuilder LogToFile(string filePath)
    {
        loggers.Add(new LogToFile(filePath));
        return this;
    }
    public LoggerBuilder LogToConsole()
    {
        loggers.Add(new LogToConsole());
        return this;
    }
    public LoggerBuilder SetMinimumLevel(LogEventLevel level)
    {
        minLevel = level;
        return this;
    }

    public Logger CreateLogger()
    {
        return new Logger(loggers.ToArray(), minLevel);
    }

}
#endif