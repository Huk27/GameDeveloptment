using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;

namespace FarmStatistics.Analysis
{
    /// <summary>
    /// LookupAnything 패턴을 적용한 농장 비교 분석 도구 (Phase 3.2)
    /// 현재 농장과 다른 농장들 또는 이상적인 농장과 비교 분석을 제공합니다.
    /// </summary>
    public class FarmComparisonTool : BaseAnalysisProvider<ComparisonResult>
    {
        #region Fields

        private readonly IMonitor _monitor;
        private readonly GameDataCollector _dataCollector;
        private readonly Dictionary<string, FarmBenchmarkData> _benchmarks;

        #endregion

        #region Constructor

        public FarmComparisonTool(IMonitor monitor, GameDataCollector dataCollector) 
            : base(TimeSpan.FromMinutes(15)) // 비교 분석은 15분 캐시
        {
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            _dataCollector = dataCollector ?? throw new ArgumentNullException(nameof(dataCollector));
            _benchmarks = new Dictionary<string, FarmBenchmarkData>();
            
            InitializeBenchmarks();
            _monitor.Log("농장 비교 분석 도구 초기화 완료", LogLevel.Debug);
        }

        #endregion

        #region Analysis Factory Registration

        protected override void RegisterAnalysisFactories()
        {
            // 수익성 비교
            _analysisFactories["profitability_comparison"] = async (parameters) =>
                await CompareProfitability(parameters);

            // 효율성 비교
            _analysisFactories["efficiency_comparison"] = async (parameters) =>
                await CompareEfficiency(parameters);

            // 다양성 비교
            _analysisFactories["diversity_comparison"] = async (parameters) =>
                await CompareDiversity(parameters);

            // 성장 잠재력 비교
            _analysisFactories["growth_potential_comparison"] = async (parameters) =>
                await CompareGrowthPotential(parameters);

            // 종합 비교
            _analysisFactories["comprehensive_comparison"] = async (parameters) =>
                await CompareComprehensive(parameters);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 벤치마크 데이터를 추가합니다.
        /// </summary>
        public void AddBenchmark(string name, FarmBenchmarkData benchmarkData)
        {
            try
            {
                _benchmarks[name] = benchmarkData;
                InvalidateCache(); // 새 벤치마크 추가 시 캐시 무효화
                _monitor?.Log($"벤치마크 추가: {name}", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor?.Log($"벤치마크 추가 중 오류 ({name}): {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 현재 농장 데이터를 가져옵니다.
        /// </summary>
        public async Task<FarmData> GetCurrentFarmData()
        {
            try
            {
                var farmStats = await _dataCollector.CollectCurrentDataAsync();
                
                return new FarmData
                {
                    Name = "현재 농장",
                    TotalEarnings = farmStats.OverviewData?.TotalEarnings ?? 0,
                    CropTypes = farmStats.CropStatistics?.Count ?? 0,
                    AnimalTypes = farmStats.AnimalStatistics?.Count ?? 0,
                    TotalCropsHarvested = farmStats.OverviewData?.TotalCropsHarvested ?? 0,
                    TotalAnimalProducts = farmStats.OverviewData?.TotalAnimalProducts ?? 0,
                    PlayTimeHours = GetPlayTimeHours(),
                    CropStatistics = farmStats.CropStatistics?.ToList() ?? new List<CropStatistic>(),
                    AnimalStatistics = farmStats.AnimalStatistics?.ToList() ?? new List<AnimalStatistic>()
                };
            }
            catch (Exception ex)
            {
                _monitor?.Log($"현재 농장 데이터 수집 중 오류: {ex.Message}", LogLevel.Error);
                return new FarmData { Name = "현재 농장" };
            }
        }

        #endregion

        #region Private Analysis Methods

        /// <summary>
        /// 수익성 비교 분석
        /// </summary>
        private async Task<ComparisonResult> CompareProfitability(AnalysisParameters parameters)
        {
            try
            {
                var currentFarm = await GetCurrentFarmData();
                var benchmarkName = parameters.GetParameter<string>("benchmark", "average_farm");
                
                if (!_benchmarks.TryGetValue(benchmarkName, out var benchmark))
                {
                    throw new ArgumentException($"벤치마크를 찾을 수 없습니다: {benchmarkName}");
                }

                var currentProfitability = CalculateProfitability(currentFarm);
                var benchmarkProfitability = benchmark.AverageProfitability;
                
                var difference = currentProfitability - benchmarkProfitability;
                var percentageDifference = benchmarkProfitability != 0 ? 
                    (difference / benchmarkProfitability) * 100 : 0;

                return new ComparisonResult
                {
                    ComparisonType = "수익성 비교",
                    CurrentFarmName = currentFarm.Name,
                    BenchmarkName = benchmark.Name,
                    CurrentValue = currentProfitability,
                    BenchmarkValue = benchmarkProfitability,
                    Difference = difference,
                    PercentageDifference = percentageDifference,
                    IsCurrentBetter = currentProfitability > benchmarkProfitability,
                    Rating = CalculateRating(percentageDifference),
                    Metrics = new List<ComparisonMetric>
                    {
                        new ComparisonMetric
                        {
                            Name = "총 수익",
                            CurrentValue = currentFarm.TotalEarnings,
                            BenchmarkValue = benchmark.AverageTotalEarnings,
                            Unit = "G",
                            Difference = currentFarm.TotalEarnings - benchmark.AverageTotalEarnings
                        },
                        new ComparisonMetric
                        {
                            Name = "시간당 수익",
                            CurrentValue = currentFarm.PlayTimeHours > 0 ? currentFarm.TotalEarnings / currentFarm.PlayTimeHours : 0,
                            BenchmarkValue = benchmark.AverageHourlyEarnings,
                            Unit = "G/시간",
                            Difference = (currentFarm.PlayTimeHours > 0 ? currentFarm.TotalEarnings / currentFarm.PlayTimeHours : 0) - benchmark.AverageHourlyEarnings
                        }
                    },
                    Summary = GenerateProfitabilitySummary(currentProfitability, benchmarkProfitability, percentageDifference),
                    Recommendations = GenerateProfitabilityRecommendations(currentFarm, benchmark, percentageDifference)
                };
            }
            catch (Exception ex)
            {
                _monitor?.Log($"수익성 비교 분석 중 오류: {ex.Message}", LogLevel.Error);
                throw new AnalysisException("수익성 비교 분석 실패", ex);
            }
        }

        /// <summary>
        /// 효율성 비교 분석
        /// </summary>
        private async Task<ComparisonResult> CompareEfficiency(AnalysisParameters parameters)
        {
            try
            {
                var currentFarm = await GetCurrentFarmData();
                var benchmarkName = parameters.GetParameter<string>("benchmark", "efficient_farm");
                
                if (!_benchmarks.TryGetValue(benchmarkName, out var benchmark))
                {
                    throw new ArgumentException($"벤치마크를 찾을 수 없습니다: {benchmarkName}");
                }

                var currentEfficiency = CalculateEfficiency(currentFarm);
                var benchmarkEfficiency = benchmark.AverageEfficiency;
                
                var difference = currentEfficiency - benchmarkEfficiency;
                var percentageDifference = benchmarkEfficiency != 0 ? 
                    (difference / benchmarkEfficiency) * 100 : 0;

                return new ComparisonResult
                {
                    ComparisonType = "효율성 비교",
                    CurrentFarmName = currentFarm.Name,
                    BenchmarkName = benchmark.Name,
                    CurrentValue = currentEfficiency,
                    BenchmarkValue = benchmarkEfficiency,
                    Difference = difference,
                    PercentageDifference = percentageDifference,
                    IsCurrentBetter = currentEfficiency > benchmarkEfficiency,
                    Rating = CalculateRating(percentageDifference),
                    Metrics = new List<ComparisonMetric>
                    {
                        new ComparisonMetric
                        {
                            Name = "생산 효율성",
                            CurrentValue = CalculateProductionEfficiency(currentFarm),
                            BenchmarkValue = benchmark.AverageProductionEfficiency,
                            Unit = "개/시간",
                            Difference = CalculateProductionEfficiency(currentFarm) - benchmark.AverageProductionEfficiency
                        },
                        new ComparisonMetric
                        {
                            Name = "수익 효율성",
                            CurrentValue = currentFarm.PlayTimeHours > 0 ? currentFarm.TotalEarnings / currentFarm.PlayTimeHours : 0,
                            BenchmarkValue = benchmark.AverageHourlyEarnings,
                            Unit = "G/시간",
                            Difference = (currentFarm.PlayTimeHours > 0 ? currentFarm.TotalEarnings / currentFarm.PlayTimeHours : 0) - benchmark.AverageHourlyEarnings
                        }
                    },
                    Summary = GenerateEfficiencySummary(currentEfficiency, benchmarkEfficiency, percentageDifference),
                    Recommendations = GenerateEfficiencyRecommendations(currentFarm, benchmark, percentageDifference)
                };
            }
            catch (Exception ex)
            {
                _monitor?.Log($"효율성 비교 분석 중 오류: {ex.Message}", LogLevel.Error);
                throw new AnalysisException("효율성 비교 분석 실패", ex);
            }
        }

        /// <summary>
        /// 다양성 비교 분석
        /// </summary>
        private async Task<ComparisonResult> CompareDiversity(AnalysisParameters parameters)
        {
            try
            {
                var currentFarm = await GetCurrentFarmData();
                var benchmarkName = parameters.GetParameter<string>("benchmark", "diverse_farm");
                
                if (!_benchmarks.TryGetValue(benchmarkName, out var benchmark))
                {
                    throw new ArgumentException($"벤치마크를 찾을 수 없습니다: {benchmarkName}");
                }

                var currentDiversity = CalculateDiversity(currentFarm);
                var benchmarkDiversity = benchmark.AverageDiversity;
                
                var difference = currentDiversity - benchmarkDiversity;
                var percentageDifference = benchmarkDiversity != 0 ? 
                    (difference / benchmarkDiversity) * 100 : 0;

                return new ComparisonResult
                {
                    ComparisonType = "다양성 비교",
                    CurrentFarmName = currentFarm.Name,
                    BenchmarkName = benchmark.Name,
                    CurrentValue = currentDiversity,
                    BenchmarkValue = benchmarkDiversity,
                    Difference = difference,
                    PercentageDifference = percentageDifference,
                    IsCurrentBetter = currentDiversity > benchmarkDiversity,
                    Rating = CalculateRating(percentageDifference),
                    Metrics = new List<ComparisonMetric>
                    {
                        new ComparisonMetric
                        {
                            Name = "작물 종류",
                            CurrentValue = currentFarm.CropTypes,
                            BenchmarkValue = benchmark.AverageCropTypes,
                            Unit = "종",
                            Difference = currentFarm.CropTypes - benchmark.AverageCropTypes
                        },
                        new ComparisonMetric
                        {
                            Name = "동물 종류",
                            CurrentValue = currentFarm.AnimalTypes,
                            BenchmarkValue = benchmark.AverageAnimalTypes,
                            Unit = "종",
                            Difference = currentFarm.AnimalTypes - benchmark.AverageAnimalTypes
                        }
                    },
                    Summary = GenerateDiversitySummary(currentDiversity, benchmarkDiversity, percentageDifference),
                    Recommendations = GenerateDiversityRecommendations(currentFarm, benchmark, percentageDifference)
                };
            }
            catch (Exception ex)
            {
                _monitor?.Log($"다양성 비교 분석 중 오류: {ex.Message}", LogLevel.Error);
                throw new AnalysisException("다양성 비교 분석 실패", ex);
            }
        }

        /// <summary>
        /// 성장 잠재력 비교 분석
        /// </summary>
        private async Task<ComparisonResult> CompareGrowthPotential(AnalysisParameters parameters)
        {
            try
            {
                var currentFarm = await GetCurrentFarmData();
                var benchmarkName = parameters.GetParameter<string>("benchmark", "growth_farm");
                
                if (!_benchmarks.TryGetValue(benchmarkName, out var benchmark))
                {
                    throw new ArgumentException($"벤치마크를 찾을 수 없습니다: {benchmarkName}");
                }

                var currentGrowthPotential = CalculateGrowthPotential(currentFarm);
                var benchmarkGrowthPotential = benchmark.AverageGrowthPotential;
                
                var difference = currentGrowthPotential - benchmarkGrowthPotential;
                var percentageDifference = benchmarkGrowthPotential != 0 ? 
                    (difference / benchmarkGrowthPotential) * 100 : 0;

                return new ComparisonResult
                {
                    ComparisonType = "성장 잠재력 비교",
                    CurrentFarmName = currentFarm.Name,
                    BenchmarkName = benchmark.Name,
                    CurrentValue = currentGrowthPotential,
                    BenchmarkValue = benchmarkGrowthPotential,
                    Difference = difference,
                    PercentageDifference = percentageDifference,
                    IsCurrentBetter = currentGrowthPotential > benchmarkGrowthPotential,
                    Rating = CalculateRating(percentageDifference),
                    Summary = GenerateGrowthPotentialSummary(currentGrowthPotential, benchmarkGrowthPotential, percentageDifference),
                    Recommendations = GenerateGrowthPotentialRecommendations(currentFarm, benchmark, percentageDifference)
                };
            }
            catch (Exception ex)
            {
                _monitor?.Log($"성장 잠재력 비교 분석 중 오류: {ex.Message}", LogLevel.Error);
                throw new AnalysisException("성장 잠재력 비교 분석 실패", ex);
            }
        }

        /// <summary>
        /// 종합 비교 분석
        /// </summary>
        private async Task<ComparisonResult> CompareComprehensive(AnalysisParameters parameters)
        {
            try
            {
                var profitabilityResult = await CompareProfitability(parameters);
                var efficiencyResult = await CompareEfficiency(parameters);
                var diversityResult = await CompareDiversity(parameters);
                var growthResult = await CompareGrowthPotential(parameters);

                // 가중 평균으로 종합 점수 계산
                var weights = new Dictionary<string, double>
                {
                    ["profitability"] = 0.4, // 수익성 40%
                    ["efficiency"] = 0.3,    // 효율성 30%
                    ["diversity"] = 0.2,     // 다양성 20%
                    ["growth"] = 0.1         // 성장성 10%
                };

                var overallScore = 
                    (profitabilityResult.PercentageDifference * weights["profitability"]) +
                    (efficiencyResult.PercentageDifference * weights["efficiency"]) +
                    (diversityResult.PercentageDifference * weights["diversity"]) +
                    (growthResult.PercentageDifference * weights["growth"]);

                return new ComparisonResult
                {
                    ComparisonType = "종합 비교",
                    CurrentFarmName = profitabilityResult.CurrentFarmName,
                    BenchmarkName = "종합 벤치마크",
                    PercentageDifference = overallScore,
                    IsCurrentBetter = overallScore > 0,
                    Rating = CalculateRating(overallScore),
                    Metrics = new List<ComparisonMetric>
                    {
                        new ComparisonMetric
                        {
                            Name = "수익성",
                            CurrentValue = profitabilityResult.PercentageDifference,
                            BenchmarkValue = 0,
                            Unit = "%",
                            Difference = profitabilityResult.PercentageDifference
                        },
                        new ComparisonMetric
                        {
                            Name = "효율성",
                            CurrentValue = efficiencyResult.PercentageDifference,
                            BenchmarkValue = 0,
                            Unit = "%",
                            Difference = efficiencyResult.PercentageDifference
                        },
                        new ComparisonMetric
                        {
                            Name = "다양성",
                            CurrentValue = diversityResult.PercentageDifference,
                            BenchmarkValue = 0,
                            Unit = "%",
                            Difference = diversityResult.PercentageDifference
                        },
                        new ComparisonMetric
                        {
                            Name = "성장성",
                            CurrentValue = growthResult.PercentageDifference,
                            BenchmarkValue = 0,
                            Unit = "%",
                            Difference = growthResult.PercentageDifference
                        }
                    },
                    Summary = GenerateComprehensiveSummary(overallScore, new[]
                    {
                        profitabilityResult, efficiencyResult, diversityResult, growthResult
                    }),
                    Recommendations = GenerateComprehensiveRecommendations(new[]
                    {
                        profitabilityResult, efficiencyResult, diversityResult, growthResult
                    })
                };
            }
            catch (Exception ex)
            {
                _monitor?.Log($"종합 비교 분석 중 오류: {ex.Message}", LogLevel.Error);
                throw new AnalysisException("종합 비교 분석 실패", ex);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 벤치마크를 초기화합니다.
        /// </summary>
        private void InitializeBenchmarks()
        {
            // 평균 농장 벤치마크
            _benchmarks["average_farm"] = new FarmBenchmarkData
            {
                Name = "평균 농장",
                AverageTotalEarnings = 500000,
                AverageHourlyEarnings = 2000,
                AverageProfitability = 0.6,
                AverageEfficiency = 0.7,
                AverageDiversity = 0.5,
                AverageGrowthPotential = 0.6,
                AverageCropTypes = 8,
                AverageAnimalTypes = 4,
                AverageProductionEfficiency = 15
            };

            // 효율적인 농장 벤치마크
            _benchmarks["efficient_farm"] = new FarmBenchmarkData
            {
                Name = "효율적인 농장",
                AverageTotalEarnings = 800000,
                AverageHourlyEarnings = 3500,
                AverageProfitability = 0.8,
                AverageEfficiency = 0.9,
                AverageDiversity = 0.6,
                AverageGrowthPotential = 0.7,
                AverageCropTypes = 6,
                AverageAnimalTypes = 3,
                AverageProductionEfficiency = 25
            };

            // 다양한 농장 벤치마크
            _benchmarks["diverse_farm"] = new FarmBenchmarkData
            {
                Name = "다양한 농장",
                AverageTotalEarnings = 600000,
                AverageHourlyEarnings = 2500,
                AverageProfitability = 0.7,
                AverageEfficiency = 0.6,
                AverageDiversity = 0.9,
                AverageGrowthPotential = 0.8,
                AverageCropTypes = 15,
                AverageAnimalTypes = 8,
                AverageProductionEfficiency = 18
            };

            // 성장형 농장 벤치마크
            _benchmarks["growth_farm"] = new FarmBenchmarkData
            {
                Name = "성장형 농장",
                AverageTotalEarnings = 400000,
                AverageHourlyEarnings = 1800,
                AverageProfitability = 0.5,
                AverageEfficiency = 0.6,
                AverageDiversity = 0.7,
                AverageGrowthPotential = 0.9,
                AverageCropTypes = 12,
                AverageAnimalTypes = 6,
                AverageProductionEfficiency = 20
            };
        }

        /// <summary>
        /// 수익성을 계산합니다.
        /// </summary>
        private double CalculateProfitability(FarmData farm)
        {
            // 수익성 = 총 수익 / (플레이 시간 * 1000) * 가중치
            var timeWeight = Math.Max(1, farm.PlayTimeHours / 100.0); // 100시간 기준
            return farm.PlayTimeHours > 0 ? (farm.TotalEarnings / farm.PlayTimeHours) / 1000.0 * timeWeight : 0;
        }

        /// <summary>
        /// 효율성을 계산합니다.
        /// </summary>
        private double CalculateEfficiency(FarmData farm)
        {
            // 효율성 = (생산량 + 수익성) / 2
            var productionEfficiency = CalculateProductionEfficiency(farm);
            var profitEfficiency = farm.PlayTimeHours > 0 ? farm.TotalEarnings / farm.PlayTimeHours / 1000.0 : 0;
            return (productionEfficiency + profitEfficiency) / 2;
        }

        /// <summary>
        /// 생산 효율성을 계산합니다.
        /// </summary>
        private double CalculateProductionEfficiency(FarmData farm)
        {
            var totalProduction = farm.TotalCropsHarvested + farm.TotalAnimalProducts;
            return farm.PlayTimeHours > 0 ? totalProduction / farm.PlayTimeHours : 0;
        }

        /// <summary>
        /// 다양성을 계산합니다.
        /// </summary>
        private double CalculateDiversity(FarmData farm)
        {
            // 다양성 = (작물 종류 + 동물 종류) / 20 (최대 20종 기준)
            return Math.Min(1.0, (farm.CropTypes + farm.AnimalTypes) / 20.0);
        }

        /// <summary>
        /// 성장 잠재력을 계산합니다.
        /// </summary>
        private double CalculateGrowthPotential(FarmData farm)
        {
            // 성장 잠재력 = 다양성 * 0.6 + 효율성 * 0.4
            var diversity = CalculateDiversity(farm);
            var efficiency = CalculateEfficiency(farm);
            return diversity * 0.6 + efficiency * 0.4;
        }

        /// <summary>
        /// 평가 등급을 계산합니다.
        /// </summary>
        private ComparisonRating CalculateRating(double percentageDifference)
        {
            if (percentageDifference >= 50) return ComparisonRating.Excellent;
            if (percentageDifference >= 20) return ComparisonRating.Good;
            if (percentageDifference >= -10) return ComparisonRating.Average;
            if (percentageDifference >= -30) return ComparisonRating.BelowAverage;
            return ComparisonRating.Poor;
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
        /// 플레이 시간을 시간 단위로 가져옵니다.
        /// </summary>
        private double GetPlayTimeHours()
        {
            try
            {
                if (Game1.player?.millisecondsPlayed != null)
                {
                    return Game1.player.millisecondsPlayed / (1000.0 * 60.0 * 60.0);
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region Summary and Recommendation Generators

        private string GenerateProfitabilitySummary(double current, double benchmark, double percentageDifference)
        {
            var comparison = percentageDifference > 0 ? "높습니다" : "낮습니다";
            return $"현재 농장의 수익성은 벤치마크보다 {Math.Abs(percentageDifference):F1}% {comparison}. " +
                   $"현재 수익성 지수: {current:F2}, 벤치마크: {benchmark:F2}";
        }

        private List<string> GenerateProfitabilityRecommendations(FarmData current, FarmBenchmarkData benchmark, double percentageDifference)
        {
            var recommendations = new List<string>();
            
            if (percentageDifference < -20)
            {
                recommendations.Add("고수익 작물(고추, 블루베리 등)로 전환을 고려하세요.");
                recommendations.Add("가공품 제작으로 부가가치를 높이세요.");
                recommendations.Add("동물 사육을 통한 안정적 수입원을 확보하세요.");
            }
            else if (percentageDifference > 20)
            {
                recommendations.Add("현재 수익 전략을 유지하며 규모를 확장하세요.");
                recommendations.Add("성공한 작물의 재배 면적을 늘려보세요.");
            }
            else
            {
                recommendations.Add("효율성 개선을 통해 수익성을 높여보세요.");
                recommendations.Add("새로운 수익원 개발을 시도해보세요.");
            }
            
            return recommendations;
        }

        private string GenerateEfficiencySummary(double current, double benchmark, double percentageDifference)
        {
            var comparison = percentageDifference > 0 ? "높습니다" : "낮습니다";
            return $"현재 농장의 효율성은 벤치마크보다 {Math.Abs(percentageDifference):F1}% {comparison}. " +
                   $"현재 효율성 지수: {current:F2}, 벤치마크: {benchmark:F2}";
        }

        private List<string> GenerateEfficiencyRecommendations(FarmData current, FarmBenchmarkData benchmark, double percentageDifference)
        {
            var recommendations = new List<string>();
            
            if (percentageDifference < 0)
            {
                recommendations.Add("자동화 도구(스프링클러, 자동 수확기)를 활용하세요.");
                recommendations.Add("작업 동선을 최적화하여 시간을 절약하세요.");
                recommendations.Add("빠르게 성장하는 작물에 집중하세요.");
            }
            else
            {
                recommendations.Add("현재 효율성을 유지하며 생산량을 늘려보세요.");
                recommendations.Add("더 복잡한 가공품에 도전해보세요.");
            }
            
            return recommendations;
        }

        private string GenerateDiversitySummary(double current, double benchmark, double percentageDifference)
        {
            var comparison = percentageDifference > 0 ? "높습니다" : "낮습니다";
            return $"현재 농장의 다양성은 벤치마크보다 {Math.Abs(percentageDifference):F1}% {comparison}. " +
                   $"현재 다양성 지수: {current:F2}, 벤치마크: {benchmark:F2}";
        }

        private List<string> GenerateDiversityRecommendations(FarmData current, FarmBenchmarkData benchmark, double percentageDifference)
        {
            var recommendations = new List<string>();
            
            if (percentageDifference < 0)
            {
                recommendations.Add("새로운 작물 종류를 시도해보세요.");
                recommendations.Add("다양한 동물을 사육해보세요.");
                recommendations.Add("계절별로 다른 작물을 재배하세요.");
            }
            else
            {
                recommendations.Add("현재 다양성을 유지하며 각 분야의 효율성을 높이세요.");
                recommendations.Add("특히 수익성 높은 분야에 집중해보세요.");
            }
            
            return recommendations;
        }

        private string GenerateGrowthPotentialSummary(double current, double benchmark, double percentageDifference)
        {
            var comparison = percentageDifference > 0 ? "높습니다" : "낮습니다";
            return $"현재 농장의 성장 잠재력은 벤치마크보다 {Math.Abs(percentageDifference):F1}% {comparison}. " +
                   $"현재 성장 잠재력: {current:F2}, 벤치마크: {benchmark:F2}";
        }

        private List<string> GenerateGrowthPotentialRecommendations(FarmData current, FarmBenchmarkData benchmark, double percentageDifference)
        {
            var recommendations = new List<string>();
            
            if (percentageDifference < 0)
            {
                recommendations.Add("새로운 분야에 투자하여 성장 기반을 마련하세요.");
                recommendations.Add("기술 업그레이드에 투자하세요.");
                recommendations.Add("장기적인 발전 계획을 수립하세요.");
            }
            else
            {
                recommendations.Add("현재 성장 동력을 유지하며 확장하세요.");
                recommendations.Add("성공한 분야의 규모를 늘려보세요.");
            }
            
            return recommendations;
        }

        private string GenerateComprehensiveSummary(double overallScore, ComparisonResult[] results)
        {
            var rating = CalculateRating(overallScore);
            return $"종합 평가: {rating} (벤치마크 대비 {overallScore:+0.0;-0.0}%). " +
                   $"가장 강한 분야: {results.OrderByDescending(r => r.PercentageDifference).First().ComparisonType}, " +
                   $"개선 필요 분야: {results.OrderBy(r => r.PercentageDifference).First().ComparisonType}";
        }

        private List<string> GenerateComprehensiveRecommendations(ComparisonResult[] results)
        {
            var recommendations = new List<string>();
            var weakestArea = results.OrderBy(r => r.PercentageDifference).First();
            var strongestArea = results.OrderByDescending(r => r.PercentageDifference).First();
            
            recommendations.Add($"{weakestArea.ComparisonType} 개선에 집중하세요.");
            recommendations.Add($"{strongestArea.ComparisonType}의 장점을 유지하며 확장하세요.");
            recommendations.Add("균형 잡힌 농장 발전을 위해 모든 분야를 고려하세요.");
            
            return recommendations;
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// 농장 데이터
    /// </summary>
    public class FarmData
    {
        public string Name { get; set; } = "";
        public double TotalEarnings { get; set; }
        public int CropTypes { get; set; }
        public int AnimalTypes { get; set; }
        public int TotalCropsHarvested { get; set; }
        public int TotalAnimalProducts { get; set; }
        public double PlayTimeHours { get; set; }
        public List<CropStatistic> CropStatistics { get; set; } = new();
        public List<AnimalStatistic> AnimalStatistics { get; set; } = new();
    }

    /// <summary>
    /// 벤치마크 데이터
    /// </summary>
    public class FarmBenchmarkData
    {
        public string Name { get; set; } = "";
        public double AverageTotalEarnings { get; set; }
        public double AverageHourlyEarnings { get; set; }
        public double AverageProfitability { get; set; }
        public double AverageEfficiency { get; set; }
        public double AverageDiversity { get; set; }
        public double AverageGrowthPotential { get; set; }
        public int AverageCropTypes { get; set; }
        public int AverageAnimalTypes { get; set; }
        public double AverageProductionEfficiency { get; set; }
    }

    /// <summary>
    /// 비교 결과
    /// </summary>
    public class ComparisonResult
    {
        public string ComparisonType { get; set; } = "";
        public string CurrentFarmName { get; set; } = "";
        public string BenchmarkName { get; set; } = "";
        public double CurrentValue { get; set; }
        public double BenchmarkValue { get; set; }
        public double Difference { get; set; }
        public double PercentageDifference { get; set; }
        public bool IsCurrentBetter { get; set; }
        public ComparisonRating Rating { get; set; }
        public double Percentile { get; set; } // 추가
        public List<ComparisonMetric> Metrics { get; set; } = new();
        public Dictionary<string, double> MetricsDictionary => Metrics.ToDictionary(m => m.Name, m => m.CurrentValue);
        public string Summary { get; set; } = "";
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// 비교 메트릭
    /// </summary>
    public class ComparisonMetric
    {
        public string Name { get; set; } = "";
        public double CurrentValue { get; set; }
        public double BenchmarkValue { get; set; }
        public string Unit { get; set; } = "";
        public double Difference { get; set; }
    }

    /// <summary>
    /// 비교 평가 등급
    /// </summary>
    public enum ComparisonRating
    {
        Poor,           // 매우 부족
        BelowAverage,   // 평균 이하
        Average,        // 평균
        Good,           // 좋음
        Excellent       // 매우 좋음
    }

    #endregion
}
