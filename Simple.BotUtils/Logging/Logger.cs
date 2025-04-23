#if !NETSTANDARD1_0
namespace Simple.BotUtils.Logging;

using System;
using System.Text.RegularExpressions;

public class Logger
{
    public enum LogEventLevel
    {
        Debug,
        Information,
        Warning,
        Error,
        Fatal
    }
    private static readonly Regex regex = new(@"\{([^}]+)\}");

    internal static string MessageBuider(LogEventLevel level, string messageTemplate, params object[] propertyValues)
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
        return $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff zzz}] {strLevel} {formattedMessage}";
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
}
#endif