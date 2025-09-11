using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace DevStack.Infrastructure.BoardForge.Tests;

public static class MemoryCacheMoqExtensions
{
    /// <summary>
    /// Hit (specific key) and returns the provided value (true)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void SetupTryGetValueHit<T>(this Mock<IMemoryCache> mock, object key, T value)
    {
        object boxed = value!;
        mock.Setup(m => m.TryGetValue(key, out boxed)).Returns(true);
    }

    /// <summary>
    /// Miss (specific key) and returns the provided value (false)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    public static void SetupTryGetValueMiss(this Mock<IMemoryCache> mock, object key)
    {
        object _;
        mock.Setup(m => m.TryGetValue(key, out _)).Returns(false);
    }

    // Miss (any key)
    public static void SetupTryGetValueMiss(this Mock<IMemoryCache> mock)
    {
        object _; // out param ignored
        mock.Setup(m => m.TryGetValue(It.IsAny<object>(), out _)).Returns(false);
    }

    /// <summary>
    /// Sets up the IMemoryCache mock so that when Set(key, value, expiration) is called,
    /// it creates an ICacheEntry that stores the provided value and expiration.
    /// Returns the ICacheEntry mock for further verification.
    /// </summary>
    public static Mock<ICacheEntry> SetupSet<T>(
        this Mock<IMemoryCache> mock,
        object expectedKey,
        T expectedValue,
        TimeSpan? expectedExpiration = null)
    {
        var entry = new Mock<ICacheEntry>();
        entry.SetupAllProperties();

        bool disposed = false;
        entry.Setup(e => e.Dispose()).Callback(() => disposed = true);

        mock.Setup(c => c.CreateEntry(expectedKey)).Returns(entry.Object);

        // When the extension calls .Value, capture the value
        entry.SetupProperty(e => e.Value, expectedValue);

        // If we want to assert expiration, hook into AbsoluteExpirationRelativeToNow
        if (expectedExpiration != null)
        {
            entry.SetupProperty(e => e.AbsoluteExpirationRelativeToNow, expectedExpiration);
        }

        return entry;
    }
}