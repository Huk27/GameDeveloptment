using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;
using System.Threading;

namespace FarmStatistics.Analysis
{
    /// <summary>
    /// LookupAnything 패턴을 적용한 농장 트렌드 분석기 (Phase 3.2)
    /// 수익, 생산량, 효율성 등의 트렌드를 분석하고 예측합니다.
    /// </summary>
    public class FarmTrendAnalyzer : BaseAnalysisProvider<TrendAnalysisResult>
    {
        #region Fields

        private readonly IMonitor _monitor;
        private readonly GameDataCollector _dataCollector;
        private readonly Dictionary<string, List<DailyFarmData>> _historicalData;
        private readonly SemaphoreSlim _dataLock = new(1, 1);

        #endregion

        #region Constructor

        public FarmTrendAnalyzer(IMonitor monitor, GameDataCollector dataCollector) 
            : base(TimeSpan.FromMinutes(10)) // 트렌드 분석은 10분 캐시
        {
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            _dataCollector = dataCollector ?? throw new ArgumentNullException(nameof(dataCollector));
            _historicalData = new Dictionary<string, List<DailyFarmData>>();
            
            _monitor.Log("농장 트렌드 분석기 초기화 완료", LogLevel.Debug);
        }

        #endregion

        #region Analysis Factory Registration

        /// <summary>
        /// 분석 팩토리들을 등록합니다.
        /// </summary>
        protected override void RegisterAnalysisFactories()
        {
            // 수익 트렌드 분석
            _analysisFactories["profit_trend"] = async (parameters) => 
                await AnalyzeProfitTrend(parameters);
                
            // 생산량 트렌드 분석
            _analysisFactories["production_trend"] = async (parameters) => 
                await AnalyzeProductionTrend(parameters);
                
            // 효율성 트렌드 분석
            _analysisFactories["efficiency_trend"] = async (parameters) => 
                await AnalyzeEfficiencyTrend(parameters);
                
            // 계절별 비교 분석
            _analysisFactories["seasonal_comparison"] = async (parameters) => 
                await AnalyzeSeasonalComparison(parameters);
                
            // 예측 분석
            _analysisFactories["prediction"] = async (parameters) => 
                await AnalyzePrediction(parameters);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 일일 데이터를 기록합니다.
        /// </summary>
        public void RecordDailyData(DailyFarmData dailyData)
        {
            try
            {
                var dateKey = dailyData.Date.ToString("yyyy-MM-dd");
                lock (_dataLock)
                {
                    
                    if (!_historicalData.ContainsKey(dateKey))
                    {
                        _historicalData[dateKey] = new List<DailyFarmData>();
                    }
                    
                    _historicalData[dateKey].Add(dailyData);
                    
                    // 90일 이상 된 데이터는 제거 (메모리 관리)
                    var cutoffDate = DateTime.Now.AddDays(-90);
                    var keysToRemove = _historicalData.Keys
                        .Where(key => DateTime.Parse(key) < cutoffDate)
                        .ToList();
                    
                    foreach (var key in keysToRemove)
                    {
                        _historicalData.Remove(key);
                    }
                }
                
                // 새 데이터가 추가되면 캐시 무효화
                InvalidateCache();
                
                _monitor?.Log($"일일 데이터 기록: {dateKey}", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                _monitor?.Log($"일일 데이터 기록 중 오류: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 현재 게임 데이터를 기반으로 일일 데이터를 생성합니다.
        /// </summary>
        public async Task<DailyFarmData> GenerateCurrentDailyData()
        {
            try
            {
                var farmData = await _dataCollector.CollectCurrentDataAsync();
                
                return new DailyFarmData
                {
                    Date = DateTime.Now.Date,
                    Season = Game1.currentSeason,
                    Year = Game1.year,
                    Day = Game1.dayOfMonth,
                    TotalEarnings = farmData.OverviewData?.TotalEarnings ?? 0,
                    CropCount = farmData.CropStatistics?.Count ?? 0,
                    AnimalCount = farmData.AnimalStatistics?.Count ?? 0,
                    TotalCropsHarvested = farmData.OverviewData?.TotalCropsHarvested ?? 0,
                    TotalAnimalProducts = farmData.OverviewData?.TotalAnimalProducts ?? 0,
                    PlayTimeMinutes = GetPlayTimeMinutes()
                };
            }
            catch (Exception ex)
            {
                _monitor?.Log($"현재 일일 데이터 생성 중 오류: {ex.Message}", LogLevel.Error);
                return new DailyFarmData
                {
                    Date = DateTime.Now.Date,
                    Season = Game1.currentSeason ?? "spring",
                    Year = Game1.year,
                    Day = Game1.dayOfMonth
                };
            }
        }

        #endregion

        #region Private Analysis Methods

        /// <summary>
        /// 수익 트렌드 분석
        /// </summary>
        private async Task<TrendAnalysisResult> AnalyzeProfitTrend(AnalysisParameters parameters)
        {
            await Task.CompletedTask; // 비동기 패턴 유지
            
            try
            {
                var days = parameters.GetParameter("days", 28);
                // This entire method needs to be async to support the data collector
                var dailyData = await GetHistoricalData(days);
                
                if (dailyData.Count < 3)
                {
                    return CreateInsufficientDataResult("수익 트렌드");
                }
                
                var profits = dailyData.Select(d => d.TotalEarnings).ToList();
                var dates = dailyData.Select(d => d.Date).ToList();
                
                // 트렌드 계산 (선형 회귀)
                var trend = CalculateLinearTrend(profits);
                
                // 예측 (다음 7일)
                var prediction = PredictNextValues(profits, 7);
                
                // 신뢰도 계산
                var confidence = CalculateConfidence(profits, trend);
                
                return new TrendAnalysisResult
                {
                    AnalysisType = "수익 트렌드",
                    TimeRange = $"최근 {days}일",
                    TrendDirection = GetTrendDirection(trend.Slope),
                    TrendStrength = GetTrendStrength(trend.RSquared),
                    CurrentValue = profits.LastOrDefault(),
                    AverageValue = profits.Average(),
                    MinValue = profits.Min(),
                    MaxValue = profits.Max(),
                    TrendSlope = trend.Slope,
                    Confidence = confidence,
                    Prediction = prediction,
                    DataPoints = dailyData.Select(d => new DataPoint
                    {
                        Date = d.Date,
                        Value = d.TotalEarnings,
                        Label = $"{d.TotalEarnings:N0}G"
                    }).ToList(),
                    Summary = GenerateProfitTrendSummary(trend, profits, confidence),
                    Recommendations = GenerateProfitRecommendations(trend, profits)
                };
            }
            catch (Exception ex)
            {
                _monitor?.Log($"수익 트렌드 분석 중 오류: {ex.Message}", LogLevel.Error);
                throw new AnalysisException("수익 트렌드 분석 실패", ex);
            }
        }

        /// <summary>
        /// 생산량 트렌드 분석
        /// </summary>
        private async Task<TrendAnalysisResult> AnalyzeProductionTrend(AnalysisParameters parameters)
        {
            await Task.CompletedTask;
            
            try
            {
                var days = parameters.GetParameter("days", 28);
                var dailyData = await GetHistoricalData(days);
                
                if (dailyData.Count < 3)
                {
                    return CreateInsufficientDataResult("생산량 트렌드");
                }
                
                var productions = dailyData.Select(d => (double)(d.TotalCropsHarvested + d.TotalAnimalProducts)).ToList();
                var trend = CalculateLinearTrend(productions);
                var prediction = PredictNextValues(productions, 7);
                var confidence = CalculateConfidence(productions, trend);
                
                return new TrendAnalysisResult
                {
                    AnalysisType = "생산량 트렌드",
                    TimeRange = $"최근 {days}일",
                    TrendDirection = GetTrendDirection(trend.Slope),
                    TrendStrength = GetTrendStrength(trend.RSquared),
                    CurrentValue = productions.LastOrDefault(),
                    AverageValue = productions.Average(),
                    MinValue = productions.Min(),
                    MaxValue = productions.Max(),
                    TrendSlope = trend.Slope,
                    Confidence = confidence,
                    Prediction = prediction,
                    DataPoints = dailyData.Select(d => new DataPoint
                    {
                        Date = d.Date,
                        Value = d.TotalCropsHarvested + d.TotalAnimalProducts,
                        Label = $"{d.TotalCropsHarvested + d.TotalAnimalProducts:N0}개"
                    }).ToList(),
                    Summary = GenerateProductionTrendSummary(trend, productions, confidence),
                    Recommendations = GenerateProductionRecommendations(trend, productions)
                };
            }
            catch (Exception ex)
            {
                _monitor?.Log($"생산량 트렌드 분석 중 오류: {ex.Message}", LogLevel.Error);
                throw new AnalysisException("생산량 트렌드 분석 실패", ex);
            }
        }

        /// <summary>
        /// 효율성 트렌드 분석
        /// </summary>
        private async Task<TrendAnalysisResult> AnalyzeEfficiencyTrend(AnalysisParameters parameters)
        {
            await Task.CompletedTask;
            
            try
            {
                var days = parameters.GetParameter("days", 28);
                var dailyData = await GetHistoricalData(days);
                
                if (dailyData.Count < 3)
                {
                    return CreateInsufficientDataResult("효율성 트렌드");
                }
                
                // 효율성 = 수익 / 플레이 시간
                var efficiencies = dailyData
                    .Where(d => d.PlayTimeMinutes > 0)
                    .Select(d => d.TotalEarnings / Math.Max(d.PlayTimeMinutes, 1))
                    .ToList();
                
                if (efficiencies.Count < 3)
                {
                    return CreateInsufficientDataResult("효율성 트렌드 (플레이 시간 부족)");
                }
                
                var trend = CalculateLinearTrend(efficiencies);
                var prediction = PredictNextValues(efficiencies, 7);
                var confidence = CalculateConfidence(efficiencies, trend);
                
                return new TrendAnalysisResult
                {
                    AnalysisType = "효율성 트렌드",
                    TimeRange = $"최근 {days}일",
                    TrendDirection = GetTrendDirection(trend.Slope),
                    TrendStrength = GetTrendStrength(trend.RSquared),
                    CurrentValue = efficiencies.LastOrDefault(),
                    AverageValue = efficiencies.Average(),
                    MinValue = efficiencies.Min(),
                    MaxValue = efficiencies.Max(),
                    TrendSlope = trend.Slope,
                    Confidence = confidence,
                    Prediction = prediction,
                    DataPoints = dailyData
                        .Where(d => d.PlayTimeMinutes > 0)
                        .Select(d => new DataPoint
                        {
                            Date = d.Date,
                            Value = d.TotalEarnings / Math.Max(d.PlayTimeMinutes, 1),
                            Label = $"{d.TotalEarnings / Math.Max(d.PlayTimeMinutes, 1):F1}G/분"
                        }).ToList(),
                    Summary = GenerateEfficiencyTrendSummary(trend, efficiencies, confidence),
                    Recommendations = GenerateEfficiencyRecommendations(trend, efficiencies)
                };
            }
            catch (Exception ex)
            {
                _monitor?.Log($"효율성 트렌드 분석 중 오류: {ex.Message}", LogLevel.Error);
                throw new AnalysisException("효율성 트렌드 분석 실패", ex);
            }
        }

        /// <summary>
        /// 계절별 비교 분석
        /// </summary>
        private async Task<TrendAnalysisResult> AnalyzeSeasonalComparison(AnalysisParameters parameters)
        {
            await Task.CompletedTask;
            
            try
            {
                var currentSeason = Game1.currentSeason ?? "spring";
                var seasonalData = GetSeasonalData();
                
                if (seasonalData.Count < 2)
                {
                    return CreateInsufficientDataResult("계절별 비교");
                }
                
                var currentSeasonData = seasonalData.GetValueOrDefault(currentSeason, new List<DailyFarmData>());
                var currentAverage = currentSeasonData.Any() ? currentSeasonData.Average(d => d.TotalEarnings) : 0;
                
                var allSeasonsAverage = seasonalData.Values
                    .Where(data => data.Any())
                    .Select(data => data.Average(d => d.TotalEarnings))
                    .Average();
                
                var bestSeason = seasonalData
                    .Where(kvp => kvp.Value.Any())
                    .OrderByDescending(kvp => kvp.Value.Average(d => d.TotalEarnings))
                    .FirstOrDefault();
                
                return new TrendAnalysisResult
                {
                    AnalysisType = "계절별 비교",
                    TimeRange = "전체 기간",
                    TrendDirection = currentAverage > allSeasonsAverage ? TrendDirection.Increasing : TrendDirection.Decreasing,
                    TrendStrength = TrendStrength.Medium,
                    CurrentValue = currentAverage,
                    AverageValue = allSeasonsAverage,
                    MinValue = seasonalData.Values.Where(v => v.Any()).Min(v => v.Average(d => d.TotalEarnings)),
                    MaxValue = seasonalData.Values.Where(v => v.Any()).Max(v => v.Average(d => d.TotalEarnings)),
                    Confidence = 0.8,
                    DataPoints = seasonalData.Select(kvp => new DataPoint
                    {
                        Label = GetSeasonName(kvp.Key),
                        Value = kvp.Value.Any() ? kvp.Value.Average(d => d.TotalEarnings) : 0,
                        Date = DateTime.Now // 계절 비교에서는 의미 없음
                    }).ToList(),
                    Summary = GenerateSeasonalComparisonSummary(currentSeason, bestSeason.Key, currentAverage, allSeasonsAverage),
                    Recommendations = GenerateSeasonalRecommendations(seasonalData, currentSeason)
                };
            }
            catch (Exception ex)
            {
                _monitor?.Log($"계절별 비교 분석 중 오류: {ex.Message}", LogLevel.Error);
                throw new AnalysisException("계절별 비교 분석 실패", ex);
            }
        }

        /// <summary>
        /// 예측 분석
        /// </summary>
        private async Task<TrendAnalysisResult> AnalyzePrediction(AnalysisParameters parameters)
        {
            await Task.CompletedTask;
            
            try
            {
                var days = parameters.GetParameter("days", 14);
                var predictionDays = parameters.GetParameter("prediction_days", 7);
                var dailyData = await GetHistoricalData(days);
                
                if (dailyData.Count < 7)
                {
                    return CreateInsufficientDataResult("예측 분석");
                }
                
                var profits = dailyData.Select(d => d.TotalEarnings).ToList();
                var prediction = PredictNextValues(profits, predictionDays);
                var confidence = CalculateConfidence(profits, CalculateLinearTrend(profits));
                
                return new TrendAnalysisResult
                {
                    AnalysisType = "예측 분석",
                    TimeRange = $"다음 {predictionDays}일 예측",
                    TrendDirection = prediction.Any() && prediction.Last() > profits.Last() ? 
                        TrendDirection.Increasing : TrendDirection.Decreasing,
                    TrendStrength = GetTrendStrength(confidence),
                    CurrentValue = profits.LastOrDefault(),
                    AverageValue = profits.Average(),
                    Confidence = confidence,
                    Prediction = prediction,
                    DataPoints = prediction.Select((value, index) => new DataPoint
                    {
                        Date = DateTime.Now.AddDays(index + 1),
                        Value = value,
                        Label = $"{value:N0}G (예측)"
                    }).ToList(),
                    Summary = GeneratePredictionSummary(profits, prediction, confidence),
                    Recommendations = GeneratePredictionRecommendations(profits, prediction, confidence)
                };
            }
            catch (Exception ex)
            {
                _monitor?.Log($"예측 분석 중 오류: {ex.Message}", LogLevel.Error);
                throw new AnalysisException("예측 분석 실패", ex);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 히스토리컬 데이터를 가져옵니다. (현재는 단일 최신 데이터만 반환)
        /// </summary>
        private async Task<List<DailyFarmData>> GetHistoricalData(int days)
        {
            await _dataLock.WaitAsync();
            try
            {
                // Phase 3.2에서는 실제 과거 데이터 대신 현재 데이터만 사용
                var currentData = await GenerateCurrentDailyData();
                return new List<DailyFarmData> { currentData };
            }
            finally
            {
                _dataLock.Release();
            }
        }

        /// <summary>
        /// 계절별 데이터를 가져옵니다.
        /// </summary>
        private Dictionary<string, List<DailyFarmData>> GetSeasonalData()
        {
            lock (_dataLock)
            {
                var result = new Dictionary<string, List<DailyFarmData>>();
                var allData = _historicalData.Values.SelectMany(dataList => dataList);
                
                foreach (var data in allData)
                {
                    if (!result.ContainsKey(data.Season))
                    {
                        result[data.Season] = new List<DailyFarmData>();
                    }
                    result[data.Season].Add(data);
                }
                
                return result;
            }
        }

        /// <summary>
        /// 선형 트렌드를 계산합니다.
        /// </summary>
        private LinearTrend CalculateLinearTrend(List<double> values)
        {
            if (values.Count < 2)
                return new LinearTrend { Slope = 0, Intercept = values.FirstOrDefault(), RSquared = 0 };
            
            var n = values.Count;
            var xValues = Enumerable.Range(0, n).Select(i => (double)i).ToList();
            
            var xMean = xValues.Average();
            var yMean = values.Average();
            
            var numerator = xValues.Zip(values, (x, y) => (x - xMean) * (y - yMean)).Sum();
            var denominator = xValues.Select(x => Math.Pow(x - xMean, 2)).Sum();
            
            var slope = denominator != 0 ? numerator / denominator : 0;
            var intercept = yMean - slope * xMean;
            
            // R-squared 계산
            var predictedValues = xValues.Select(x => slope * x + intercept).ToList();
            var totalSumSquares = values.Select(y => Math.Pow(y - yMean, 2)).Sum();
            var residualSumSquares = values.Zip(predictedValues, (actual, predicted) => 
                Math.Pow(actual - predicted, 2)).Sum();
            
            var rSquared = totalSumSquares != 0 ? 1 - (residualSumSquares / totalSumSquares) : 0;
            
            return new LinearTrend
            {
                Slope = slope,
                Intercept = intercept,
                RSquared = Math.Max(0, Math.Min(1, rSquared)) // 0~1 범위로 제한
            };
        }

        /// <summary>
        /// 다음 값들을 예측합니다.
        /// </summary>
        private List<double> PredictNextValues(List<double> historicalValues, int predictionCount)
        {
            var trend = CalculateLinearTrend(historicalValues);
            var lastIndex = historicalValues.Count - 1;
            
            var predictions = new List<double>();
            for (int i = 1; i <= predictionCount; i++)
            {
                var predictedValue = trend.Slope * (lastIndex + i) + trend.Intercept;
                predictions.Add(Math.Max(0, predictedValue)); // 음수 방지
            }
            
            return predictions;
        }

        /// <summary>
        /// 신뢰도를 계산합니다.
        /// </summary>
        private double CalculateConfidence(List<double> values, LinearTrend trend)
        {
            if (values.Count < 3)
                return 0.3;
            
            // R-squared 기반 신뢰도 + 데이터 포인트 수 고려
            var baseConfidence = trend.RSquared;
            var dataPointBonus = Math.Min(0.3, values.Count * 0.02); // 데이터가 많을수록 신뢰도 증가
            
            return Math.Min(0.95, baseConfidence + dataPointBonus);
        }

        /// <summary>
        /// 트렌드 방향을 결정합니다.
        /// </summary>
        private TrendDirection GetTrendDirection(double slope)
        {
            if (Math.Abs(slope) < 0.1) return TrendDirection.Stable;
            return slope > 0 ? TrendDirection.Increasing : TrendDirection.Decreasing;
        }

        /// <summary>
        /// 트렌드 강도를 결정합니다.
        /// </summary>
        private TrendStrength GetTrendStrength(double rSquared)
        {
            if (rSquared < 0.3) return TrendStrength.Weak;
            if (rSquared < 0.7) return TrendStrength.Medium;
            return TrendStrength.Strong;
        }

        /// <summary>
        /// 계절 이름을 한국어로 변환합니다.
        /// </summary>
        private string GetSeasonName(string season)
        {
            return season switch
            {
                "spring" => "봄",
                "summer" => "여름",
                "fall" => "가을",
                "winter" => "겨울",
                _ => season
            };
        }

        /// <summary>
        /// 수익 문자열을 파싱합니다.
        /// </summary>
        private double ParseEarnings(string earningsStr)
        {
            if (string.IsNullOrEmpty(earningsStr))
                return 0;
                
            var cleanStr = earningsStr.Replace("G", "").Replace(",", "").Trim();
            return double.TryParse(cleanStr, out var result) ? result : 0;
        }

        /// <summary>
        /// 개수 문자열을 파싱합니다.
        /// </summary>
        private int ParseCount(string countStr)
        {
            if (string.IsNullOrEmpty(countStr))
                return 0;
                
            var cleanStr = countStr.Replace("개", "").Replace(",", "").Trim();
            return int.TryParse(cleanStr, out var result) ? result : 0;
        }

        /// <summary>
        /// 플레이 시간을 분 단위로 가져옵니다.
        /// </summary>
        private int GetPlayTimeMinutes()
        {
            try
            {
                if (Game1.player?.millisecondsPlayed != null)
                {
                    return (int)(Game1.player.millisecondsPlayed / (1000 * 60));
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 데이터 부족 시 결과를 생성합니다.
        /// </summary>
        private TrendAnalysisResult CreateInsufficientDataResult(string analysisType)
        {
            return new TrendAnalysisResult
            {
                AnalysisType = analysisType,
                TimeRange = "데이터 부족",
                TrendDirection = TrendDirection.Stable,
                TrendStrength = TrendStrength.Weak,
                Confidence = 0,
                DataPoints = new List<DataPoint>(),
                Summary = "분석을 위한 충분한 데이터가 없습니다. 하루 이상 게임을 플레이해야 트렌드 분석이 가능합니다.",
                Recommendations = new List<string> { "내일 다시 확인해주세요. 데이터가 수집되면 분석을 제공합니다." }
            };
        }

        #endregion

        #region Summary and Recommendation Generators

        private string GenerateProfitTrendSummary(LinearTrend trend, List<double> profits, double confidence)
        {
            var direction = GetTrendDirection(trend.Slope);
            var strength = GetTrendStrength(trend.RSquared);
            var current = profits.LastOrDefault();
            var average = profits.Average();
            
            return direction switch
            {
                TrendDirection.Increasing => $"수익이 {strength.ToString().ToLower()} 상승 추세입니다. " +
                    $"현재 {current:N0}G로 평균 {average:N0}G보다 {(current > average ? "높습니다" : "낮습니다")}. " +
                    $"신뢰도: {confidence:P0}",
                TrendDirection.Decreasing => $"수익이 {strength.ToString().ToLower()} 하락 추세입니다. " +
                    $"개선이 필요할 수 있습니다. 신뢰도: {confidence:P0}",
                _ => $"수익이 안정적입니다. 평균 {average:N0}G를 유지하고 있습니다. 신뢰도: {confidence:P0}"
            };
        }

        private List<string> GenerateProfitRecommendations(LinearTrend trend, List<double> profits)
        {
            var recommendations = new List<string>();
            var direction = GetTrendDirection(trend.Slope);
            
            if (direction == TrendDirection.Decreasing)
            {
                recommendations.Add("더 수익성 높은 작물로 전환을 고려해보세요.");
                recommendations.Add("동물 사육을 통한 안정적인 수입원을 확보하세요.");
                recommendations.Add("가공품 제작으로 부가가치를 높여보세요.");
            }
            else if (direction == TrendDirection.Increasing)
            {
                recommendations.Add("현재 전략을 유지하며 규모를 확장해보세요.");
                recommendations.Add("성공한 작물의 재배 면적을 늘려보세요.");
            }
            else
            {
                recommendations.Add("새로운 수익원 개발을 통해 성장을 도모하세요.");
                recommendations.Add("효율성 개선을 통한 최적화를 시도해보세요.");
            }
            
            return recommendations;
        }

        private string GenerateProductionTrendSummary(LinearTrend trend, List<double> productions, double confidence)
        {
            var direction = GetTrendDirection(trend.Slope);
            var current = productions.LastOrDefault();
            var average = productions.Average();
            
            return $"생산량이 {direction.ToString().ToLower()} 추세입니다. " +
                   $"현재 {current:N0}개, 평균 {average:N0}개. 신뢰도: {confidence:P0}";
        }

        private List<string> GenerateProductionRecommendations(LinearTrend trend, List<double> productions)
        {
            var recommendations = new List<string>();
            var direction = GetTrendDirection(trend.Slope);
            
            if (direction == TrendDirection.Decreasing)
            {
                recommendations.Add("작물 재배 면적을 늘려보세요.");
                recommendations.Add("더 많은 동물을 사육해보세요.");
            }
            else
            {
                recommendations.Add("현재 생산량을 유지하며 품질 개선에 집중하세요.");
            }
            
            return recommendations;
        }

        private string GenerateEfficiencyTrendSummary(LinearTrend trend, List<double> efficiencies, double confidence)
        {
            var direction = GetTrendDirection(trend.Slope);
            var current = efficiencies.LastOrDefault();
            
            return $"시간당 효율성이 {direction.ToString().ToLower()} 추세입니다. " +
                   $"현재 {current:F1}G/분. 신뢰도: {confidence:P0}";
        }

        private List<string> GenerateEfficiencyRecommendations(LinearTrend trend, List<double> efficiencies)
        {
            var recommendations = new List<string>();
            recommendations.Add("자동화 도구를 활용하여 효율성을 높여보세요.");
            recommendations.Add("작업 동선을 최적화해보세요.");
            return recommendations;
        }

        private string GenerateSeasonalComparisonSummary(string currentSeason, string bestSeason, double currentAverage, double allSeasonsAverage)
        {
            return $"현재 {GetSeasonName(currentSeason)}의 평균 수익은 {currentAverage:N0}G입니다. " +
                   $"가장 수익성이 좋은 계절은 {GetSeasonName(bestSeason)}입니다.";
        }

        private List<string> GenerateSeasonalRecommendations(Dictionary<string, List<DailyFarmData>> seasonalData, string currentSeason)
        {
            var recommendations = new List<string>();
            recommendations.Add($"{GetSeasonName(currentSeason)}에 적합한 작물에 집중하세요.");
            recommendations.Add("계절별 특성을 활용한 전략을 수립하세요.");
            return recommendations;
        }

        private string GeneratePredictionSummary(List<double> historical, List<double> prediction, double confidence)
        {
            var currentValue = historical.LastOrDefault();
            var predictedValue = prediction.LastOrDefault();
            var change = predictedValue - currentValue;
            var changePercent = currentValue != 0 ? (change / currentValue) * 100 : 0;
            
            return $"다음 주 예상 수익: {predictedValue:N0}G " +
                   $"(현재 대비 {changePercent:+0.0;-0.0}%). 신뢰도: {confidence:P0}";
        }

        private List<string> GeneratePredictionRecommendations(List<double> historical, List<double> prediction, double confidence)
        {
            var recommendations = new List<string>();
            
            if (confidence < 0.5)
            {
                recommendations.Add("예측 정확도가 낮습니다. 더 많은 데이터가 필요합니다.");
            }
            
            var trend = prediction.LastOrDefault() - historical.LastOrDefault();
            if (trend > 0)
            {
                recommendations.Add("긍정적인 전망입니다. 현재 전략을 유지하세요.");
            }
            else
            {
                recommendations.Add("개선이 필요할 수 있습니다. 전략 조정을 고려하세요.");
            }
            
            return recommendations;
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// 일일 농장 데이터
    /// </summary>
    public class DailyFarmData
    {
        public DateTime Date { get; set; }
        public string Season { get; set; } = "";
        public int Year { get; set; }
        public int Day { get; set; }
        public double TotalEarnings { get; set; }
        public int CropCount { get; set; }
        public int AnimalCount { get; set; }
        public int TotalCropsHarvested { get; set; }
        public int TotalAnimalProducts { get; set; }
        public int PlayTimeMinutes { get; set; }
    }

    /// <summary>
    /// 트렌드 분석 결과
    /// </summary>
    public class TrendAnalysisResult
    {
        public string AnalysisType { get; set; } = "";
        public string TimeRange { get; set; } = "";
        public TrendDirection TrendDirection { get; set; }
        public TrendStrength TrendStrength { get; set; }
        public double CurrentValue { get; set; }
        public double AverageValue { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double TrendSlope { get; set; }
        public double Confidence { get; set; }
        public List<double> Prediction { get; set; } = new();
        public List<DataPoint> DataPoints { get; set; } = new();
        public string Summary { get; set; } = "";
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// 데이터 포인트
    /// </summary>
    public class DataPoint
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public string Label { get; set; } = "";
    }

    /// <summary>
    /// 선형 트렌드 데이터
    /// </summary>
    public class LinearTrend
    {
        public double Slope { get; set; }
        public double Intercept { get; set; }
        public double RSquared { get; set; }
    }

    /// <summary>
    /// 트렌드 방향
    /// </summary>
    public enum TrendDirection
    {
        Increasing,
        Decreasing,
        Stable
    }

    /// <summary>
    /// 트렌드 강도
    /// </summary>
    public enum TrendStrength
    {
        Weak,
        Medium,
        Strong
    }

    #endregion
}
