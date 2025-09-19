
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using FarmDashboard.Data;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace FarmDashboard.Services
{
    public class FarmDataCollector
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly string _uniqueKeyPrefix;
        private readonly object _syncRoot = new();

        private FarmSnapshot _snapshot = new();
        private readonly Dictionary<ActivityType, double> _activityMillis = new();
        private readonly Dictionary<long, AnimalTrackerEntry> _animalTracker = new();
        private ActivityType _currentActivity = ActivityType.Other;
        private double _lastRecordedMilliseconds;
        private double _dayStartMilliseconds;
        private long _dayStartTotalMoney;
        private long _seasonStartTotalMoney;
        private bool _isReady;

        private int _unwateredCropTiles;
        private int _unpettedAnimalCount;
        private int _readyCropValue;
        private int _readyAnimalValue;
        private int _estimatedReplantCost;
        private readonly Dictionary<string, CropInsightAggregator> _cropInsightAggregators = new();

        private string SeasonMoneyKey => $"{_uniqueKeyPrefix}/seasonStartMoney";
        private string DailyHistoryKey => $"{_uniqueKeyPrefix}/dailyEarningsHistory";
        private const int DailyHistoryLength = 7;

        public FarmDataCollector(IModHelper helper, IMonitor monitor, string uniqueId)
        {
            _helper = helper;
            _monitor = monitor;
            _uniqueKeyPrefix = uniqueId;

            foreach (ActivityType type in Enum.GetValues<ActivityType>())
                _activityMillis[type] = 0;
        }
        public void InitializeForSave()
        {
            if (!Context.IsWorldReady || Game1.player == null)
                return;

            lock (_syncRoot)
            {
                _snapshot = new FarmSnapshot();
                _animalTracker.Clear();
                ResetActivityTracking();
                _dayStartTotalMoney = Game1.player.totalMoneyEarned;
                _seasonStartTotalMoney = ReadSeasonStartMoney();
                _dayStartMilliseconds = Game1.player.millisecondsPlayed;
                _isReady = true;
                RefreshAllData(forceFull: true);
            }
        }

        public void OnDayStarted()
        {
            if (!_isReady || Game1.player == null)
                return;

            lock (_syncRoot)
            {
                RecordPreviousDayEarnings();

                _dayStartTotalMoney = Game1.player.totalMoneyEarned;
                _dayStartMilliseconds = Game1.player.millisecondsPlayed;
                ResetActivityTracking();
                UpdateAnimalTrackerForNewDay();

                if (Game1.dayOfMonth == 1)
                {
                    _seasonStartTotalMoney = Game1.player.totalMoneyEarned;
                    WriteSeasonStartMoney(_seasonStartTotalMoney);
                }

                RefreshAllData(forceFull: true);
            }
        }

        public void RefreshTimeSensitiveData()
        {
            if (!_isReady)
                return;

            RefreshAllData(forceFull: false);
        }

        public void OnUpdateTicked(UpdateTickedEventArgs e)
        {
            if (!_isReady || !Context.IsWorldReady || Game1.player == null)
                return;

            double current = Game1.player.millisecondsPlayed;
            if (_lastRecordedMilliseconds <= 0)
                _lastRecordedMilliseconds = current;

            double delta = current - _lastRecordedMilliseconds;
            if (delta < 0)
                delta = 0;

            _activityMillis[_currentActivity] += delta;
            _currentActivity = DetectActivity();
            _lastRecordedMilliseconds = current;
        }

        public FarmSnapshot GetSnapshot()
        {
            lock (_syncRoot)
            {
                return _snapshot.Clone();
            }
        }

        private void RefreshAllData(bool forceFull)
        {
            if (!Context.IsWorldReady || Game1.player == null)
                return;

            lock (_syncRoot)
            {
                _unwateredCropTiles = 0;
                _unpettedAnimalCount = 0;
                _readyCropValue = 0;
                _readyAnimalValue = 0;
                _estimatedReplantCost = 0;

                UpdateEarnings();
                UpdateCropData();
                UpdateAnimalData();
                UpdateTimeData();
                UpdateGoals();
                UpdateDerivedSections();

                _snapshot.LastUpdated = DateTime.UtcNow;
            }
        }
        private void UpdateEarnings()
        {
            if (Game1.player == null)
                return;

            long total = Game1.player.totalMoneyEarned;
            _snapshot.LifetimeEarnings = (int)total;
            _snapshot.TodayEarnings = (int)Math.Max(0, total - _dayStartTotalMoney);
            _snapshot.SeasonEarnings = (int)Math.Max(0, total - _seasonStartTotalMoney);
            _snapshot.WalletBalance = Game1.player.Money;
        }

private void UpdateCropData()
        {
            _cropInsightAggregators.Clear();
            int total = 0;
            int ready = 0;
            int growing = 0;
            int wilted = 0;

            var groups = new Dictionary<string, CropAggregator>();

            foreach (var (location, dirt) in EnumerateCropTiles())
            {
                var crop = dirt.crop;
                if (crop == null)
                    continue;

                total++;

                string locationName = GetLocationDisplayName(location);
                string cropName = GetCropDisplayName(crop);
                string key = $"{cropName}::{locationName}";
                if (!groups.TryGetValue(key, out var aggregator))
                {
                    aggregator = new CropAggregator(cropName, locationName);
                    groups[key] = aggregator;
                }

                aggregator.TotalCount++;

                bool dry = IsDirtUnwatered(dirt, location);
                if (dry)
                    _unwateredCropTiles++;

                bool isDead = crop.dead.Value;
                bool isReady = !isDead && IsCropReadyToHarvest(dirt);

                if (isDead)
                {
                    wilted++;
                    aggregator.WiltedCount++;
                }
                else if (isReady)
                {
                    ready++;
                    aggregator.ReadyCount++;
                    int value = EstimateCropValue(crop, dirt);
                    aggregator.ReadyValue += value;
                    _readyCropValue += value;
                }
                else
                {
                    growing++;
                    aggregator.GrowingCount++;
                    int days = GetDaysUntilHarvest(crop, dirt);
                    if (days >= 0)
                    {
                        if (!aggregator.DaysUntilNext.HasValue || days < aggregator.DaysUntilNext.Value)
                            aggregator.DaysUntilNext = days;
                    }
                }

                CollectCropInsights(aggregator, crop, dirt, locationName, dry, isReady, isDead);
            }

            _snapshot.TotalCrops = total;
            _snapshot.ReadyCrops = ready;
            _snapshot.GrowingCrops = growing;
            _snapshot.WiltedCrops = wilted;

            int wiltedTotal = groups.Values.Sum(g => g.WiltedCount);
            _estimatedReplantCost = wiltedTotal * 100;

            _snapshot.CropStatuses = groups.Values
                .Select(g => g.ToStatusEntry())
                .OrderByDescending(e => e.ReadyCount)
                .ThenBy(e => e.DaysUntilNextHarvest >= 0 ? e.DaysUntilNextHarvest : int.MaxValue)
                .Take(40)
                .ToList();

            _snapshot.HarvestForecasts = groups.Values
                .Where(g => g.DaysUntilNext.HasValue && g.DaysUntilNext.Value > 0)
                .OrderBy(g => g.DaysUntilNext.Value)
                .ThenByDescending(g => g.GrowingCount)
                .Take(12)
                .Select(g => g.ToForecastEntry())
                .ToList();

            _snapshot.CropInsights = _cropInsightAggregators.ToDictionary(pair => pair.Key, pair => pair.Value.ToInsight());
        }

        private void UpdateAnimalTrackerForNewDay()
        {
            foreach (var tracker in _animalTracker.Values)
            {
                if (!tracker.PetToday)
                    tracker.ConsecutiveDaysUntended++;
                else
                    tracker.ConsecutiveDaysUntended = 0;

                tracker.PetToday = false;
                tracker.ProducedToday = false;
            }
        }

        private void TrackAnimal(FarmAnimal animal, bool producedReady)
        {
            if (animal == null)
                return;

            if (!_animalTracker.TryGetValue(animal.myID.Value, out var entry))
            {
                entry = new AnimalTrackerEntry { Id = animal.myID.Value };
                _animalTracker[animal.myID.Value] = entry;
            }

            entry.Name = animal.displayName ?? animal.Name ?? "Animal";
            entry.Type = animal.type.Value ?? string.Empty;
            if (animal.wasPet.Value)
                entry.PetToday = true;
            if (producedReady)
                entry.ProducedToday = true;
        }

        private void UpdateAnimalData()
        {
            var farm = Game1.getFarm();
            if (farm == null)
            {
                _snapshot.TotalAnimals = 0;
                _snapshot.AdultAnimals = 0;
                _snapshot.BabyAnimals = 0;
                _snapshot.AverageAnimalHappiness = 0f;
                _snapshot.ProductsReady = 0;
                _snapshot.AnimalStatuses = new List<AnimalStatusEntry>();
                return;
            }

            int total = 0;
            int adults = 0;
            int babies = 0;
            float happinessSum = 0f;
            int productsReady = 0;

            var statuses = new List<AnimalStatusEntry>();

            foreach (Building building in farm.buildings)
            {
                if (building?.indoors.Value is not AnimalHouse house)
                    continue;

                foreach (var animal in house.animals.Values)
                {
                    total++;
                    bool isAdult = !animal.isBaby();
                    if (isAdult)
                        adults++;
                    else
                        babies++;

                    float happiness = GetAnimalHappiness(animal);
                    happinessSum += happiness;

                    string produceId = animal.currentProduce.Value;
                    bool produceReady = !string.IsNullOrWhiteSpace(produceId) && produceId != "-1";
                    if (produceReady)
                    {
                        productsReady++;
                        _readyAnimalValue += EstimateAnimalProduceValue(animal);
                    }

                    if (!animal.wasPet.Value)
                        _unpettedAnimalCount++;

                    TrackAnimal(animal, produceReady);

                    var status = new AnimalStatusEntry
                    {
                        Id = animal.myID.Value,
                        Name = animal.displayName ?? animal.Name ?? "Animal",
                        Type = animal.type.Value ?? string.Empty,
                        Building = building.buildingType.Value ?? "Barn",
                        IsAdult = isAdult,
                        Happiness = happiness,
                        ProduceReady = produceReady,
                        Mood = DescribeAnimalMood(animal)
                    };
                    statuses.Add(status);
                }
            }

            _snapshot.TotalAnimals = total;
            _snapshot.AdultAnimals = adults;
            _snapshot.BabyAnimals = babies;
            _snapshot.AverageAnimalHappiness = total > 0 ? happinessSum / total : 0f;
            _snapshot.ProductsReady = productsReady;

            var activeIds = statuses.Select(s => s.Id).ToHashSet();
            foreach (var key in _animalTracker.Keys.ToList())
            {
                if (!activeIds.Contains(key))
                    _animalTracker.Remove(key);
            }

            UpdateAnimalInsights(statuses);

            _snapshot.AnimalStatuses = statuses
                .OrderByDescending(s => s.ProduceReady)
                .ThenBy(s => s.Name)
                .ToList();
        }

        private void UpdateTimeData()
        {
            if (Game1.player == null)
                return;

            double todayMillis = Math.Max(0, Game1.player.millisecondsPlayed - _dayStartMilliseconds);
            _snapshot.TodayPlayTime = TimeSpan.FromMilliseconds(todayMillis);

            double totalMillis = _activityMillis.Values.Sum();
            if (totalMillis < 10)
                totalMillis = todayMillis;

            _snapshot.ActivityBreakdown = _activityMillis
                .Select(pair => new ActivityBreakdownEntry
                {
                    Activity = pair.Key,
                    TimeSpent = TimeSpan.FromMilliseconds(pair.Value),
                    Percentage = totalMillis <= 0 ? 0 : (float)Math.Round(pair.Value / totalMillis * 100f, 1)
                })
                .OrderByDescending(entry => entry.TimeSpent)
                .ToList();

            double hours = todayMillis / 1000 / 60 / 60;
            _snapshot.GoldPerHour = hours <= 0 ? 0 : (float)Math.Round(_snapshot.TodayEarnings / hours, 1);

            _snapshot.ActivitySummary = BuildActivitySummary(_snapshot.ActivityBreakdown);
        }

        private void UpdateAnimalInsights(IEnumerable<AnimalStatusEntry> statuses)
        {
            var insights = new Dictionary<string, AnimalInsight>();
            foreach (var status in statuses)
            {
                string key = $"{status.Name}::{status.Type}";
                var insight = new AnimalInsight
                {
                    Id = status.Id,
                    Name = status.Name,
                    Type = status.Type,
                    Building = status.Building,
                    Happiness = status.Happiness,
                    ProduceReady = status.ProduceReady,
                    Mood = status.Mood,
                    Alerts = new List<string>()
                };

                if (status.Happiness < 60)
                    insight.Alerts.Add("Low happiness");
                if (!status.ProduceReady)
                    insight.Alerts.Add("No produce yet");

                if (_animalTracker.TryGetValue(status.Id, out var tracker))
                {
                    insight.ConsecutiveDaysUntended = tracker.ConsecutiveDaysUntended;
                    if (!tracker.PetToday)
                        insight.Alerts.Add("Needs petting");
                    if (!tracker.ProducedToday)
                        insight.Alerts.Add("No product yet");
                    if (tracker.ConsecutiveDaysUntended >= 2)
                        insight.Alerts.Add($"{tracker.ConsecutiveDaysUntended} day(s) without care");
                }

                insights[key] = insight;
            }

            _snapshot.AnimalInsights = insights;
        }

        private int EstimateCropValue(Crop crop, HoeDirt dirt)
        {
            try
            {
                var item = ItemRegistry.Create(crop.indexOfHarvest.Value);
                if (item == null)
                    return 0;

                int stack = Math.Max(1, crop.minHarvest.Value);
                item.Stack = stack;

                if (item is StardewValley.Object obj)
                {
                    obj.Quality = crop.indexOfHarvest.Value.StartsWith("(O)") ? obj.Quality : dirt.crop?.whichQuality ?? obj.Quality;
                }

                return Utility.getSellToStorePriceOfItem(item);
            }
            catch
            {
                return 0;
            }
        }

        private int EstimateAnimalProduceValue(FarmAnimal animal)
        {
            try
            {
                string produceId = animal.currentProduce.Value;\n                if (string.IsNullOrWhiteSpace(produceId) || produceId == "-1")
                    return 0;

                var item = CreateItem(produceId);\r\n                if (item is StardewValley.Object obj)\r\n                {\r\n                    obj.Stack = 1;\r\n                    obj.Quality = animal.produceQuality.Value;\r\n                    return Utility.getSellToStorePriceOfItem(obj);\r\n                }\r\n\r\n                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private float GetAnimalHappiness(FarmAnimal animal)
        {
            if (animal?.happiness?.Value == null)
                return 0f;

                        return (float)Math.Round(animal.happiness.Value / 255f * 100f, 2);
        }

        private string DescribeAnimalMood(FarmAnimal animal)
        {
            if (animal == null)
                return string.Empty;

            if (animal.fullness.Value < 200)
                return "Hungry";
            if (!animal.wasPet.Value)
                return "Needs affection";
            if (animal.happiness.Value >= 200)
                return "Ecstatic";
            if (animal.happiness.Value >= 150)
                return "Content";
            return "Moody";
        }
        private void ResetActivityTracking()
        {
            foreach (ActivityType type in Enum.GetValues<ActivityType>())
                _activityMillis[type] = 0;

            _currentActivity = DetectActivity();
            _lastRecordedMilliseconds = Game1.player?.millisecondsPlayed ?? 0;
        }

        private ActivityType DetectActivity()
        {
            if (!Context.IsWorldReady || Game1.player == null)
                return ActivityType.Other;

            var player = Game1.player;
            if (player.UsingTool && player.CurrentTool != null)
            {
                return player.CurrentTool switch
                {
                    Hoe => ActivityType.Farming,
                    WateringCan => ActivityType.Farming,
                    Axe => ActivityType.Foraging,
                    Pickaxe => ActivityType.Mining,
                    FishingRod => ActivityType.Fishing,
                    MeleeWeapon => ActivityType.Combat,
                    _ => ActivityType.Other
                };
            }

            var location = Game1.currentLocation;
            if (location == null)
                return ActivityType.Other;

            return location switch
            {
                Farm => ActivityType.Farming,
                FarmHouse => ActivityType.Social,
                Town => ActivityType.Social,
                Mountain => ActivityType.Foraging,
                Forest => ActivityType.Foraging,
                Woods => ActivityType.Foraging,
                Beach => ActivityType.Fishing,
                MineShaft => ActivityType.Mining,
                VolcanoDungeon => ActivityType.Combat,
                _ when location.waterTiles != null => ActivityType.Fishing,
                _ => ActivityType.Other
            };
        }

        private long ReadSeasonStartMoney()
        {
            if (Game1.player == null)
                return 0;

            string key = BuildSeasonKey();
            if (Game1.player.modData.TryGetValue(key, out var value) && long.TryParse(value, out var parsed))
                return parsed;

            long current = Game1.player.totalMoneyEarned;
            Game1.player.modData[key] = current.ToString(CultureInfo.InvariantCulture);
            return current;
        }

        private void WriteSeasonStartMoney(long value)
        {
            if (Game1.player == null)
                return;

            Game1.player.modData[BuildSeasonKey()] = value.ToString(CultureInfo.InvariantCulture);
        }

        private string BuildSeasonKey()
        {
            return $"{SeasonMoneyKey}/{Game1.year}/{Game1.currentSeason.ToLowerInvariant()}";
        }

        private class CropAggregator
        {
            public string Name { get; }
            public string Location { get; }
            public int TotalCount { get; set; }
            public int ReadyCount { get; set; }
            public int GrowingCount { get; set; }
            public int WiltedCount { get; set; }
            public int ReadyValue { get; set; }
            public int? DaysUntilNext { get; set; }
            public int MaxDaysSincePlanted { get; set; }
            public int TotalGrowthDays { get; set; }

            public CropAggregator(string name, string location)
            {
                Name = name;
                Location = location;
            }

            public CropStatusEntry ToStatusEntry()
            {
                return new CropStatusEntry
                {
                    Name = Name,
                    Location = Location,
                    ReadyCount = ReadyCount,
                    GrowingCount = GrowingCount,
                    WiltedCount = WiltedCount,
                    DaysUntilNextHarvest = ReadyCount > 0 ? 0 : DaysUntilNext ?? -1,
                    DaysSincePlanted = MaxDaysSincePlanted > 0 ? MaxDaysSincePlanted : null,
                    TotalGrowthDays = TotalGrowthDays > 0 ? TotalGrowthDays : null
                };
            }

            public HarvestForecastEntry ToForecastEntry()
            {
                int? expectedValue = ReadyCount > 0 ? (int?)Math.Max(0, ReadyValue / Math.Max(1, ReadyCount)) : null;
                return new HarvestForecastEntry
                {
                    Name = Name,
                    Quantity = GrowingCount,
                    DaysUntilReady = DaysUntilNext ?? -1,
                    Location = Location,
                    ExpectedValueEach = expectedValue
                };
            }
        }

    
        private ActivitySummarySnapshot BuildActivitySummary(IReadOnlyList<ActivityBreakdownEntry> breakdown)
        {
            var snapshot = new ActivitySummarySnapshot();
            if (breakdown == null || breakdown.Count == 0)
                return snapshot;

            var sorted = breakdown
                .Where(b => b.TimeSpent.TotalMinutes >= 1)
                .OrderByDescending(b => b.TimeSpent)
                .ToList();

            snapshot.Entries = sorted.Take(10).ToList();

            var suggestions = new List<ActivitySuggestion>();
            var top = sorted.FirstOrDefault();
            if (top != null)
            {
                string category = top.Activity.ToString();
                string message = top.Activity switch
                {
                    ActivityType.Farming => "Great farming push today — remember to hydrate crops before bed.",
                    ActivityType.Fishing => "Fishing day! Check the Fish Shop for tackle restock.",
                    ActivityType.Mining => "Lots of mining done. Smelt ores overnight for best turnaround.",
                    ActivityType.Foraging => "Foraging complete. Consider crafting field snacks for tomorrow.",
                    ActivityType.Combat => "Combat-heavy day. Repair gear and restock healing items.",
                    ActivityType.Social => "Social focus! Gift favorite items tomorrow for extra hearts.",
                    _ => "Balanced day — plan tomorrow around current luck and weather."
                };

                suggestions.Add(new ActivitySuggestion
                {
                    Title = $"Top activity: {category}",
                    Message = message,
                    Category = category
                });
            }

            float farmingPercent = sorted.Where(b => b.Activity == ActivityType.Farming).Sum(b => b.Percentage);
            float otherPercent = sorted.Where(b => b.Activity == ActivityType.Other).Sum(b => b.Percentage);
            if (otherPercent > 40)
            {
                suggestions.Add(new ActivitySuggestion
                {
                    Title = "Lots of idle time",
                    Message = "Large 'Other' category detected — consider planning chores or mining runs tomorrow.",
                    Category = "Other"
                });
            }

            if (farmingPercent > 60 && _unwateredCropTiles > 0)
            {
                suggestions.Add(new ActivitySuggestion
                {
                    Title = "Prioritise watering",
                    Message = "Heavy farming day, but some crops remain dry — adjust sprinkler coverage.",
                    Category = "Farming"
                });
            }

            snapshot.Suggestions = suggestions;
            return snapshot;
        }

    private class CropInsightAggregator
        {
            public string CropName { get; }
            public string Location { get; }
            public int TotalTiles { get; set; }
            public int ReadyCount { get; set; }
            public int WaterStressTiles { get; set; }
            public int WiltedCount { get; set; }
            public int EstimatedValue { get; set; }
            public int? DaysUntilFirstHarvest { get; set; }
            public int MaxDaysSincePlanted { get; set; }
            public int TotalGrowthDays { get; set; }
            private readonly List<string> _alerts = new();

            public CropInsightAggregator(string cropName, string location)
            {
                CropName = cropName;
                Location = location;
            }

            public void AddAlert(string message)
            {
                if (!string.IsNullOrWhiteSpace(message))
                    _alerts.Add(message);
            }

            public CropInsight ToInsight()
            {
                return new CropInsight
                {
                    CropName = CropName,
                    Location = Location,
                    ReadyCount = ReadyCount,
                    DaysUntilFirstHarvest = DaysUntilFirstHarvest ?? -1,
                    DaysSincePlanted = MaxDaysSincePlanted,
                    WaterStressTiles = WaterStressTiles,
                    EstimatedValue = EstimatedValue,
                    Alerts = BuildAlerts()
                };
            }

            private List<string> BuildAlerts()
            {
                var alerts = new List<string>(_alerts);

                if (WaterStressTiles > 0)
                    alerts.Add($"{WaterStressTiles} tile(s) dry");
                if (WiltedCount > 0)
                    alerts.Add($"{WiltedCount} tile(s) wilted");

                return alerts
                    .Where(a => !string.IsNullOrWhiteSpace(a))
                    .Distinct()
                    .ToList();
            }
        }
    }
}



