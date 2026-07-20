namespace Simple.BotUtils.Helpers;

using System.Collections.Generic;

public static class DictionaryHelper
{
    public static T Get<K, T>(this Dictionary<K, T> dic, K key, T def)
        where K : notnull
    {
        if (dic.TryGetValue(key, out T? value)) return value;
        return def;
    }

}
