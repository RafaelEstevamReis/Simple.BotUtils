namespace Simple.BotUtils;

using System;

public static class DateTimeExtensions
{
    public static TimeSpan? Age(this DateTime? dateTime) => dateTime == null ? null : (DateTime.UtcNow - dateTime);
    public static TimeSpan Age(this DateTime dateTime) => DateTime.UtcNow - dateTime;

    public static DateTime FromUnixSeconds(this long seconds) => GetUnixEpoch().AddSeconds(seconds);
    public static DateTime FromUnixMilis(this long milis) => GetUnixEpoch().AddMilliseconds(milis);


#if NETSTANDARD || NET20_OR_GREATER
    private static readonly DateTime Epoch = new DateTime(1970, 1, 1);
    public static DateTime GetUnixEpoch() => Epoch;
#else
    public static DateTime GetUnixEpoch() => DateTime.UnixEpoch;
#endif
}
