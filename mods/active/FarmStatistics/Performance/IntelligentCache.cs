using System;
using System.Collections.Generic;
using System.Threading;

namespace FarmStatistics.Performance
{
    public class IntelligentCache<TKey, TValue> where TKey : notnull
    {
        private readonly Dictionary<TKey, CacheEntry<TValue>> _cache = new();
        private readonly TimeSpan _defaultExpiry;
        private readonly Timer _cleanupTimer;

        public IntelligentCache(TimeSpan defaultExpiry)
        {
            _defaultExpiry = defaultExpiry;
            _cleanupTimer = new Timer(PerformCleanup, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public bool TryGetValue(TKey key, out TValue? value)
        {
            if (_cache.TryGetValue(key, out var entry) && !entry.IsExpired)
            {
                value = entry.Value;
                return true;
            }

            value = default;
            return false;
        }

        public void Set(TKey key, TValue value, TimeSpan? expiry = null)
        {
            var expirationTime = DateTime.Now + (expiry ?? _defaultExpiry);
            _cache[key] = new CacheEntry<TValue>(value, expirationTime);
        }

        private void PerformCleanup(object? state)
        {
            var keysToRemove = new List<TKey>();
            foreach (var pair in _cache)
            {
                if (pair.Value.IsExpired)
                {
                    keysToRemove.Add(pair.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }
        }
    }

    internal class CacheEntry<TValue>
    {
        public TValue Value { get; }
        private readonly DateTime _expirationTime;

        public bool IsExpired => DateTime.Now >= _expirationTime;

        public CacheEntry(TValue value, DateTime expirationTime)
        {
            Value = value;
            _expirationTime = expirationTime;
        }
    }
}
