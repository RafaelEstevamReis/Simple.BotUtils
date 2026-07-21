namespace Simple.BotUtils.DI;

using System;

internal class InjectedObject
{
    public Func<object>? Constructor { get; set; }
    public object? Instance { get; set; }
    public InjectionType InjectionType { get; set; }
}
