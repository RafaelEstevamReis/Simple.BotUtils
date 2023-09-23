namespace Simple.BotUtils.Helpers;

using System.Collections.Generic;

public static class DictionaryHelper
{
    public static T Get<K, T>(this Dictionary<K, T> dic, K key, T def)
    {
        if (dic.ContainsKey(key)) return dic[key];
        return def;
    }

}
