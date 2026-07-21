namespace Simple.BotUtils.Caching;

using System;

public class CacheOptions
{
    public Func<object>? UpdateCallback { get; set; }
    public ExpirationPolicy ExpirationPolicy { get; set; }
    public TimeSpan ExpirationValue { get; set; }
}
