#if !NETSTANDARD1_0
namespace Simple.BotUtils.Logging;

using System;

public class LogToConsole : ILogger
{
    public void Information(string messageTemplate, params object[] propertyValues)
    {
        Console.WriteLine(Logger.MessageBuider(Logger.LogEventLevel.Information, messageTemplate, propertyValues));
    }

    public void Warning(string messageTemplate, params object[] propertyValues)
    {
        Console.WriteLine(Logger.MessageBuider(Logger.LogEventLevel.Warning, messageTemplate, propertyValues));
    }

    public void Error(string messageTemplate, params object[] propertyValues)
    {
        Console.WriteLine(Logger.MessageBuider(Logger.LogEventLevel.Error, messageTemplate, propertyValues));
    }

    public void Error(Exception exception, string messageTemplate, params object[] propertyValues)
    {
        Console.WriteLine(Logger.MessageBuider(Logger.LogEventLevel.Error, messageTemplate, propertyValues));
        Console.WriteLine($"Exception: {exception.Message}");
        Console.WriteLine($"StackTrace: {exception.StackTrace}");
    }

    public void Fatal(string messageTemplate, params object[] propertyValues)
    {
        Console.WriteLine(Logger.MessageBuider(Logger.LogEventLevel.Fatal, messageTemplate, propertyValues));
    }

    public void Debug(string messageTemplate, params object[] propertyValues)
    {
        Console.WriteLine(Logger.MessageBuider(Logger.LogEventLevel.Debug, messageTemplate, propertyValues));
    }

}

#endif