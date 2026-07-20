namespace Simple.BotUtils.Helpers;

using System;
using System.Collections.Generic;

public class GenericComparer<T>(Func<T, T, bool> equalsFunc, Func<T, int>? hashFunc = null) : IEqualityComparer<T>
{
    private readonly Func<T, T, bool> equalsFunc = equalsFunc ?? throw new ArgumentNullException(nameof(equalsFunc));
    private readonly Func<T, int> hashFunc = hashFunc ?? (obj => obj?.GetHashCode() ?? 0);

    public bool Equals(T? x, T? y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;

        return equalsFunc(x, y);
    }

    public int GetHashCode(T obj)
    {
        return hashFunc(obj);
    }
}