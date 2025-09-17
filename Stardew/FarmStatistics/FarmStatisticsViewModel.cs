using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley;

namespace FarmStatistics
{
    /// <summary>
    /// 농장 통계 데이터를 나타내는 클래스
    /// </summary>
    public class CropStatistic
    {
        public string CropName { get; set; } = "";
        public int Harvested { get; set; }
        public int Revenue { get; set; }
        public float GrowthTime { get; set; }
        public string Quality { get; set; } = "";
        public Tuple<Texture2D, Rectangle>? Sprite { get; set; }
    }

    public class AnimalStatistic
    {
        public string AnimalName { get; set; } = "";
        public int Products { get; set; }
        public int Revenue { get; set; }
        public float Happiness { get; set; }
        public Tuple<Texture2D, Rectangle>? Sprite { get; set; }
    }

    public class TimeStatistic : INotifyPropertyChanged
    {
        public string Activity { get; set; } = "";
        public int Hours { get; set; }
        public float Percentage { get; set; }
        public string Color { get; set; } = "#FFFFFF";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class GoalStatistic : INotifyPropertyChanged
    {
        public string GoalName { get; set; } = "";
        public int Current { get; set; }
        public int Target { get; set; }
        public float Progress { get; set; }
        public string ProgressText { get; set; } = "";

        public GoalStatistic()
        {
            UpdateProgress();
        }

        public void UpdateProgress()
        {
            Progress = Target > 0 ? (float)Current / Target * 100f : 0f;
            ProgressText = $"{Current}/{Target} ({Progress:F0}%)";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// 탭 데이터를 나타내는 클래스
    /// </summary>
    public class TabData : INotifyPropertyChanged
    {
        public string Name { get; }
        public string DisplayName { get; }
        public Tuple<Texture2D, Rectangle> Sprite { get; }
        
        private bool _active = false;
        public bool Active 
        { 
            get => _active; 
            set => SetField(ref _active, value); 
        }

        public TabData(string name, string displayName, Texture2D texture, Rectangle sourceRect)
        {
            Name = name;
            DisplayName = displayName;
            Sprite = Tuple.Create(texture, sourceRect);
        }

        #region Property Changes
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }

    /// <summary>
    /// 농장 통계 ViewModel
    /// </summary>
    public class FarmStatisticsViewModel : INotifyPropertyChanged
    {
        private readonly GameDataCollector _dataCollector;
        
        // Phase 2: 생성자 추가 (실제 데이터 콜렉터 주입)
        public FarmStatisticsViewModel(GameDataCollector dataCollector = null)
        {
            _dataCollector = dataCollector;
            InitializeTabs();
        }
        
        // 탭 시스템
        public IReadOnlyList<TabData> Tabs { get; set; } = new List<TabData>();
        public string SelectedTab { get; set; } = "overview";
        
        // 탭별 표시 여부를 위한 boolean 프로퍼티들
        public bool ShowOverviewTab => SelectedTab == "overview";
        public bool ShowCropsTab => SelectedTab == "crops";
        public bool ShowAnimalsTab => SelectedTab == "animals";
        public bool ShowTimeTab => SelectedTab == "time";
        public bool ShowGoalsTab => SelectedTab == "goals";

        // 개요 탭 데이터
        public string TotalEarnings { get; set; } = "1,250,000g";
        public string TotalCropsHarvested { get; set; } = "1,250개";
        public string TotalAnimalProducts { get; set; } = "450개";
        public string TotalPlayTime { get; set; } = "45시간 30분";
        public string SeasonComparison { get; set; } = "이번 계절: +15% 증가";

        // 작물 통계 데이터
        public IReadOnlyList<CropStatistic> CropStatistics { get; set; } = new List<CropStatistic>();
        public string CropsHeaderText => $"작물 통계 ({CropStatistics.Count}종)";

        // 동물 통계 데이터
        public IReadOnlyList<AnimalStatistic> AnimalStatistics { get; set; } = new List<AnimalStatistic>();
        public string AnimalsHeaderText => $"동물 통계 ({AnimalStatistics.Count}종)";

        // 시간 통계 데이터
        public IReadOnlyList<TimeStatistic> TimeStatistics { get; set; } = new List<TimeStatistic>();
        public string TimeHeaderText => "활동별 시간 통계";

        // 목표 통계 데이터
        public IReadOnlyList<GoalStatistic> GoalStatistics { get; set; } = new List<GoalStatistic>();
        public string GoalsHeaderText => $"목표 진행률 ({GoalStatistics.Count}개)";

        #region Property Changes
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion

        public void OnTabActivated(string name)
        {
            SelectedTab = name;
            foreach (var tab in Tabs)
            {
                if (tab.Name != name)
                {
                    tab.Active = false;
                }
            }
            OnPropertyChanged(nameof(SelectedTab));
            
            // 탭 표시 여부 프로퍼티들도 업데이트
            OnPropertyChanged(nameof(ShowOverviewTab));
            OnPropertyChanged(nameof(ShowCropsTab));
            OnPropertyChanged(nameof(ShowAnimalsTab));
            OnPropertyChanged(nameof(ShowTimeTab));
            OnPropertyChanged(nameof(ShowGoalsTab));
        }

        public static FarmStatisticsViewModel LoadDemoData()
        {
            var viewModel = new FarmStatisticsViewModel();
            viewModel.InitializeTabs();
            viewModel.LoadDemoStatistics();
            return viewModel;
        }

        private void InitializeTabs()
        {
            var tabs = new List<TabData>();
            var mouseCursors = Game1.mouseCursors;
            
            tabs.Add(new TabData("overview", "개요", mouseCursors, new Rectangle(211, 428, 7, 6)));
            tabs.Add(new TabData("crops", "작물", mouseCursors, new Rectangle(0, 428, 10, 10)));
            tabs.Add(new TabData("animals", "동물", mouseCursors, new Rectangle(10, 428, 10, 10)));
            tabs.Add(new TabData("time", "시간", mouseCursors, new Rectangle(60, 428, 10, 10)));
            tabs.Add(new TabData("goals", "목표", mouseCursors, new Rectangle(70, 428, 10, 10)));
            
            Tabs = tabs;
            
            if (Tabs.Count > 0)
            {
                OnTabActivated("overview");
            }
        }

        private void LoadDemoStatistics()
        {
            // 작물 통계 데모 데이터
            var cropStats = new List<CropStatistic>
            {
                new CropStatistic { CropName = "감자", Harvested = 150, Revenue = 45000, GrowthTime = 6, Quality = "금", Sprite = GetCropSprite("Potato") },
                new CropStatistic { CropName = "토마토", Harvested = 120, Revenue = 72000, GrowthTime = 11, Quality = "은", Sprite = GetCropSprite("Tomato") },
                new CropStatistic { CropName = "옥수수", Harvested = 200, Revenue = 60000, GrowthTime = 14, Quality = "일반", Sprite = GetCropSprite("Corn") },
                new CropStatistic { CropName = "호박", Harvested = 80, Revenue = 80000, GrowthTime = 13, Quality = "이리듐", Sprite = GetCropSprite("Pumpkin") },
                new CropStatistic { CropName = "딸기", Harvested = 300, Revenue = 90000, GrowthTime = 8, Quality = "금", Sprite = GetCropSprite("Strawberry") }
            };
            CropStatistics = cropStats;

            // 동물 통계 데모 데이터
            var animalStats = new List<AnimalStatistic>
            {
                new AnimalStatistic { AnimalName = "소", Products = 45, Revenue = 22500, Happiness = 85.5f, Sprite = GetAnimalSprite("Cow") },
                new AnimalStatistic { AnimalName = "닭", Products = 120, Revenue = 18000, Happiness = 92.0f, Sprite = GetAnimalSprite("Chicken") },
                new AnimalStatistic { AnimalName = "양", Products = 30, Revenue = 15000, Happiness = 78.0f, Sprite = GetAnimalSprite("Sheep") },
                new AnimalStatistic { AnimalName = "돼지", Products = 25, Revenue = 37500, Happiness = 88.5f, Sprite = GetAnimalSprite("Pig") }
            };
            AnimalStatistics = animalStats;

            // 시간 통계 데모 데이터
            var timeStats = new List<TimeStatistic>
            {
                new TimeStatistic { Activity = "농업", Hours = 18, Percentage = 40.0f, Color = "#4AFF4A" },
                new TimeStatistic { Activity = "채광", Hours = 12, Percentage = 26.7f, Color = "#FFA500" },
                new TimeStatistic { Activity = "낚시", Hours = 8, Percentage = 17.8f, Color = "#4A9EFF" },
                new TimeStatistic { Activity = "전투", Hours = 4, Percentage = 8.9f, Color = "#FF4A4A" },
                new TimeStatistic { Activity = "채집", Hours = 3, Percentage = 6.7f, Color = "#9E4AFF" }
            };
            TimeStatistics = timeStats;

            // 목표 통계 데모 데이터
            var goalStats = new List<GoalStatistic>();
            
            var goal1 = new GoalStatistic { GoalName = "일일 수익", Current = 8500, Target = 10000 };
            goal1.UpdateProgress();
            goalStats.Add(goal1);
            
            var goal2 = new GoalStatistic { GoalName = "작물 수확", Current = 60, Target = 100 };
            goal2.UpdateProgress();
            goalStats.Add(goal2);
            
            var goal3 = new GoalStatistic { GoalName = "동물 제품", Current = 25, Target = 50 };
            goal3.UpdateProgress();
            goalStats.Add(goal3);
            
            var goal4 = new GoalStatistic { GoalName = "플레이 시간", Current = 2, Target = 3 };
            goal4.UpdateProgress();
            goalStats.Add(goal4);
            GoalStatistics = goalStats;
        }

        private Tuple<Texture2D, Rectangle>? GetCropSprite(string cropName)
        {
            // 실제 게임에서 작물 스프라이트를 가져오는 로직
            // 데모용으로는 null 반환
            return null;
        }

        private Tuple<Texture2D, Rectangle>? GetAnimalSprite(string animalName)
        {
            // 실제 게임에서 동물 스프라이트를 가져오는 로직
            // 데모용으로는 null 반환
            return null;
        }

        /// <summary>
        /// 실제 게임 데이터로 통계를 업데이트합니다
        /// </summary>
        public void UpdateData()
        {
            try
            {
                // Phase 2: 주입된 데이터 콜렉터 사용 또는 기본값
                var dataCollector = _dataCollector ?? new GameDataCollector(null);
                var farmData = dataCollector.CollectCurrentData();
                
                // 개요 데이터 업데이트
                UpdateOverviewData(farmData.OverviewData);
                
                // 작물 데이터 업데이트
                CropStatistics = farmData.CropStatistics;
                OnPropertyChanged(nameof(CropStatistics));
                OnPropertyChanged(nameof(CropsHeaderText));
                
                // 동물 데이터 업데이트
                AnimalStatistics = farmData.AnimalStatistics;
                OnPropertyChanged(nameof(AnimalStatistics));
                OnPropertyChanged(nameof(AnimalsHeaderText));
                
                // 시간 데이터 업데이트
                TimeStatistics = farmData.TimeStatistics;
                OnPropertyChanged(nameof(TimeStatistics));
                
                // 목표 데이터 업데이트
                GoalStatistics = farmData.GoalStatistics;
                OnPropertyChanged(nameof(GoalStatistics));
                OnPropertyChanged(nameof(GoalsHeaderText));
            }
            catch (Exception ex)
            {
                // Phase 2: 오류 발생 시 로그 출력 및 기본 데이터 로드
                System.Console.WriteLine($"Phase 2 - FarmStatistics 데이터 업데이트 오류: {ex.Message}");
                
                // 기본 데이터로 폴백
                LoadDemoData();
            }
        }

        /// <summary>
        /// 개요 탭 데이터를 업데이트합니다
        /// </summary>
        private void UpdateOverviewData(OverviewData overviewData)
        {
            TotalEarnings = overviewData.TotalEarnings;
            TotalCropsHarvested = overviewData.TotalCropsHarvested;
            TotalAnimalProducts = overviewData.TotalAnimalProducts;
            TotalPlayTime = overviewData.TotalPlayTime;
            SeasonComparison = overviewData.SeasonComparison;
            
            // 개요 탭 프로퍼티들 변경 알림
            OnPropertyChanged(nameof(TotalEarnings));
            OnPropertyChanged(nameof(TotalCropsHarvested));
            OnPropertyChanged(nameof(TotalAnimalProducts));
            OnPropertyChanged(nameof(TotalPlayTime));
            OnPropertyChanged(nameof(SeasonComparison));
        }
    }
}
