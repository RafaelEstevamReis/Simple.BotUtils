namespace Simple.BotUtils.Helpers;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class Randomizer
{
    public static Random Instance { get; private set; } = new Random();

    public static T ChooseOne<T>(this T[] options)
    {
        var idx = Instance.Next(options.Length);
        return options[idx];
    }
    public static T ChooseOne<T>(this List<T> options)
    {
        var idx = Instance.Next(options.Count);
        return options[idx];
    }

    public static int GetRandomInt(int max)
        => Instance.Next(max);
    public static int GetRandomInt(int min, int max)
        => Instance.Next(min, max);
    public static double GetRandomDouble()
    {
        return Instance.NextDouble();
    }

    public static float GetRandomFloat()
    {
#if NET6_0_OR_GREATER
        return Instance.NextSingle();
#else
        return (float)Instance.NextDouble();
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Shuffle<T>(this IList<T> list)
    {
        if (list is null) throw new ArgumentNullException("list");

        int n = list.Count;
        while (n > 1)
        {
            int k = Instance.Next(n--);

#if NET6_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER
            (list[n], list[k]) = (list[k], list[n]);
#else
            var temp = list[k];
            list[k] = list[n];
            list[n] = temp;
#endif
        }
    }


}
