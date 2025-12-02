#if !NETSTANDARD1_0
namespace Simple.BotUtils.Logging;

using System;

public class LogToConsole : ILogger
{
    public LogEventLevel MinLevel { get; set; }

    public LogToConsole(LogEventLevel minLevel = LogEventLevel.Information)
    {
        MinLevel = minLevel;
    }

    public void Information(string messageTemplate, params object[] propertyValues)
    {
        write(LogEventLevel.Information, messageTemplate, propertyValues);
    }

    public void Warning(string messageTemplate, params object[] propertyValues)
    {
        write(LogEventLevel.Warning, messageTemplate, propertyValues);
    }

    public void Error(string messageTemplate, params object[] propertyValues)
    {
        write(LogEventLevel.Error, messageTemplate, propertyValues);
    }

    public void Error(Exception exception, string messageTemplate, params object[] propertyValues)
    {
        if (MinLevel > LogEventLevel.Error) return;
        Console.WriteLine(Logger.MessageBuider(LogEventLevel.Error, true, messageTemplate, propertyValues));
        Console.WriteLine($"Exception: {exception.Message}");
        Console.WriteLine($"StackTrace: {exception.StackTrace}");
    }

    public void Fatal(string messageTemplate, params object[] propertyValues)
    {
        write(LogEventLevel.Fatal, messageTemplate, propertyValues);
    }

    public void Debug(string messageTemplate, params object[] propertyValues)
    {
        write(LogEventLevel.Debug, messageTemplate, propertyValues);
    }

    private void write(LogEventLevel level, string messageTemplate, params object[] propertyValues)
    {
        if (MinLevel > level) return;
        Console.WriteLine(Logger.MessageBuider(level, true, messageTemplate, propertyValues));
    }

}

#endif