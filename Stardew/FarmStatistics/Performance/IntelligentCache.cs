using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using StardewModdingAPI;

namespace FarmStatistics.Performance
{
    /// <summary>
    /// LookupAnything 패턴을 적용한 지능형 캐싱 시스템 (Phase 3.1)
    /// 데이터 타입별로 다른 캐싱 전략을 적용하여 성능을 최적화합니다.
    /// </summary>
    public class IntelligentCache<TKey, TValue> : IDisposable where TKey : notnull
    {
        #region Fields

        private readonly IMonitor _monitor;
        private readonly ConcurrentDictionary<TKey, CacheEntry<TValue>> _cache = new();
        private readonly Timer _cleanupTimer;
        private readonly ReaderWriterLockSlim _lock = new();
        
        // 캐시 설정
        private readonly TimeSpan _defaultExpiry;
        private readonly int _maxCacheSize;
        private readonly double _cleanupThreshold;
        
        // 통계
        private long _hitCount = 0;
        private long _missCount = 0;
        private long _evictionCount = 0;
        private DateTime _lastCleanup = DateTime.Now;

        #endregion

        #region Constructor

        public IntelligentCache(IMonitor monitor, 
            TimeSpan? defaultExpiry = null, 
            int maxCacheSize = 1000,
            double cleanupThreshold = 0.8)
        {
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            _defaultExpiry = defaultExpiry ?? TimeSpan.FromMinutes(5);
            _maxCacheSize = maxCacheSize;
            _cleanupThreshold = cleanupThreshold;
            
            // 5분마다 정리 작업 실행
            _cleanupTimer = new Timer(PerformCleanup, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
            
            _monitor.Log($"지능형 캐시 초기화 - 만료시간: {_defaultExpiry}, 최대크기: {_maxCacheSize}", LogLevel.Debug);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 캐시에서 값을 가져옵니다. (ChestsAnywhere 패턴)
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            _lock.EnterReadLock();
            try
            {
                if (_cache.TryGetValue(key, out var entry))
                {
                    if (!entry.IsExpired)
                    {
                        // 히트 - 접근 시간 업데이트
                        entry.LastAccessed = DateTime.Now;
                        entry.AccessCount++;
                        value = entry.Value;
                        
                        Interlocked.Increment(ref _hitCount);
                        _monitor?.Log($"캐시 히트: {key}", LogLevel.Trace);
                        return true;
                    }
                    else
                    {
                        // 만료된 항목 제거 (백그라운드에서)
                        _ = Task.Run(() => RemoveExpiredEntry(key));
                    }
                }
                
                // 미스
                Interlocked.Increment(ref _missCount);
                value = default(TValue);
                _monitor?.Log($"캐시 미스: {key}", LogLevel.Trace);
                return false;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 캐시에 값을 저장합니다. (Automate 패턴)
        /// </summary>
        public void Set(TKey key, TValue value, TimeSpan? expiry = null)
        {
            try
            {
                var expiryTime = expiry ?? _defaultExpiry;
                var entry = new CacheEntry<TValue>
                {
                    Value = value,
                    CreatedAt = DateTime.Now,
                    LastAccessed = DateTime.Now,
                    ExpiresAt = DateTime.Now.Add(expiryTime),
                    AccessCount = 0
                };

                _lock.EnterWriteLock();
                try
                {
                    // 캐시 크기 체크 및 정리
                    if (_cache.Count >= _maxCacheSize * _cleanupThreshold)
                    {
                        PerformEmergencyCleanup();
                    }

                    _cache.AddOrUpdate(key, entry, (k, oldEntry) =>
                    {
                        // 기존 항목 업데이트 시 통계 유지
                        entry.AccessCount = oldEntry.AccessCount;
                        return entry;
                    });

                    _monitor?.Log($"캐시 저장: {key} (만료: {expiryTime})", LogLevel.Trace);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"캐시 저장 중 오류 ({key}): {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 특정 키의 캐시를 무효화합니다.
        /// </summary>
        public bool Invalidate(TKey key)
        {
            try
            {
                _lock.EnterWriteLock();
                try
                {
                    if (_cache.TryRemove(key, out _))
                    {
                        _monitor?.Log($"캐시 무효화: {key}", LogLevel.Debug);
                        return true;
                    }
                    return false;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"캐시 무효화 중 오류 ({key}): {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// 패턴에 맞는 모든 키의 캐시를 무효화합니다.
        /// </summary>
        public int InvalidatePattern(Func<TKey, bool> predicate)
        {
            try
            {
                _lock.EnterWriteLock();
                try
                {
                    var keysToRemove = _cache.Keys.Where(predicate).ToList();
                    var removedCount = 0;

                    foreach (var key in keysToRemove)
                    {
                        if (_cache.TryRemove(key, out _))
                        {
                            removedCount++;
                        }
                    }

                    if (removedCount > 0)
                    {
                        _monitor?.Log($"패턴 캐시 무효화: {removedCount}개 항목", LogLevel.Debug);
                    }

                    return removedCount;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"패턴 캐시 무효화 중 오류: {ex.Message}", LogLevel.Error);
                return 0;
            }
        }

        /// <summary>
        /// 전체 캐시를 지웁니다.
        /// </summary>
        public void Clear()
        {
            try
            {
                _lock.EnterWriteLock();
                try
                {
                    var count = _cache.Count;
                    _cache.Clear();
                    _monitor?.Log($"전체 캐시 지움: {count}개 항목", LogLevel.Debug);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"캐시 지우기 중 오류: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 캐시 통계를 반환합니다.
        /// </summary>
        public CacheStatistics GetStatistics()
        {
            _lock.EnterReadLock();
            try
            {
                var totalRequests = _hitCount + _missCount;
                var hitRate = totalRequests > 0 ? (double)_hitCount / totalRequests * 100 : 0;

                return new CacheStatistics
                {
                    HitCount = _hitCount,
                    MissCount = _missCount,
                    HitRate = hitRate,
                    EvictionCount = _evictionCount,
                    CurrentSize = _cache.Count,
                    MaxSize = _maxCacheSize,
                    LastCleanup = _lastCleanup,
                    ExpiredEntries = _cache.Values.Count(e => e.IsExpired)
                };
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 정기적인 캐시 정리 작업
        /// </summary>
        private void PerformCleanup(object state)
        {
            try
            {
                _lock.EnterWriteLock();
                try
                {
                    var now = DateTime.Now;
                    var expiredKeys = _cache
                        .Where(kvp => kvp.Value.IsExpired)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    var removedCount = 0;
                    foreach (var key in expiredKeys)
                    {
                        if (_cache.TryRemove(key, out _))
                        {
                            removedCount++;
                            Interlocked.Increment(ref _evictionCount);
                        }
                    }

                    _lastCleanup = now;

                    if (removedCount > 0)
                    {
                        _monitor?.Log($"캐시 정리 완료: {removedCount}개 만료 항목 제거", LogLevel.Debug);
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"캐시 정리 중 오류: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 응급 캐시 정리 (LRU 기반)
        /// </summary>
        private void PerformEmergencyCleanup()
        {
            try
            {
                var targetSize = (int)(_maxCacheSize * 0.7); // 70%까지 줄임
                var currentSize = _cache.Count;
                
                if (currentSize <= targetSize) return;

                // LRU 알고리즘으로 제거할 항목 선택
                var itemsToRemove = _cache
                    .OrderBy(kvp => kvp.Value.LastAccessed)
                    .ThenBy(kvp => kvp.Value.AccessCount)
                    .Take(currentSize - targetSize)
                    .Select(kvp => kvp.Key)
                    .ToList();

                var removedCount = 0;
                foreach (var key in itemsToRemove)
                {
                    if (_cache.TryRemove(key, out _))
                    {
                        removedCount++;
                        Interlocked.Increment(ref _evictionCount);
                    }
                }

                _monitor?.Log($"응급 캐시 정리: {removedCount}개 항목 제거 (LRU)", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor?.Log($"응급 캐시 정리 중 오류: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 만료된 단일 항목 제거
        /// </summary>
        private void RemoveExpiredEntry(TKey key)
        {
            try
            {
                _lock.EnterWriteLock();
                try
                {
                    if (_cache.TryGetValue(key, out var entry) && entry.IsExpired)
                    {
                        _cache.TryRemove(key, out _);
                        Interlocked.Increment(ref _evictionCount);
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"만료 항목 제거 중 오류 ({key}): {ex.Message}", LogLevel.Debug);
            }
        }

        #endregion

        #region IDisposable

        private bool _disposed = false;

        public void Dispose()
        {
            if (!_disposed)
            {
                _cleanupTimer?.Dispose();
                _lock?.Dispose();
                
                var stats = GetStatistics();
                _monitor?.Log($"지능형 캐시 종료 - 히트율: {stats.HitRate:F1}%, 총 요청: {stats.HitCount + stats.MissCount}", LogLevel.Info);
                
                _disposed = true;
            }
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// 캐시 엔트리 클래스
    /// </summary>
    public class CacheEntry<T>
    {
        public T Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessed { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int AccessCount { get; set; }

        public bool IsExpired => DateTime.Now > ExpiresAt;
    }

    /// <summary>
    /// 캐시 통계 클래스
    /// </summary>
    public class CacheStatistics
    {
        public long HitCount { get; set; }
        public long MissCount { get; set; }
        public double HitRate { get; set; }
        public long EvictionCount { get; set; }
        public int CurrentSize { get; set; }
        public int MaxSize { get; set; }
        public DateTime LastCleanup { get; set; }
        public int ExpiredEntries { get; set; }
    }

    #endregion
}
