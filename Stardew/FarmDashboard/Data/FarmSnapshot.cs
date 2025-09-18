using System;
using System.Collections.Generic;

namespace FarmDashboard.Data
{
    public class FarmSnapshot
    {
        public int TodayEarnings { get; set; }
        public int SeasonEarnings { get; set; }
        public int LifetimeEarnings { get; set; }

        public int TotalCrops { get; set; }
        public int ReadyCrops { get; set; }
        public int GrowingCrops { get; set; }
        public int WiltedCrops { get; set; }

        public int TotalAnimals { get; set; }
        public int AdultAnimals { get; set; }
        public int BabyAnimals { get; set; }
        public float AverageAnimalHappiness { get; set; }
        public int ProductsReady { get; set; }

        public TimeSpan TodayPlayTime { get; set; }
        public List<ActivityBreakdownEntry> ActivityBreakdown { get; set; } = new();
        public float GoldPerHour { get; set; }

        public List<GoalProgress> Goals { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public List<DailyFlowEntry> DailyEarnings { get; set; } = new();
        public List<string> Highlights { get; set; } = new();
        public List<FarmHealthMetric> FarmHealthMetrics { get; set; } = new();
        public List<RevenueStreamEntry> RevenueStreams { get; set; } = new();
        public List<ExpenseEntry> ExpenseEntries { get; set; } = new();
        public List<ProductPerformanceEntry> TopProducts { get; set; } = new();
        public List<CropStatusEntry> CropStatuses { get; set; } = new();
        public List<HarvestForecastEntry> HarvestForecasts { get; set; } = new();
        public List<AnimalStatusEntry> AnimalStatuses { get; set; } = new();
        public List<CareTaskEntry> CareTasks { get; set; } = new();
        public ExplorationSnapshot Exploration { get; set; } = new();
        public List<AlertEntry> Alerts { get; set; } = new();
        public List<CustomGoalEntry> CustomGoals { get; set; } = new();
        public WeatherSnapshot Weather { get; set; } = new();
        public LuckSnapshot Luck { get; set; } = new();
        public int WalletBalance { get; set; }

        public FarmSnapshot()
        {
        }

        private FarmSnapshot(FarmSnapshot other)
        {
            TodayEarnings = other.TodayEarnings;
            SeasonEarnings = other.SeasonEarnings;
            LifetimeEarnings = other.LifetimeEarnings;
            TotalCrops = other.TotalCrops;
            ReadyCrops = other.ReadyCrops;
            GrowingCrops = other.GrowingCrops;
            WiltedCrops = other.WiltedCrops;
            TotalAnimals = other.TotalAnimals;
            AdultAnimals = other.AdultAnimals;
            BabyAnimals = other.BabyAnimals;
            AverageAnimalHappiness = other.AverageAnimalHappiness;
            ProductsReady = other.ProductsReady;
            TodayPlayTime = other.TodayPlayTime;
            GoldPerHour = other.GoldPerHour;
            LastUpdated = other.LastUpdated;

            ActivityBreakdown = CloneList(other.ActivityBreakdown, e => e.Clone());
            Goals = CloneList(other.Goals, g => g.Clone());
            DailyEarnings = CloneList(other.DailyEarnings, e => e.Clone());
            Highlights = new List<string>(other.Highlights);
            FarmHealthMetrics = CloneList(other.FarmHealthMetrics, m => m.Clone());
            RevenueStreams = CloneList(other.RevenueStreams, r => r.Clone());
            ExpenseEntries = CloneList(other.ExpenseEntries, e => e.Clone());
            TopProducts = CloneList(other.TopProducts, p => p.Clone());
            CropStatuses = CloneList(other.CropStatuses, c => c.Clone());
            HarvestForecasts = CloneList(other.HarvestForecasts, h => h.Clone());
            AnimalStatuses = CloneList(other.AnimalStatuses, a => a.Clone());
            CareTasks = CloneList(other.CareTasks, c => c.Clone());
            Exploration = other.Exploration.Clone();
            Alerts = CloneList(other.Alerts, a => a.Clone());
            CustomGoals = CloneList(other.CustomGoals, g => g.Clone());
            Weather = other.Weather.Clone();
            Luck = other.Luck.Clone();
            WalletBalance = other.WalletBalance;
        }

        public FarmSnapshot Clone() => new(this);

        private static List<T> CloneList<T>(IEnumerable<T> source, Func<T, T> clone)
        {
            var list = new List<T>();
            if (source == null)
                return list;

            foreach (var item in source)
                list.Add(clone(item));

            return list;
        }
    }

    public class ActivityBreakdownEntry
    {
        public ActivityType Activity { get; set; }
        public TimeSpan TimeSpent { get; set; }
        public float Percentage { get; set; }

        public ActivityBreakdownEntry()
        {
        }

        private ActivityBreakdownEntry(ActivityBreakdownEntry other)
        {
            Activity = other.Activity;
            TimeSpent = other.TimeSpent;
            Percentage = other.Percentage;
        }

        public ActivityBreakdownEntry Clone() => new(this);
    }

    public class GoalProgress
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public GoalCategory Category { get; set; } = GoalCategory.Other;
        public int CurrentValue { get; set; }
        public int TargetValue { get; set; }
        public GoalStatus Status { get; set; } = GoalStatus.Active;
        public float Percentage => TargetValue == 0 ? 0f : Math.Clamp((float)CurrentValue / TargetValue * 100f, 0f, 100f);

        public GoalProgress()
        {
        }

        private GoalProgress(GoalProgress other)
        {
            Id = other.Id;
            Name = other.Name;
            Description = other.Description;
            Category = other.Category;
            CurrentValue = other.CurrentValue;
            TargetValue = other.TargetValue;
            Status = other.Status;
        }

        public GoalProgress Clone() => new(this);
    }

    public class DailyFlowEntry
    {
        public string Label { get; set; } = string.Empty;
        public int Earnings { get; set; }
        public int Expenses { get; set; }
        public int Net => Earnings - Expenses;

        public DailyFlowEntry()
        {
        }

        private DailyFlowEntry(DailyFlowEntry other)
        {
            Label = other.Label;
            Earnings = other.Earnings;
            Expenses = other.Expenses;
        }

        public DailyFlowEntry Clone() => new(this);
    }

    public class FarmHealthMetric
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public float Percentage { get; set; }
        public string Severity { get; set; } = string.Empty;

        public FarmHealthMetric()
        {
        }

        private FarmHealthMetric(FarmHealthMetric other)
        {
            Label = other.Label;
            Value = other.Value;
            Percentage = other.Percentage;
            Severity = other.Severity;
        }

        public FarmHealthMetric Clone() => new(this);
    }

    public class RevenueStreamEntry
    {
        public string Label { get; set; } = string.Empty;
        public int Amount { get; set; }
        public float Percentage { get; set; }

        public RevenueStreamEntry()
        {
        }

        private RevenueStreamEntry(RevenueStreamEntry other)
        {
            Label = other.Label;
            Amount = other.Amount;
            Percentage = other.Percentage;
        }

        public RevenueStreamEntry Clone() => new(this);
    }

    public class ExpenseEntry
    {
        public string Label { get; set; } = string.Empty;
        public int Amount { get; set; }

        public ExpenseEntry()
        {
        }

        private ExpenseEntry(ExpenseEntry other)
        {
            Label = other.Label;
            Amount = other.Amount;
        }

        public ExpenseEntry Clone() => new(this);
    }

    public class ProductPerformanceEntry
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int TotalValue { get; set; }

        public ProductPerformanceEntry()
        {
        }

        private ProductPerformanceEntry(ProductPerformanceEntry other)
        {
            Name = other.Name;
            Quantity = other.Quantity;
            TotalValue = other.TotalValue;
        }

        public ProductPerformanceEntry Clone() => new(this);
    }

    public class CropStatusEntry
    {
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int ReadyCount { get; set; }
        public int GrowingCount { get; set; }
        public int WiltedCount { get; set; }
        public int DaysUntilNextHarvest { get; set; }

        public CropStatusEntry()
        {
        }

        private CropStatusEntry(CropStatusEntry other)
        {
            Name = other.Name;
            Location = other.Location;
            ReadyCount = other.ReadyCount;
            GrowingCount = other.GrowingCount;
            WiltedCount = other.WiltedCount;
            DaysUntilNextHarvest = other.DaysUntilNextHarvest;
        }

        public CropStatusEntry Clone() => new(this);
    }

    public class HarvestForecastEntry
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int DaysUntilReady { get; set; }
        public string Location { get; set; } = string.Empty;

        public HarvestForecastEntry()
        {
        }

        private HarvestForecastEntry(HarvestForecastEntry other)
        {
            Name = other.Name;
            Quantity = other.Quantity;
            DaysUntilReady = other.DaysUntilReady;
            Location = other.Location;
        }

        public HarvestForecastEntry Clone() => new(this);
    }

    public class AnimalStatusEntry
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public bool IsAdult { get; set; }
        public float Happiness { get; set; }
        public bool ProduceReady { get; set; }
        public string Mood { get; set; } = string.Empty;

        public AnimalStatusEntry()
        {
        }

        private AnimalStatusEntry(AnimalStatusEntry other)
        {
            Name = other.Name;
            Type = other.Type;
            Building = other.Building;
            IsAdult = other.IsAdult;
            Happiness = other.Happiness;
            ProduceReady = other.ProduceReady;
            Mood = other.Mood;
        }

        public AnimalStatusEntry Clone() => new(this);
    }

    public class CareTaskEntry
    {
        public string Label { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string Details { get; set; } = string.Empty;

        public CareTaskEntry()
        {
        }

        private CareTaskEntry(CareTaskEntry other)
        {
            Label = other.Label;
            Completed = other.Completed;
            Details = other.Details;
        }

        public CareTaskEntry Clone() => new(this);
    }

    public class ExplorationSnapshot
    {
        public int DeepestMineLevel { get; set; }
        public int DeepestSkullCavernLevel { get; set; }
        public int VolcanoDepth { get; set; }
        public List<string> ForagingProgress { get; set; } = new();
        public List<string> FishingHighlights { get; set; } = new();
        public string TomorrowPlan { get; set; } = string.Empty;

        public ExplorationSnapshot()
        {
        }

        private ExplorationSnapshot(ExplorationSnapshot other)
        {
            DeepestMineLevel = other.DeepestMineLevel;
            DeepestSkullCavernLevel = other.DeepestSkullCavernLevel;
            VolcanoDepth = other.VolcanoDepth;
            ForagingProgress = new List<string>(other.ForagingProgress);
            FishingHighlights = new List<string>(other.FishingHighlights);
            TomorrowPlan = other.TomorrowPlan;
        }

        public ExplorationSnapshot Clone() => new(this);
    }

    public class AlertEntry
    {
        public string Label { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        public AlertEntry()
        {
        }

        private AlertEntry(AlertEntry other)
        {
            Label = other.Label;
            Category = other.Category;
        }

        public AlertEntry Clone() => new(this);
    }

    public class CustomGoalEntry
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public float Progress { get; set; }

        public CustomGoalEntry()
        {
        }

        private CustomGoalEntry(CustomGoalEntry other)
        {
            Name = other.Name;
            Description = other.Description;
            Progress = other.Progress;
        }

        public CustomGoalEntry Clone() => new(this);
    }

    public class WeatherSnapshot
    {
        public string Today { get; set; } = string.Empty;
        public string Tomorrow { get; set; } = string.Empty;
        public string Forecast { get; set; } = string.Empty;

        public WeatherSnapshot()
        {
        }

        private WeatherSnapshot(WeatherSnapshot other)
        {
            Today = other.Today;
            Tomorrow = other.Tomorrow;
            Forecast = other.Forecast;
        }

        public WeatherSnapshot Clone() => new(this);
    }

    public class LuckSnapshot
    {
        public string Level { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public LuckSnapshot()
        {
        }

        private LuckSnapshot(LuckSnapshot other)
        {
            Level = other.Level;
            Description = other.Description;
        }

        public LuckSnapshot Clone() => new(this);
    }

    public enum GoalStatus
    {
        Active,
        Completed,
        Failed
    }

    public enum GoalCategory
    {
        Farming,
        Animals,
        Mining,
        Fishing,
        Combat,
        Social,
        Economic,
        Exploration,
        Other
    }

    public enum ActivityType
    {
        Farming,
        Mining,
        Fishing,
        Combat,
        Foraging,
        Social,
        Other
    }
}
