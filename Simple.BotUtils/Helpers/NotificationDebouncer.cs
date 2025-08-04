namespace Simple.BotUtils.Helpers;

using System;
using System.Collections.Generic;

public class NotificationDebouncer<T>
{
    private readonly IEqualityComparer<T> comparer;

    private T currentValue;
    private T candidateValue;
    private bool hasCandidate;
    private int hitCount = 0;

    public int MinBounces { get; set; } = 0;
    public event EventHandler<NewValueEventArgs> NewValue;

    public NotificationDebouncer(IEqualityComparer<T> comparer = null)
    {
        this.comparer = comparer ?? EqualityComparer<T>.Default;
    }

    public bool SetValue(T value)
    {
        // Is equal to current value?
        if (comparer.Equals(value, currentValue))
        {
            // Reset counters
            hitCount = 0;
            hasCandidate = false;
            return false;
        }

        if (!hasCandidate || !comparer.Equals(value, candidateValue))
        {
            // New or Different value
            candidateValue = value;
            hasCandidate = true;
            hitCount = 0;
        }

        // Increment candidate
        hitCount++;

        // Is enough?
        if (hitCount < MinBounces) return false;

        // Update new value
        T oldValue = currentValue;
        currentValue = candidateValue;
        // Reset
        hasCandidate = false;
        hitCount = 0;
        // Invoke
        NewValue?.Invoke(this, NewValueEventArgs.Create(currentValue, oldValue, MinBounces));
        return true;
    }

    public class NewValueEventArgs : EventArgs
    {
        private NewValueEventArgs(T newValue, T oldValue, int bounces)
        {
            NewValue = newValue;
            OldValue = oldValue;
            Bounces = bounces;
        }

        public T NewValue { get; }
        public T OldValue { get; }
        public int Bounces { get; }

        public static NewValueEventArgs Create(T newValue, T oldValue, int bounces)
        {
            return new NewValueEventArgs(newValue, oldValue, bounces);
        }
    }
}
