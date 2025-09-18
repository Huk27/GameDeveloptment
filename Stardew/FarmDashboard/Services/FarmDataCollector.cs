using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FarmDashboard.Data;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
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
        private ActivityType _currentActivity = ActivityType.Other;
        private double _lastRecordedMilliseconds;
        private double _dayStartMilliseconds;
        private long _dayStartTotalMoney;
        private long _seasonStartTotalMoney;
        private bool _isReady;

        private string SeasonMoneyKey => $"{_uniqueKeyPrefix}/seasonStartMoney";

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
                _dayStartTotalMoney = Game1.player.totalMoneyEarned;
                _dayStartMilliseconds = Game1.player.millisecondsPlayed;
                ResetActivityTracking();

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
                UpdateEarnings();
                UpdateCropData();
                UpdateAnimalData();
                UpdateTimeData();
                UpdateGoals();
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
        }

        private void UpdateCropData()
        {
            var farm = Game1.getFarm();
            if (farm == null)
            {
                _snapshot.TotalCrops = 0;
                _snapshot.ReadyCrops = 0;
                _snapshot.GrowingCrops = 0;
                _snapshot.WiltedCrops = 0;
                return;
            }

            int total = 0;
            int ready = 0;
            int growing = 0;
            int wilted = 0;

            foreach (var feature in farm.terrainFeatures.Values)
            {
                if (feature is HoeDirt dirt && dirt.crop != null)
                {
                    total++;
                    if (dirt.crop.dead.Value)
                    {
                        wilted++;
                        continue;
                    }

                    if (IsCropReadyToHarvest(dirt))
                        ready++;
                    else
                        growing++;
                }
            }

            _snapshot.TotalCrops = total;
            _snapshot.ReadyCrops = ready;
            _snapshot.GrowingCrops = growing;
            _snapshot.WiltedCrops = wilted;
        }

        private bool IsCropReadyToHarvest(HoeDirt dirt)
        {
            var crop = dirt.crop;
            if (crop == null)
                return false;

            if (crop.dead.Value)
                return false;

            if (crop.currentPhase.Value >= crop.phaseDays.Count - 1 && !crop.fullyGrown.Value)
                return true;

            if (crop.fullyGrown.Value && crop.dayOfCurrentPhase.Value <= 0)
                return true;

            if (crop.regrowAfterHarvest.Value >= 0 && crop.dayOfCurrentPhase.Value <= 0)
                return true;

            return false;
        }

        private void UpdateAnimalData()
        {
            var farm = Game1.getFarm();
            if (farm == null)
            {
                _snapshot.TotalAnimals = 0;
                _snapshot.AdultAnimals = 0;
                _snapshot.BabyAnimals = 0;
                _snapshot.AverageAnimalHappiness = 0;
                _snapshot.ProductsReady = 0;
                return;
            }

            int total = 0;
            int adults = 0;
            int babies = 0;
            float happinessSum = 0;
            int productsReady = 0;

            foreach (var building in farm.buildings)
            {
                if (building.indoors.Value is not AnimalHouse house)
                    continue;

                foreach (var animal in house.animals.Values)
                {
                    total++;
                    happinessSum += animal.happiness.Value;

                    if (animal.age.Value >= animal.ageWhenMature.Value)
                        adults++;
                    else
                        babies++;

                    if (animal.currentProduce.Value > 0 || animal.fullness.Value < 200)
                    {
                        if (animal.currentProduce.Value > 0)
                            productsReady++;
                    }
                }
            }

            _snapshot.TotalAnimals = total;
            _snapshot.AdultAnimals = adults;
            _snapshot.BabyAnimals = babies;
            _snapshot.AverageAnimalHappiness = total > 0 ? happinessSum / total / 255f * 100f : 0f;
            _snapshot.ProductsReady = productsReady;
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
        }

        private void UpdateGoals()
        {
            var goals = new List<GoalProgress>
            {
                new()
                {
                    Id = "daily_earnings",
                    Name = "일일 수익 목표",
                    Description = "오늘 10,000g 이상을 벌어보세요.",
                    Category = GoalCategory.Economic,
                    CurrentValue = _snapshot.TodayEarnings,
                    TargetValue = 10000,
                    Status = _snapshot.TodayEarnings >= 10000 ? GoalStatus.Completed : GoalStatus.Active
                },
                new()
                {
                    Id = "animal_happiness",
                    Name = "동물 행복 유지",
                    Description = "동물 행복도를 80% 이상 유지하세요.",
                    Category = GoalCategory.Animals,
                    CurrentValue = (int)Math.Round(_snapshot.AverageAnimalHappiness),
                    TargetValue = 80,
                    Status = _snapshot.AverageAnimalHappiness >= 80 ? GoalStatus.Completed : GoalStatus.Active
                },
                new()
                {
                    Id = "harvest_ready",
                    Name = "수확 준비",
                    Description = "수확 가능한 작물을 20개 이상 준비하세요.",
                    Category = GoalCategory.Farming,
                    CurrentValue = _snapshot.ReadyCrops,
                    TargetValue = 20,
                    Status = _snapshot.ReadyCrops >= 20 ? GoalStatus.Completed : GoalStatus.Active
                }
            };

            _snapshot.Goals = goals;
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
                SkullCavern => ActivityType.Mining,
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
    }
}
