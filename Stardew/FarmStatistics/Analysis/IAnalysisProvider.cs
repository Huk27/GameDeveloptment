using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FarmStatistics.Analysis
{
    /// <summary>
    /// LookupAnything 패턴을 적용한 데이터 분석 제공자 인터페이스 (Phase 3.2)
    /// </summary>
    public interface IAnalysisProvider<T>
    {
        /// <summary>
        /// 분석 데이터를 가져옵니다.
        /// </summary>
        Task<T> GetAnalysisAsync(string key, AnalysisParameters parameters = null);
        
        /// <summary>
        /// 특정 키의 분석 데이터가 있는지 확인합니다.
        /// </summary>
        bool HasAnalysis(string key);
        
        /// <summary>
        /// 분석 캐시를 무효화합니다.
        /// </summary>
        void InvalidateCache(string key = null);
        
        /// <summary>
        /// 분석 제공자가 지원하는 분석 타입들을 반환합니다.
        /// </summary>
        IEnumerable<string> GetSupportedAnalysisTypes();
    }

    /// <summary>
    /// 기본 분석 제공자 추상 클래스
    /// </summary>
    public abstract class BaseAnalysisProvider<T> : IAnalysisProvider<T>
    {
        protected readonly Dictionary<string, Func<AnalysisParameters, Task<T>>> _analysisFactories;
        protected readonly Dictionary<string, T> _cache;
        protected readonly Dictionary<string, DateTime> _cacheTimestamps;
        protected readonly TimeSpan _cacheExpiry;

        protected BaseAnalysisProvider(TimeSpan? cacheExpiry = null)
        {
            _analysisFactories = new Dictionary<string, Func<AnalysisParameters, Task<T>>>();
            _cache = new Dictionary<string, T>();
            _cacheTimestamps = new Dictionary<string, DateTime>();
            _cacheExpiry = cacheExpiry ?? TimeSpan.FromMinutes(5);
            
            RegisterAnalysisFactories();
        }

        /// <summary>
        /// 분석 팩토리들을 등록합니다.
        /// </summary>
        protected abstract void RegisterAnalysisFactories();

        /// <summary>
        /// 분석 데이터를 가져옵니다.
        /// </summary>
        public virtual async Task<T> GetAnalysisAsync(string key, AnalysisParameters parameters = null)
        {
            try
            {
                // 캐시 확인
                if (TryGetCachedAnalysis(key, out var cachedResult))
                {
                    return cachedResult;
                }

                // 분석 팩토리 확인
                if (!_analysisFactories.TryGetValue(key, out var factory))
                {
                    throw new ArgumentException($"지원하지 않는 분석 타입: {key}");
                }

                // 분석 실행
                var result = await factory(parameters ?? new AnalysisParameters());
                
                // 캐시에 저장
                CacheAnalysis(key, result);
                
                return result;
            }
            catch (Exception ex)
            {
                throw new AnalysisException($"분석 실행 중 오류 ({key}): {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 특정 키의 분석 데이터가 있는지 확인합니다.
        /// </summary>
        public virtual bool HasAnalysis(string key)
        {
            return _analysisFactories.ContainsKey(key);
        }

        /// <summary>
        /// 분석 캐시를 무효화합니다.
        /// </summary>
        public virtual void InvalidateCache(string key = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                _cache.Clear();
                _cacheTimestamps.Clear();
            }
            else
            {
                _cache.Remove(key);
                _cacheTimestamps.Remove(key);
            }
        }

        /// <summary>
        /// 분석 제공자가 지원하는 분석 타입들을 반환합니다.
        /// </summary>
        public virtual IEnumerable<string> GetSupportedAnalysisTypes()
        {
            return _analysisFactories.Keys;
        }

        /// <summary>
        /// 캐시된 분석 결과를 가져옵니다.
        /// </summary>
        protected bool TryGetCachedAnalysis(string key, out T result)
        {
            result = default(T);
            
            if (!_cache.TryGetValue(key, out result))
                return false;
                
            if (!_cacheTimestamps.TryGetValue(key, out var timestamp))
                return false;
                
            if (DateTime.Now - timestamp > _cacheExpiry)
            {
                _cache.Remove(key);
                _cacheTimestamps.Remove(key);
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// 분석 결과를 캐시에 저장합니다.
        /// </summary>
        protected void CacheAnalysis(string key, T result)
        {
            _cache[key] = result;
            _cacheTimestamps[key] = DateTime.Now;
        }
    }

    /// <summary>
    /// 분석 파라미터 클래스
    /// </summary>
    public class AnalysisParameters
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeRange TimeRange { get; set; } = TimeRange.Last30Days;
        public Dictionary<string, object> CustomParameters { get; set; } = new();
        
        public T GetParameter<T>(string key, T defaultValue = default(T))
        {
            if (CustomParameters.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }
        
        public void SetParameter<T>(string key, T value)
        {
            CustomParameters[key] = value;
        }
    }

    /// <summary>
    /// 시간 범위 열거형
    /// </summary>
    public enum TimeRange
    {
        Today,
        Yesterday,
        Last7Days,
        Last14Days,
        Last30Days,
        CurrentSeason,
        PreviousSeason,
        CurrentYear,
        AllTime,
        Custom
    }

    /// <summary>
    /// 분석 예외 클래스
    /// </summary>
    public class AnalysisException : Exception
    {
        public AnalysisException(string message) : base(message) { }
        public AnalysisException(string message, Exception innerException) : base(message, innerException) { }
    }
}
