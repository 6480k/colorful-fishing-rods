﻿using System.Runtime.CompilerServices;

using AtraBase.Toolkit;

namespace AtraShared.Caching;

/// <summary>
/// Wrapper class: caches a value for approximately four ticks.
/// </summary>
/// <typeparam name="T">Type of the value.</typeparam>
public struct TickCache<T>
{
    private int lastTick = -1;
    private T? result = default;
    private Func<T> get;

    /// <summary>
    /// Initializes a new instance of the <see cref="TickCache{T}"/> struct.
    /// </summary>
    /// <param name="get">Function that will get the value.</param>
    public TickCache(Func<T> get)
    {
        this.get = get;
    }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <returns>Value.</returns>
    [MethodImpl(TKConstants.Hot)]
    public T? GetValue()
    {
        if ((Game1.ticks & ~0b11) != this.lastTick)
        {
            this.lastTick = Game1.ticks & ~0b11;
            this.result = this.get();
        }
        return this.result;
    }

    /// <summary>
    /// Clears the cache.
    /// </summary>
    public void Reset() => this.lastTick = -1;
}