using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley;
using FarmStatistics.Analysis;

namespace FarmStatistics
{
    public class TabData : INotifyPropertyChanged
    {
        public string Name { get; set; } = "";
        public Tuple<Texture2D, Rectangle>? Sprite { get; set; }
        
        private bool _active;
        public bool Active 
        { 
            get => _active; 
            set
            {
                if (_active != value)
                {
                    _active = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class FarmStatisticsViewModel : INotifyPropertyChanged
    {
        private readonly GameDataCollector? _dataCollector;
        private readonly AdvancedAnalysisManager? _analysisManager;

        private const string DefaultTrendSummary = "Trend insights are currently unavailable.";
        private const string DefaultComparisonMessage = "Comparison data is currently unavailable.";
        public FarmStatisticsViewModel()
        {
            _dataCollector = null;
            InitializeTabs();
        }
        
        public FarmStatisticsViewModel(GameDataCollector dataCollector, AdvancedAnalysisManager analysisManager)
        {
            _dataCollector = dataCollector;
            _analysisManager = analysisManager;
            InitializeTabs();
        }
        
        public IReadOnlyList<TabData> Tabs { get; set; } = new List<TabData>();
        public int TotalEarnings { get; set; }
        public int TotalCropsHarvested { get; set; }
        public int TotalAnimalProducts { get; set; }
        public string TotalPlayTime { get; set; } = "";
        public string SeasonComparison { get; set; } = "";
        public IReadOnlyList<CropStatistic> CropStatistics { get; set; } = new List<CropStatistic>();
        public IReadOnlyList<CropStatistic> PagedCropStatistics { get; private set; } = new List<CropStatistic>();
        public int CurrentCropPage { get; private set; } = 1;
        public int TotalCropPages { get; private set; } = 1;
        private const int PageSize = 10;

        public string CropsHeaderText => $"?묐Ъ ?듦퀎 ({CropStatistics.Count}媛??묐Ъ, {CurrentCropPage}/{TotalCropPages} ?섏씠吏)";
        public IReadOnlyList<AnimalStatistic> AnimalStatistics { get; set; } = new List<AnimalStatistic>();
        public string AnimalsHeaderText => $"?숇Ъ ?듦퀎 ({AnimalStatistics.Count}媛??숇Ъ)";
        public IReadOnlyList<TimeStatistic> TimeStatistics { get; set; } = new List<TimeStatistic>();
        public string TimeHeaderText => $"?쒓컙蹂??듦퀎 ({TimeStatistics.Count}媛???ぉ)";
        public IReadOnlyList<GoalStatistic> GoalStatistics { get; set; } = new List<GoalStatistic>();
        public string GoalsHeaderText => $"紐⑺몴 ?꾪솴 ({GoalStatistics.Count}媛?紐⑺몴)";
        
        private bool _showOverviewTab = true;
        public bool ShowOverviewTab { get => _showOverviewTab; set { if (_showOverviewTab != value) { _showOverviewTab = value; OnPropertyChanged(); } } }
        private bool _showCropsTab = false;
        public bool ShowCropsTab { get => _showCropsTab; set { if (_showCropsTab != value) { _showCropsTab = value; OnPropertyChanged(); } } }
        private bool _showAnimalsTab = false;
        public bool ShowAnimalsTab { get => _showAnimalsTab; set { if (_showAnimalsTab != value) { _showAnimalsTab = value; OnPropertyChanged(); } } }
        private bool _showTimeTab = false;
        public bool ShowTimeTab { get => _showTimeTab; set { if (_showTimeTab != value) { _showTimeTab = value; OnPropertyChanged(); } } }
        private bool _showGoalsTab = false;
        public bool ShowGoalsTab { get => _showGoalsTab; set { if (_showGoalsTab != value) { _showGoalsTab = value; OnPropertyChanged(); } } }
        private bool _showAnalysisTab = false;
        public bool ShowAnalysisTab { get => _showAnalysisTab; set { if (_showAnalysisTab != value) { _showAnalysisTab = value; OnPropertyChanged(); } } }
        private bool _showTrendsTab = false;
        public bool ShowTrendsTab { get => _showTrendsTab; set { if (_showTrendsTab != value) { _showTrendsTab = value; OnPropertyChanged(); } } }
        private bool _showComparisonTab = false;
        public bool ShowComparisonTab { get => _showComparisonTab; set { if (_showComparisonTab != value) { _showComparisonTab = value; OnPropertyChanged(); } } }

        public int OverallScore { get; set; }
        public string OverallRating { get; set; } = "";
        public string AnalysisHeaderText => $"醫낇빀 遺꾩꽍 (?먯닔: {OverallScore}??";
        public IReadOnlyList<string> KeyInsights { get; set; } = new List<string>();
        public IReadOnlyList<string> ActionableRecommendations { get; set; } = new List<string>();
        public string TrendsHeaderText => "?띿옣 ?몃젋??遺꾩꽍";
        public string ProfitTrendSummary { get; set; } = "";
        public string ProductionTrendSummary { get; set; } = "";
        public string EfficiencyTrendSummary { get; set; } = "";
        public string ComparisonHeaderText => "踰ㅼ튂留덊겕 鍮꾧탳 遺꾩꽍";
        public int ProfitabilityScore { get; set; }
        public int EfficiencyScore { get; set; }
        public int DiversityScore { get; set; }
        public int GrowthScore { get; set; }
        public string ProfitabilityComparison { get; set; } = "";
        public string EfficiencyComparison { get; set; } = "";
        public string DiversityComparison { get; set; } = "";
        public string GrowthComparison { get; set; } = "";

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnTabActivated(string tabName)
        {
            foreach (var tab in Tabs) { if (tab.Name != tabName) { tab.Active = false; } }
            ShowOverviewTab = tabName == "overview";
            ShowCropsTab = tabName == "crops";
            ShowAnimalsTab = tabName == "animals";
            ShowTimeTab = tabName == "time";
            ShowGoalsTab = tabName == "goals";
            ShowAnalysisTab = tabName == "analysis";
            ShowTrendsTab = tabName == "trends";
            ShowComparisonTab = tabName == "comparison";
        }

        private void InitializeTabs()
        {
            var cursorsTexture = Game1.mouseCursors;
            var tabRect = new Rectangle(0, 256, 64, 64);
            var tabs = new List<TabData>
            {
                new TabData { Name = "overview", Sprite = Tuple.Create(cursorsTexture, tabRect), Active = true },
                new TabData { Name = "crops", Sprite = Tuple.Create(cursorsTexture, tabRect), Active = false },
                new TabData { Name = "animals", Sprite = Tuple.Create(cursorsTexture, tabRect), Active = false },
                new TabData { Name = "time", Sprite = Tuple.Create(cursorsTexture, tabRect), Active = false },
                new TabData { Name = "goals", Sprite = Tuple.Create(cursorsTexture, tabRect), Active = false },
                new TabData { Name = "analysis", Sprite = Tuple.Create(cursorsTexture, tabRect), Active = false },
                new TabData { Name = "trends", Sprite = Tuple.Create(cursorsTexture, tabRect), Active = false },
                new TabData { Name = "comparison", Sprite = Tuple.Create(cursorsTexture, tabRect), Active = false }
            };
            Tabs = tabs;
        }

        public async void UpdateData()
        {
            Task.Run(async () =>
            {
                try { await UpdateWithRealData(); } catch (Exception) { }
            });
        }
        
        private async Task UpdateWithRealData()
        {
            if (_dataCollector == null || _analysisManager == null) return;
            
            FarmData farmData;
            try
            {
                try
                {
                    farmData = await _dataCollector.CollectCurrentDataAsync();
                }
                catch (Exception)
                {
                    farmData = CreateEmptyFarmData();
                }
            }
            catch (KeyNotFoundException)
            {
                farmData = CreateEmptyFarmData();
            }
            catch (InvalidOperationException)
            {
                farmData = CreateEmptyFarmData();
            }
            var analysisDashboard = await _analysisManager.GenerateDashboardAsync();
            
            // 고급 분석 데이터 업데이트
            OverallScore = (int)analysisDashboard.OverallScore;
            OverallRating = GetRatingFromScore(OverallScore);

            KeyInsights = (analysisDashboard.KeyInsights ?? new List<string>()).ToList();
            ActionableRecommendations = (analysisDashboard.ActionableRecommendations ?? new List<string>()).ToList();

            var trendAnalyses = analysisDashboard.TrendAnalyses ?? new List<TrendAnalysisResult>();
            ProfitTrendSummary = trendAnalyses.Count > 0 ? trendAnalyses[0].Summary : DefaultTrendSummary;
            ProductionTrendSummary = trendAnalyses.Count > 1 ? trendAnalyses[1].Summary : DefaultTrendSummary;
            EfficiencyTrendSummary = trendAnalyses.Count > 2 ? trendAnalyses[2].Summary : DefaultTrendSummary;

            var comparison = analysisDashboard.ComparisonResults?.FirstOrDefault();
            if (comparison != null)
            {
                var metrics = comparison.Metrics ?? new List<ComparisonMetric>();

                ProfitabilityScore = NormalizeComparisonScore(metrics, 0);
                EfficiencyScore = NormalizeComparisonScore(metrics, 1);
                DiversityScore = NormalizeComparisonScore(metrics, 2);
                GrowthScore = NormalizeComparisonScore(metrics, 3);

                ProfitabilityComparison = FormatComparisonText(metrics, 0, comparison);
                EfficiencyComparison = FormatComparisonText(metrics, 1, comparison);
                DiversityComparison = FormatComparisonText(metrics, 2, comparison);
                GrowthComparison = FormatComparisonText(metrics, 3, comparison);
            }
            else
            {
                ProfitabilityScore = EfficiencyScore = DiversityScore = GrowthScore = 50;
                ProfitabilityComparison = DefaultComparisonMessage;
                EfficiencyComparison = DefaultComparisonMessage;
                DiversityComparison = DefaultComparisonMessage;
                GrowthComparison = DefaultComparisonMessage;
            }

            OnPropertyChanged(nameof(OverallScore));
            OnPropertyChanged(nameof(OverallRating));
            OnPropertyChanged(nameof(KeyInsights));
            OnPropertyChanged(nameof(ActionableRecommendations));
            OnPropertyChanged(nameof(AnalysisHeaderText));
            
            OnPropertyChanged(nameof(ProfitTrendSummary));
            OnPropertyChanged(nameof(ProductionTrendSummary));
            OnPropertyChanged(nameof(EfficiencyTrendSummary));
            OnPropertyChanged(nameof(TrendsHeaderText));

            OnPropertyChanged(nameof(ProfitabilityScore));
            OnPropertyChanged(nameof(EfficiencyScore));
            OnPropertyChanged(nameof(DiversityScore));
            OnPropertyChanged(nameof(GrowthScore));
            OnPropertyChanged(nameof(ProfitabilityComparison));
            OnPropertyChanged(nameof(EfficiencyComparison));
            OnPropertyChanged(nameof(DiversityComparison));
            OnPropertyChanged(nameof(GrowthComparison));
            OnPropertyChanged(nameof(ComparisonHeaderText));
        }

        private static int NormalizeComparisonScore(IReadOnlyList<ComparisonMetric> metrics, int index)
        {
            if (metrics == null || index < 0 || index >= metrics.Count)
                return 50;

            var difference = metrics[index].Difference;
            return (int)Math.Clamp(Math.Round(50 + difference, MidpointRounding.AwayFromZero), 0, 100);
        }

        private static string FormatComparisonText(IReadOnlyList<ComparisonMetric> metrics, int index, ComparisonResult comparison)
        {
            if (metrics == null || index < 0 || index >= metrics.Count || comparison == null)
                return DefaultComparisonMessage;

            var metric = metrics[index];
            var direction = metric.Difference > 0 ? "up" : metric.Difference < 0 ? "down" : "level";
            return $"{metric.Name}: {metric.Difference:+0.0;-0.0;0}% ({direction})";
        }
        private static FarmData CreateEmptyFarmData()
        {
            return new FarmData
            {
                OverviewData = new OverviewData
                {
                    TotalEarnings = 0,
                    TotalCropsHarvested = 0,
                    TotalAnimalProducts = 0,
                    TotalPlayTime = string.Empty,
                    SeasonComparison = "Waiting for farm data"
                },
                CropStatistics = new List<CropStatistic>(),
                AnimalStatistics = new List<AnimalStatistic>(),
                TimeStatistics = new List<TimeStatistic>(),
                GoalStatistics = new List<GoalStatistic>(),
                Timestamp = DateTime.Now
            };
        }

        private string GetRatingFromScore(int score)
        {
            if (score > 90) return "Excellent ?뙚";
            if (score > 75) return "Good ?몟";
            if (score > 50) return "Average ?삉";
            return "Needs Improvement ?뱣";
        }

        public void NextCropPage()
        {
            if (CurrentCropPage < TotalCropPages)
            {
                CurrentCropPage++;
                UpdateCropPage();
            }
        }

        public void PreviousCropPage()
        {
            if (CurrentCropPage > 1)
            {
                CurrentCropPage--;
                UpdateCropPage();
            }
        }

        private void UpdateCropPage()
        {
            TotalCropPages = (int)Math.Ceiling((double)CropStatistics.Count / PageSize);
            if (TotalCropPages == 0) TotalCropPages = 1;
            if (CurrentCropPage > TotalCropPages) CurrentCropPage = TotalCropPages;

            PagedCropStatistics = CropStatistics.Skip((CurrentCropPage - 1) * PageSize).Take(PageSize).ToList();

            OnPropertyChanged(nameof(PagedCropStatistics));
            OnPropertyChanged(nameof(CurrentCropPage));
            OnPropertyChanged(nameof(TotalCropPages));
            OnPropertyChanged(nameof(CropsHeaderText));
        }
    }

    public class CropStatistic
    {
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public int Value { get; set; }
        public string Season { get; set; } = "";
        
        // GameDataCollector?먯꽌 ?ъ슜?섎뒗 異붽? ?띿꽦??
        public string CropName 
        { 
            get => Name; 
            set => Name = value; 
        }
        public int Harvested 
        { 
            get => Quantity; 
            set => Quantity = value; 
        }
        public int Revenue 
        { 
            get => Value; 
            set => Value = value; 
        }
        public float GrowthTime { get; set; }
        public string Quality { get; set; } = "";
        public object? Sprite { get; set; }
    }

    public class AnimalStatistic
    {
        public string Name { get; set; } = "";
        public int Count { get; set; }
        public int Value { get; set; }
        
        // GameDataCollector?먯꽌 ?ъ슜?섎뒗 異붽? ?띿꽦??
        public string AnimalName 
        { 
            get => Name; 
            set => Name = value; 
        }
        public int Products 
        { 
            get => Count; 
            set => Count = value; 
        }
        public int Revenue 
        { 
            get => Value; 
            set => Value = value; 
        }
        public float Happiness { get; set; }
        public object? Sprite { get; set; }
    }

    public class TimeStatistic
    {
        public string Period { get; set; } = "";
        public string Activity { get; set; } = "";
        public string Duration { get; set; } = "";
        
        // GameDataCollector?먯꽌 ?ъ슜?섎뒗 異붽? ?띿꽦??
        public float Hours { get; set; }
        public float Percentage { get; set; }
        public string Color { get; set; } = "";

        public string HoursText => Math.Round(Hours).ToString();
        public string PercentageText => Math.Round(Percentage).ToString();
    }

    public class GoalStatistic
    {
        public string Name { get; set; } = "";
        public int Progress { get; set; }
        public string Status { get; set; } = "";
        
        // GameDataCollector?먯꽌 ?ъ슜?섎뒗 異붽? ?띿꽦??
        public string GoalName 
        { 
            get => Name; 
            set => Name = value; 
        }
        public int Current 
        { 
            get => Progress; 
            set => Progress = value; 
        }
        public int Target { get; set; }
        
        public string ProgressText => Target > 0 ? $"{Current}/{Target} ({Progress}%)" : $"{Current}";
        
        public void UpdateProgress(int current, int target)
        {
            Current = current;
            Target = target;
            Progress = target > 0 ? (int)((double)current / target * 100) : 0;
            Status = Progress >= 100 ? "Complete" : Progress >= 75 ? "Almost complete" : Progress >= 50 ? "In progress" : "Not started";
        }
    }
}






