namespace Simple.BotUtils.Startup;

using System;

[AttributeUsage(AttributeTargets.Property)]
public class ArgumentKeyAttribute(params string[] keys) : Attribute
{
    public string[] Keys { get; } = keys;
}
