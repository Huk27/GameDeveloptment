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

            ActivityBreakdown = new List<ActivityBreakdownEntry>(other.ActivityBreakdown.Count);
            foreach (var entry in other.ActivityBreakdown)
            {
                ActivityBreakdown.Add(entry.Clone());
            }

            Goals = new List<GoalProgress>(other.Goals.Count);
            foreach (var goal in other.Goals)
            {
                Goals.Add(goal.Clone());
            }
        }

        public FarmSnapshot Clone() => new(this);
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
