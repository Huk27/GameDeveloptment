using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StardewModdingAPI;
using FarmStatistics.Performance;

namespace FarmStatistics.Performance
{
    /// <summary>
    /// Phase 3.1 성능 최적화된 데이터 수집 매니저
    /// Automate + LookupAnything + ChestsAnywhere 패턴을 통합 적용
    /// </summary>
    public class OptimizedDataCollectionManager : IDisposable
    {
        #region Fields

        private readonly IMonitor _monitor;
        private readonly GameDataCollector _baseCollector;
        private readonly BatchDataProcessor _batchProcessor;
        private readonly IntelligentCache<string, FarmStatisticsData> _dataCache;
        private readonly IntelligentCache<string, object> _componentCache;
        
        // 캐시 키 상수
        private const string OVERVIEW_CACHE_KEY = "overview_data";
        private const string CROPS_CACHE_KEY = "crops_data";
        private const string ANIMALS_CACHE_KEY = "animals_data";
        private const string TIME_CACHE_KEY = "time_data";
        private const string GOALS_CACHE_KEY = "goals_data";
        
        // 성능 설정
        private readonly Dictionary<string, TimeSpan> _cacheExpiryTimes;
        private readonly Dictionary<string, bool> _dataTypePriority;
        
        #endregion

        #region Constructor

        public OptimizedDataCollectionManager(IMonitor monitor, GameDataCollector baseCollector)
        {
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            _baseCollector = baseCollector ?? throw new ArgumentNullException(nameof(baseCollector));
            
            // 배치 처리기 초기화
            _batchProcessor = new BatchDataProcessor(_monitor);
            _batchProcessor.OnBatchProcessed += OnBatchProcessed;
            
            // 캐시 시스템 초기화
            _dataCache = new IntelligentCache<string, FarmStatisticsData>(
                _monitor, 
                TimeSpan.FromMinutes(2), // 기본 2분 캐시
                maxCacheSize: 100
            );
            
            _componentCache = new IntelligentCache<string, object>(
                _monitor,
                TimeSpan.FromMinutes(1), // 컴포넌트는 1분 캐시
                maxCacheSize: 500
            );
            
            // 데이터 타입별 캐시 설정
            _cacheExpiryTimes = new Dictionary<string, TimeSpan>
            {
                [OVERVIEW_CACHE_KEY] = TimeSpan.FromSeconds(30), // 개요는 자주 업데이트
                [CROPS_CACHE_KEY] = TimeSpan.FromMinutes(2),     // 작물은 중간 주기
                [ANIMALS_CACHE_KEY] = TimeSpan.FromMinutes(3),   // 동물은 느린 주기
                [TIME_CACHE_KEY] = TimeSpan.FromMinutes(5),      // 시간 통계는 가장 느림
                [GOALS_CACHE_KEY] = TimeSpan.FromMinutes(10)     // 목표는 매우 느림
            };
            
            // 데이터 우선순위 설정
            _dataTypePriority = new Dictionary<string, bool>
            {
                [OVERVIEW_CACHE_KEY] = true,  // 높은 우선순위
                [CROPS_CACHE_KEY] = true,     // 높은 우선순위
                [ANIMALS_CACHE_KEY] = false,  // 낮은 우선순위
                [TIME_CACHE_KEY] = false,     // 낮은 우선순위
                [GOALS_CACHE_KEY] = false     // 낮은 우선순위
            };
            
            _monitor.Log("최적화된 데이터 수집 매니저 초기화 완료", LogLevel.Debug);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 최적화된 데이터 수집 (메인 메서드)
        /// </summary>
        public async Task<FarmStatisticsData> CollectDataAsync(bool forceRefresh = false)
        {
            try
            {
                // 1. 캐시 확인 (가장 빠른 경로)
                if (!forceRefresh && TryGetCachedCompleteData(out var cachedData))
                {
                    _monitor?.Log("완전 캐시 히트 - 즉시 반환", LogLevel.Trace);
                    return cachedData;
                }
                
                // 2. 부분 캐시 + 배치 처리
                var result = await CollectWithPartialCache();
                
                // 3. 결과 캐시에 저장
                CacheCompleteData(result);
                
                return result;
            }
            catch (Exception ex)
            {
                _monitor?.Log($"최적화된 데이터 수집 중 오류: {ex.Message}", LogLevel.Error);
                
                // 폴백: 기본 수집기 사용
                return _baseCollector?.CollectCurrentData() ?? new FarmStatisticsData();
            }
        }

        /// <summary>
        /// 특정 데이터 타입만 수집 (선택적 업데이트)
        /// </summary>
        public async Task<T> CollectSpecificDataAsync<T>(string dataType) where T : class
        {
            try
            {
                var cacheKey = $"specific_{dataType}";
                
                // 캐시 확인
                if (_componentCache.TryGetValue(cacheKey, out var cachedValue) && cachedValue is T cached)
                {
                    return cached;
                }
                
                // 배치 처리로 수집
                var task = CreateDataCollectionTask(dataType);
                _batchProcessor.EnqueueTask(task);
                
                // 결과 대기 (최대 5초)
                var result = await WaitForTaskResult<T>(task.TaskId, TimeSpan.FromSeconds(5));
                
                if (result != null)
                {
                    _componentCache.Set(cacheKey, result, _cacheExpiryTimes.GetValueOrDefault(dataType, TimeSpan.FromMinutes(1)));
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _monitor?.Log($"특정 데이터 수집 중 오류 ({dataType}): {ex.Message}", LogLevel.Error);
                return null;
            }
        }

        /// <summary>
        /// 캐시 무효화 (데이터 변경 시 호출)
        /// </summary>
        public void InvalidateCache(string dataType = null)
        {
            try
            {
                if (string.IsNullOrEmpty(dataType))
                {
                    // 전체 캐시 무효화
                    _dataCache.Clear();
                    _componentCache.Clear();
                    _monitor?.Log("전체 캐시 무효화", LogLevel.Debug);
                }
                else
                {
                    // 특정 데이터 타입 무효화
                    _dataCache.InvalidatePattern(key => key.Contains(dataType));
                    _componentCache.InvalidatePattern(key => key.Contains(dataType));
                    _monitor?.Log($"캐시 무효화: {dataType}", LogLevel.Debug);
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"캐시 무효화 중 오류: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 성능 통계 가져오기
        /// </summary>
        public PerformanceStatistics GetPerformanceStats()
        {
            var dataStats = _dataCache.GetStatistics();
            var componentStats = _componentCache.GetStatistics();
            var batchStats = _batchProcessor.GetStats();
            
            return new PerformanceStatistics
            {
                DataCacheStats = dataStats,
                ComponentCacheStats = componentStats,
                BatchProcessingStats = batchStats,
                TotalMemoryUsage = GC.GetTotalMemory(false),
                LastUpdateTime = DateTime.Now
            };
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 완전 캐시 데이터 확인
        /// </summary>
        private bool TryGetCachedCompleteData(out FarmStatisticsData data)
        {
            const string completeDataKey = "complete_farm_data";
            return _dataCache.TryGetValue(completeDataKey, out data);
        }

        /// <summary>
        /// 부분 캐시 + 배치 처리로 데이터 수집
        /// </summary>
        private async Task<FarmStatisticsData> CollectWithPartialCache()
        {
            var result = new FarmStatisticsData();
            var tasks = new List<DataCollectionTask>();
            
            // 1. 각 데이터 타입별로 캐시 확인 및 작업 생성
            if (!TryGetCachedComponent(OVERVIEW_CACHE_KEY, out result.OverviewData))
            {
                tasks.Add(CreateDataCollectionTask(OVERVIEW_CACHE_KEY));
            }
            
            if (!TryGetCachedComponent(CROPS_CACHE_KEY, out result.CropStatistics))
            {
                tasks.Add(CreateDataCollectionTask(CROPS_CACHE_KEY));
            }
            
            if (!TryGetCachedComponent(ANIMALS_CACHE_KEY, out result.AnimalStatistics))
            {
                tasks.Add(CreateDataCollectionTask(ANIMALS_CACHE_KEY));
            }
            
            if (!TryGetCachedComponent(TIME_CACHE_KEY, out result.TimeStatistics))
            {
                tasks.Add(CreateDataCollectionTask(TIME_CACHE_KEY));
            }
            
            if (!TryGetCachedComponent(GOALS_CACHE_KEY, out result.GoalStatistics))
            {
                tasks.Add(CreateDataCollectionTask(GOALS_CACHE_KEY));
            }
            
            // 2. 필요한 작업들을 배치로 처리
            if (tasks.Any())
            {
                // 우선순위별로 정렬
                var prioritizedTasks = tasks
                    .OrderByDescending(t => _dataTypePriority.GetValueOrDefault(t.Parameters["dataType"]?.ToString(), false))
                    .ToList();
                
                _batchProcessor.EnqueueTasks(prioritizedTasks);
                
                // 결과 대기
                await WaitForTasksCompletion(prioritizedTasks, TimeSpan.FromSeconds(10));
                
                // 3. 결과를 result에 병합
                await MergeTaskResults(result, prioritizedTasks);
            }
            
            return result;
        }

        /// <summary>
        /// 캐시된 컴포넌트 데이터 확인
        /// </summary>
        private bool TryGetCachedComponent<T>(string cacheKey, out T data) where T : class
        {
            if (_componentCache.TryGetValue(cacheKey, out var cachedValue) && cachedValue is T typedValue)
            {
                data = typedValue;
                return true;
            }
            
            data = null;
            return false;
        }

        /// <summary>
        /// 데이터 수집 작업 생성
        /// </summary>
        private DataCollectionTask CreateDataCollectionTask(string dataType)
        {
            return new DataCollectionTask
            {
                TaskType = GetTaskType(dataType),
                Execute = () => ExecuteDataCollection(dataType),
                Parameters = new Dictionary<string, object> { ["dataType"] = dataType }
            };
        }

        /// <summary>
        /// 데이터 타입에서 작업 타입 변환
        /// </summary>
        private DataCollectionTaskType GetTaskType(string dataType)
        {
            return dataType switch
            {
                OVERVIEW_CACHE_KEY => DataCollectionTaskType.OverviewData,
                CROPS_CACHE_KEY => DataCollectionTaskType.CropData,
                ANIMALS_CACHE_KEY => DataCollectionTaskType.AnimalData,
                TIME_CACHE_KEY => DataCollectionTaskType.TimeData,
                GOALS_CACHE_KEY => DataCollectionTaskType.GoalData,
                _ => DataCollectionTaskType.OverviewData
            };
        }

        /// <summary>
        /// 실제 데이터 수집 실행
        /// </summary>
        private object ExecuteDataCollection(string dataType)
        {
            try
            {
                // 기본 수집기에서 해당 데이터만 수집
                var fullData = _baseCollector.CollectCurrentData();
                
                return dataType switch
                {
                    OVERVIEW_CACHE_KEY => fullData.OverviewData,
                    CROPS_CACHE_KEY => fullData.CropStatistics,
                    ANIMALS_CACHE_KEY => fullData.AnimalStatistics,
                    TIME_CACHE_KEY => fullData.TimeStatistics,
                    GOALS_CACHE_KEY => fullData.GoalStatistics,
                    _ => null
                };
            }
            catch (Exception ex)
            {
                _monitor?.Log($"데이터 수집 실행 중 오류 ({dataType}): {ex.Message}", LogLevel.Error);
                return null;
            }
        }

        /// <summary>
        /// 작업 완료 대기
        /// </summary>
        private async Task WaitForTasksCompletion(List<DataCollectionTask> tasks, TimeSpan timeout)
        {
            // 실제 구현에서는 더 정교한 대기 메커니즘이 필요
            // 여기서는 단순화된 버전
            await Task.Delay(Math.Min((int)timeout.TotalMilliseconds, 2000));
        }

        /// <summary>
        /// 특정 작업 결과 대기
        /// </summary>
        private async Task<T> WaitForTaskResult<T>(string taskId, TimeSpan timeout) where T : class
        {
            // 실제 구현에서는 TaskCompletionSource 사용
            await Task.Delay(Math.Min((int)timeout.TotalMilliseconds, 1000));
            return null; // 임시 구현
        }

        /// <summary>
        /// 작업 결과를 최종 데이터에 병합
        /// </summary>
        private async Task MergeTaskResults(FarmStatisticsData result, List<DataCollectionTask> tasks)
        {
            // 실제 구현에서는 배치 처리 결과를 받아서 병합
            // 여기서는 단순화된 버전
            await Task.CompletedTask;
        }

        /// <summary>
        /// 완전한 데이터를 캐시에 저장
        /// </summary>
        private void CacheCompleteData(FarmStatisticsData data)
        {
            try
            {
                const string completeDataKey = "complete_farm_data";
                _dataCache.Set(completeDataKey, data, TimeSpan.FromMinutes(1));
                
                // 컴포넌트별 캐시도 업데이트
                if (data.OverviewData != null)
                    _componentCache.Set(OVERVIEW_CACHE_KEY, data.OverviewData, _cacheExpiryTimes[OVERVIEW_CACHE_KEY]);
                    
                if (data.CropStatistics != null)
                    _componentCache.Set(CROPS_CACHE_KEY, data.CropStatistics, _cacheExpiryTimes[CROPS_CACHE_KEY]);
                    
                if (data.AnimalStatistics != null)
                    _componentCache.Set(ANIMALS_CACHE_KEY, data.AnimalStatistics, _cacheExpiryTimes[ANIMALS_CACHE_KEY]);
                    
                if (data.TimeStatistics != null)
                    _componentCache.Set(TIME_CACHE_KEY, data.TimeStatistics, _cacheExpiryTimes[TIME_CACHE_KEY]);
                    
                if (data.GoalStatistics != null)
                    _componentCache.Set(GOALS_CACHE_KEY, data.GoalStatistics, _cacheExpiryTimes[GOALS_CACHE_KEY]);
            }
            catch (Exception ex)
            {
                _monitor?.Log($"완전 데이터 캐싱 중 오류: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 배치 처리 완료 이벤트 핸들러
        /// </summary>
        private void OnBatchProcessed(BatchProcessedEventArgs args)
        {
            try
            {
                _monitor?.Log($"배치 처리 완료: {args.ProcessedTasks}개 작업, {args.ProcessingTimeMs:F1}ms", LogLevel.Trace);
                
                // 성능 모니터링
                if (args.ProcessingTimeMs > 200) // 200ms 초과 시 경고
                {
                    _monitor?.Log($"배치 처리 성능 경고: {args.ProcessingTimeMs:F1}ms (임계값: 200ms)", LogLevel.Warn);
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"배치 처리 이벤트 핸들링 중 오류: {ex.Message}", LogLevel.Error);
            }
        }

        #endregion

        #region IDisposable

        private bool _disposed = false;

        public void Dispose()
        {
            if (!_disposed)
            {
                _batchProcessor?.Dispose();
                _dataCache?.Dispose();
                _componentCache?.Dispose();
                
                var stats = GetPerformanceStats();
                _monitor?.Log($"최적화된 데이터 수집 매니저 종료 - 데이터캐시 히트율: {stats.DataCacheStats.HitRate:F1}%", LogLevel.Info);
                
                _disposed = true;
            }
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// 성능 통계 클래스
    /// </summary>
    public class PerformanceStatistics
    {
        public CacheStatistics DataCacheStats { get; set; }
        public CacheStatistics ComponentCacheStats { get; set; }
        public BatchProcessingStats BatchProcessingStats { get; set; }
        public long TotalMemoryUsage { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }

    #endregion
}
