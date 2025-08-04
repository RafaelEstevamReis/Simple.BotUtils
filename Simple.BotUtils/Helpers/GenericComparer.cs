namespace Simple.BotUtils.Helpers;

using System;
using System.Collections.Generic;

public class GenericComparer<T> : IEqualityComparer<T>
{
    private readonly Func<T, T, bool> equalsFunc;
    private readonly Func<T, int> hashFunc;

    public GenericComparer(Func<T, T, bool> equalsFunc, Func<T, int> hashFunc = null)
    {
        this.equalsFunc = equalsFunc ?? throw new ArgumentNullException(nameof(equalsFunc));
        this.hashFunc = hashFunc ?? (obj => obj?.GetHashCode() ?? 0);
    }

    public bool Equals(T x, T y)
    {
        return equalsFunc(x, y);
    }

    public int GetHashCode(T obj)
    {
        return hashFunc(obj);
    }
}