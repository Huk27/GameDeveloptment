using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace FarmStatistics.Analysis
{
    /// <summary>
    /// Phase 3.2 고급 데이터 분석 매니저
    /// 모든 분석 도구를 통합하고 관리하는 중앙 집중식 분석 시스템
    /// </summary>
    public class AdvancedAnalysisManager : IDisposable
    {
        #region Fields

        private readonly IMonitor _monitor;
        private readonly GameDataCollector _dataCollector;
        private readonly FarmTrendAnalyzer _trendAnalyzer;
        private readonly FarmComparisonTool _comparisonTool;
        
        // 분석 제공자 레지스트리
        private readonly Dictionary<string, IAnalysisProvider<object>> _analysisProviders;
        
        // 분석 결과 캐시
        private readonly Dictionary<string, AnalysisCache> _analysisCache;
        private readonly TimeSpan _defaultCacheExpiry = TimeSpan.FromMinutes(5);

        #endregion

        #region Constructor

        public AdvancedAnalysisManager(IMonitor monitor, GameDataCollector dataCollector)
        {
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            _dataCollector = dataCollector ?? throw new ArgumentNullException(nameof(dataCollector));
            
            // 분석 도구들 초기화
            _trendAnalyzer = new FarmTrendAnalyzer(_monitor, _dataCollector);
            _comparisonTool = new FarmComparisonTool(_monitor, _dataCollector);
            
            // 분석 제공자 레지스트리 초기화
            _analysisProviders = new Dictionary<string, IAnalysisProvider<object>>();
            _analysisCache = new Dictionary<string, AnalysisCache>();
            
            RegisterAnalysisProviders();
            
            _monitor.Log("고급 데이터 분석 매니저 초기화 완료", LogLevel.Debug);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 트렌드 분석을 실행합니다.
        /// </summary>
        public async Task<TrendAnalysisResult> AnalyzeTrendAsync(string analysisType, AnalysisParameters parameters = null)
        {
            try
            {
                var cacheKey = $"trend_{analysisType}_{GetParametersHash(parameters)}";
                
                // 캐시 확인
                if (TryGetCachedResult<TrendAnalysisResult>(cacheKey, out var cachedResult))
                {
                    _monitor?.Log($"트렌드 분석 캐시 히트: {analysisType}", LogLevel.Trace);
                    return cachedResult;
                }
                
                // 분석 실행
                var result = await _trendAnalyzer.GetAnalysisAsync(analysisType, parameters ?? new AnalysisParameters());
                
                // 캐시에 저장
                CacheResult(cacheKey, result, TimeSpan.FromMinutes(10));
                
                _monitor?.Log($"트렌드 분석 완료: {analysisType}", LogLevel.Debug);
                return result;
            }
            catch (Exception ex)
            {
                _monitor?.Log($"트렌드 분석 중 오류 ({analysisType}): {ex.Message}", LogLevel.Error);
                throw new AnalysisException($"트렌드 분석 실패: {analysisType}", ex);
            }
        }

        /// <summary>
        /// 비교 분석을 실행합니다.
        /// </summary>
        public async Task<ComparisonResult> AnalyzeComparisonAsync(string comparisonType, AnalysisParameters parameters = null)
        {
            try
            {
                var cacheKey = $"comparison_{comparisonType}_{GetParametersHash(parameters)}";
                
                // 캐시 확인
                if (TryGetCachedResult<ComparisonResult>(cacheKey, out var cachedResult))
                {
                    _monitor?.Log($"비교 분석 캐시 히트: {comparisonType}", LogLevel.Trace);
                    return cachedResult;
                }
                
                // 분석 실행
                var result = await _comparisonTool.GetAnalysisAsync(comparisonType, parameters ?? new AnalysisParameters());
                
                // 캐시에 저장
                CacheResult(cacheKey, result, TimeSpan.FromMinutes(15));
                
                _monitor?.Log($"비교 분석 완료: {comparisonType}", LogLevel.Debug);
                return result;
            }
            catch (Exception ex)
            {
                _monitor?.Log($"비교 분석 중 오류 ({comparisonType}): {ex.Message}", LogLevel.Error);
                throw new AnalysisException($"비교 분석 실패: {comparisonType}", ex);
            }
        }

        /// <summary>
        /// 종합 분석 대시보드를 생성합니다.
        /// </summary>
        public async Task<AnalysisDashboard> GenerateDashboardAsync()
        {
            try
            {
                var cacheKey = "dashboard_comprehensive";
                
                // 캐시 확인
                if (TryGetCachedResult<AnalysisDashboard>(cacheKey, out var cachedDashboard))
                {
                    _monitor?.Log("종합 분석 대시보드 캐시 히트", LogLevel.Trace);
                    return cachedDashboard;
                }
                
                // 병렬로 다양한 분석 실행
                var analysisParameters = new AnalysisParameters { TimeRange = TimeRange.Last30Days };
                
                // 병렬 분석 실행
                var profitTask = AnalyzeTrendAsync("profit_trend", analysisParameters);
                var productionTask = AnalyzeTrendAsync("production_trend", analysisParameters);
                var efficiencyTask = AnalyzeTrendAsync("efficiency_trend", analysisParameters);
                var comparisonTask = AnalyzeComparisonAsync("comprehensive_comparison", analysisParameters);
                
                await Task.WhenAll(profitTask, productionTask, efficiencyTask, comparisonTask);
                
                var profitTrend = await profitTask;
                var productionTrend = await productionTask;
                var efficiencyTrend = await efficiencyTask;
                var comprehensiveComparison = await comparisonTask;
                
                // 대시보드 생성
                var dashboard = new AnalysisDashboard
                {
                    GeneratedAt = DateTime.Now,
                    TimeRange = "최근 30일",
                    OverallScore = CalculateOverallScore(profitTrend, productionTrend, efficiencyTrend, comprehensiveComparison),
                    TrendAnalyses = new List<TrendAnalysisResult> { profitTrend, productionTrend, efficiencyTrend },
                    ComparisonResults = new List<ComparisonResult> { comprehensiveComparison },
                    KeyInsights = GenerateKeyInsights(profitTrend, productionTrend, efficiencyTrend, comprehensiveComparison),
                    ActionableRecommendations = GenerateActionableRecommendations(profitTrend, productionTrend, efficiencyTrend, comprehensiveComparison),
                    PerformanceMetrics = await GeneratePerformanceMetrics()
                };
                
                // 캐시에 저장
                CacheResult(cacheKey, dashboard, TimeSpan.FromMinutes(20));
                
                _monitor?.Log("종합 분석 대시보드 생성 완료", LogLevel.Debug);
                return dashboard;
            }
            catch (Exception ex)
            {
                _monitor?.Log($"대시보드 생성 중 오류: {ex.Message}", LogLevel.Error);
                throw new AnalysisException("대시보드 생성 실패", ex);
            }
        }

        /// <summary>
        /// 일일 데이터를 기록합니다.
        /// </summary>
        public async Task RecordDailyDataAsync()
        {
            try
            {
                var dailyData = await _trendAnalyzer.GenerateCurrentDailyData();
                _trendAnalyzer.RecordDailyData(dailyData);
                
                // 관련 캐시 무효화
                InvalidateCache("trend_");
                InvalidateCache("dashboard_");
                
                _monitor?.Log($"일일 데이터 기록 완료: {dailyData.Date:yyyy-MM-dd}", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor?.Log($"일일 데이터 기록 중 오류: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 사용 가능한 분석 타입들을 반환합니다.
        /// </summary>
        public Dictionary<string, List<string>> GetAvailableAnalysisTypes()
        {
            return new Dictionary<string, List<string>>
            {
                ["트렌드 분석"] = _trendAnalyzer.GetSupportedAnalysisTypes().ToList(),
                ["비교 분석"] = _comparisonTool.GetSupportedAnalysisTypes().ToList()
            };
        }

        /// <summary>
        /// 분석 캐시를 무효화합니다.
        /// </summary>
        public void InvalidateCache(string keyPrefix = null)
        {
            try
            {
                if (string.IsNullOrEmpty(keyPrefix))
                {
                    _analysisCache.Clear();
                    _trendAnalyzer.InvalidateCache();
                    _comparisonTool.InvalidateCache();
                    _monitor?.Log("전체 분석 캐시 무효화", LogLevel.Debug);
                }
                else
                {
                    var keysToRemove = _analysisCache.Keys
                        .Where(key => key.StartsWith(keyPrefix))
                        .ToList();
                    
                    foreach (var key in keysToRemove)
                    {
                        _analysisCache.Remove(key);
                    }
                    
                    _monitor?.Log($"분석 캐시 무효화: {keyPrefix}* ({keysToRemove.Count}개)", LogLevel.Debug);
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"캐시 무효화 중 오류: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 분석 성능 통계를 반환합니다.
        /// </summary>
        public AnalysisPerformanceStats GetPerformanceStats()
        {
            var cacheHits = _analysisCache.Values.Sum(cache => cache.HitCount);
            var totalRequests = _analysisCache.Values.Sum(cache => cache.HitCount + cache.MissCount);
            var hitRate = totalRequests > 0 ? (double)cacheHits / totalRequests * 100 : 0;
            
            return new AnalysisPerformanceStats
            {
                TotalCachedAnalyses = _analysisCache.Count,
                CacheHitRate = hitRate,
                TotalRequests = totalRequests,
                AverageAnalysisTime = CalculateAverageAnalysisTime(),
                LastCacheCleanup = DateTime.Now // 실제로는 마지막 정리 시간 추적 필요
            };
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 분석 제공자들을 등록합니다.
        /// </summary>
        private void RegisterAnalysisProviders()
        {
            // 트렌드 분석 제공자 등록
            foreach (var analysisType in _trendAnalyzer.GetSupportedAnalysisTypes())
            {
                _analysisProviders[$"trend_{analysisType}"] = new AnalysisProviderAdapter<TrendAnalysisResult>(_trendAnalyzer);
            }
            
            // 비교 분석 제공자 등록
            foreach (var analysisType in _comparisonTool.GetSupportedAnalysisTypes())
            {
                _analysisProviders[$"comparison_{analysisType}"] = new AnalysisProviderAdapter<ComparisonResult>(_comparisonTool);
            }
        }

        /// <summary>
        /// 캐시된 결과를 가져옵니다.
        /// </summary>
        private bool TryGetCachedResult<T>(string cacheKey, out T result)
        {
            result = default(T);
            
            if (!_analysisCache.TryGetValue(cacheKey, out var cache))
                return false;
                
            if (DateTime.Now - cache.CachedAt > cache.Expiry)
            {
                _analysisCache.Remove(cacheKey);
                cache.MissCount++;
                return false;
            }
            
            if (cache.Result is T typedResult)
            {
                result = typedResult;
                cache.HitCount++;
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// 결과를 캐시에 저장합니다.
        /// </summary>
        private void CacheResult<T>(string cacheKey, T result, TimeSpan expiry)
        {
            _analysisCache[cacheKey] = new AnalysisCache
            {
                Result = result,
                CachedAt = DateTime.Now,
                Expiry = expiry,
                HitCount = 0,
                MissCount = 0
            };
        }

        /// <summary>
        /// 파라미터의 해시를 생성합니다.
        /// </summary>
        private string GetParametersHash(AnalysisParameters parameters)
        {
            if (parameters == null)
                return "default";
                
            var hash = $"{parameters.TimeRange}_{parameters.StartDate}_{parameters.EndDate}";
            foreach (var param in parameters.CustomParameters)
            {
                hash += $"_{param.Key}_{param.Value}";
            }
            
            return hash.GetHashCode().ToString("X");
        }

        /// <summary>
        /// 전체 점수를 계산합니다.
        /// </summary>
        private double CalculateOverallScore(TrendAnalysisResult profitTrend, TrendAnalysisResult productionTrend, 
            TrendAnalysisResult efficiencyTrend, ComparisonResult comprehensiveComparison)
        {
            try
            {
                // 트렌드 점수 (0-100)
                var profitScore = CalculateTrendScore(profitTrend);
                var productionScore = CalculateTrendScore(productionTrend);
                var efficiencyScore = CalculateTrendScore(efficiencyTrend);
                
                // 비교 점수 (0-100)
                var comparisonScore = CalculateComparisonScore(comprehensiveComparison);
                
                // 가중 평균 (트렌드 60%, 비교 40%)
                var overallScore = (profitScore * 0.25 + productionScore * 0.2 + efficiencyScore * 0.15) * 0.6 +
                                   comparisonScore * 0.4;
                
                return Math.Max(0, Math.Min(100, overallScore));
            }
            catch
            {
                return 50; // 기본값
            }
        }

        /// <summary>
        /// 트렌드 점수를 계산합니다.
        /// </summary>
        private double CalculateTrendScore(TrendAnalysisResult trend)
        {
            if (trend == null) return 50;
            
            var baseScore = 50.0;
            
            // 트렌드 방향 점수
            switch (trend.TrendDirection)
            {
                case TrendDirection.Increasing:
                    baseScore += 25;
                    break;
                case TrendDirection.Decreasing:
                    baseScore -= 25;
                    break;
            }
            
            // 트렌드 강도 점수
            switch (trend.TrendStrength)
            {
                case TrendStrength.Strong:
                    baseScore += 15;
                    break;
                case TrendStrength.Medium:
                    baseScore += 5;
                    break;
                case TrendStrength.Weak:
                    baseScore -= 5;
                    break;
            }
            
            // 신뢰도 점수
            baseScore += (trend.Confidence - 0.5) * 20;
            
            return Math.Max(0, Math.Min(100, baseScore));
        }

        /// <summary>
        /// 비교 점수를 계산합니다.
        /// </summary>
        private double CalculateComparisonScore(ComparisonResult comparison)
        {
            if (comparison == null) return 50;
            
            var baseScore = 50.0;
            
            // 비교 결과에 따른 점수
            baseScore += Math.Max(-30, Math.Min(30, comparison.PercentageDifference * 0.5));
            
            // 등급에 따른 점수
            switch (comparison.Rating)
            {
                case ComparisonRating.Excellent:
                    baseScore += 20;
                    break;
                case ComparisonRating.Good:
                    baseScore += 10;
                    break;
                case ComparisonRating.BelowAverage:
                    baseScore -= 10;
                    break;
                case ComparisonRating.Poor:
                    baseScore -= 20;
                    break;
            }
            
            return Math.Max(0, Math.Min(100, baseScore));
        }

        /// <summary>
        /// 핵심 인사이트를 생성합니다.
        /// </summary>
        private List<string> GenerateKeyInsights(TrendAnalysisResult profitTrend, TrendAnalysisResult productionTrend,
            TrendAnalysisResult efficiencyTrend, ComparisonResult comprehensiveComparison)
        {
            var insights = new List<string>();
            
            // 수익 트렌드 인사이트
            if (profitTrend != null)
            {
                if (profitTrend.TrendDirection == TrendDirection.Increasing && profitTrend.Confidence > 0.7)
                {
                    insights.Add($"수익이 안정적으로 증가하고 있습니다 (신뢰도: {profitTrend.Confidence:P0})");
                }
                else if (profitTrend.TrendDirection == TrendDirection.Decreasing)
                {
                    insights.Add("수익 감소 추세가 관찰됩니다. 전략 재검토가 필요할 수 있습니다.");
                }
            }
            
            // 생산량 트렌드 인사이트
            if (productionTrend != null && productionTrend.TrendDirection == TrendDirection.Increasing)
            {
                insights.Add("생산량이 꾸준히 증가하고 있어 농장 규모가 확장되고 있습니다.");
            }
            
            // 효율성 인사이트
            if (efficiencyTrend != null && efficiencyTrend.TrendDirection == TrendDirection.Decreasing)
            {
                insights.Add("효율성이 감소하고 있습니다. 자동화나 최적화를 고려해보세요.");
            }
            
            // 비교 분석 인사이트
            if (comprehensiveComparison != null)
            {
                if (comprehensiveComparison.Rating >= ComparisonRating.Good)
                {
                    insights.Add("전반적으로 벤치마크보다 우수한 성과를 보이고 있습니다.");
                }
                else if (comprehensiveComparison.Rating <= ComparisonRating.BelowAverage)
                {
                    insights.Add("벤치마크 대비 개선이 필요한 영역이 있습니다.");
                }
            }
            
            if (insights.Count == 0)
            {
                insights.Add("분석을 위한 충분한 데이터가 수집되면 더 자세한 인사이트를 제공할 수 있습니다.");
            }
            
            return insights;
        }

        /// <summary>
        /// 실행 가능한 권장사항을 생성합니다.
        /// </summary>
        private List<string> GenerateActionableRecommendations(TrendAnalysisResult profitTrend, TrendAnalysisResult productionTrend,
            TrendAnalysisResult efficiencyTrend, ComparisonResult comprehensiveComparison)
        {
            var recommendations = new List<string>();
            
            // 각 분석 결과에서 권장사항 수집
            if (profitTrend?.Recommendations != null)
                recommendations.AddRange(profitTrend.Recommendations.Take(2));
                
            if (productionTrend?.Recommendations != null)
                recommendations.AddRange(productionTrend.Recommendations.Take(1));
                
            if (efficiencyTrend?.Recommendations != null)
                recommendations.AddRange(efficiencyTrend.Recommendations.Take(1));
                
            if (comprehensiveComparison?.Recommendations != null)
                recommendations.AddRange(comprehensiveComparison.Recommendations.Take(2));
            
            // 중복 제거 및 최대 5개로 제한
            return recommendations.Distinct().Take(5).ToList();
        }

        /// <summary>
        /// 성능 메트릭을 생성합니다.
        /// </summary>
        private async Task<Dictionary<string, double>> GeneratePerformanceMetrics()
        {
            try
            {
                var currentFarm = await _comparisonTool.GetCurrentFarmData();
                
                return new Dictionary<string, double>
                {
                    ["총_수익"] = currentFarm.TotalEarnings,
                    ["작물_종류"] = currentFarm.CropTypes,
                    ["동물_종류"] = currentFarm.AnimalTypes,
                    ["플레이_시간"] = currentFarm.PlayTimeHours,
                    ["시간당_수익"] = currentFarm.PlayTimeHours > 0 ? currentFarm.TotalEarnings / currentFarm.PlayTimeHours : 0
                };
            }
            catch
            {
                return new Dictionary<string, double>();
            }
        }

        /// <summary>
        /// 평균 분석 시간을 계산합니다.
        /// </summary>
        private double CalculateAverageAnalysisTime()
        {
            // 실제로는 분석 시간을 추적해야 하지만 여기서는 추정값 반환
            return 250.0; // 250ms
        }

        #endregion

        #region IDisposable

        private bool _disposed = false;

        public void Dispose()
        {
            if (!_disposed)
            {
                // _trendAnalyzer와 _comparisonTool은 readonly이므로 null 할당 불가
                // 대신 가비지 컬렉션에 의존
                _analysisCache.Clear();
                
                var stats = GetPerformanceStats();
                _monitor?.Log($"고급 데이터 분석 매니저 종료 - 캐시 히트율: {stats.CacheHitRate:F1}%", LogLevel.Info);
                
                _disposed = true;
            }
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// 분석 캐시
    /// </summary>
    internal class AnalysisCache
    {
        public object Result { get; set; }
        public DateTime CachedAt { get; set; }
        public TimeSpan Expiry { get; set; }
        public int HitCount { get; set; }
        public int MissCount { get; set; }
    }

    /// <summary>
    /// 분석 제공자 어댑터
    /// </summary>
    internal class AnalysisProviderAdapter<T> : IAnalysisProvider<object>
    {
        private readonly IAnalysisProvider<T> _provider;

        public AnalysisProviderAdapter(IAnalysisProvider<T> provider)
        {
            _provider = provider;
        }

        public async Task<object> GetAnalysisAsync(string key, AnalysisParameters parameters = null)
        {
            return await _provider.GetAnalysisAsync(key, parameters);
        }

        public bool HasAnalysis(string key)
        {
            return _provider.HasAnalysis(key);
        }

        public void InvalidateCache(string key = null)
        {
            _provider.InvalidateCache(key);
        }

        public IEnumerable<string> GetSupportedAnalysisTypes()
        {
            return _provider.GetSupportedAnalysisTypes();
        }
    }

    /// <summary>
    /// 분석 대시보드
    /// </summary>
    public class AnalysisDashboard
    {
        public DateTime GeneratedAt { get; set; }
        public string TimeRange { get; set; } = "";
        public double OverallScore { get; set; }
        public List<TrendAnalysisResult> TrendAnalyses { get; set; } = new();
        public List<ComparisonResult> ComparisonResults { get; set; } = new();
        public List<string> KeyInsights { get; set; } = new();
        public List<string> ActionableRecommendations { get; set; } = new();
        public Dictionary<string, double> PerformanceMetrics { get; set; } = new();
    }

    /// <summary>
    /// 분석 성능 통계
    /// </summary>
    public class AnalysisPerformanceStats
    {
        public int TotalCachedAnalyses { get; set; }
        public double CacheHitRate { get; set; }
        public int TotalRequests { get; set; }
        public double AverageAnalysisTime { get; set; }
        public DateTime LastCacheCleanup { get; set; }
    }

    #endregion
}
