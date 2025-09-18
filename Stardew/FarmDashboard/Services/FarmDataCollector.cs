
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
            int total = 0;
            int ready = 0;
            int growing = 0;
            int wilted = 0;

            var groups = new Dictionary<string, CropAggregator>();

            foreach (var (location, dirt) in EnumerateCropTiles())
            {
                total++;

                var crop = dirt.crop;
                string locationName = GetLocationDisplayName(location);
                string cropName = GetCropDisplayName(crop);
                string key = $"{cropName}::{locationName}";
                if (!groups.TryGetValue(key, out var aggregator))
                {
                    aggregator = new CropAggregator(cropName, locationName);
                    groups[key] = aggregator;
                }

                aggregator.TotalCount++;

                if (crop.dead.Value)
                {
                    wilted++;
                    aggregator.WiltedCount++;
                    continue;
                }

                bool isReady = IsCropReadyToHarvest(dirt);
                if (isReady)
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

                if (IsDirtUnwatered(dirt, location))
                    _unwateredCropTiles++;
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

                                        string produceId = animal.currentProduce.Value;\n                    bool produceReady = !string.IsNullOrWhiteSpace(produceId) && produceId != "-1";
                    if (produceReady)
                    {
                        productsReady++;
                        _readyAnimalValue += EstimateAnimalProduceValue(animal);
                    }

                    if (!animal.wasPet.Value)
                        _unpettedAnimalCount++;

                    var status = new AnimalStatusEntry
                    {
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
        }

        private void UpdateGoals()
        {
            var goals = new List<GoalProgress>
            {
                new()
                {
                    Id = "daily_earnings",
                    Name = "Daily Earnings Milestone",
                    Description = "Earn at least 10,000g today.",
                    Category = GoalCategory.Economic,
                    CurrentValue = _snapshot.TodayEarnings,
                    TargetValue = 10000,
                    Status = _snapshot.TodayEarnings >= 10000 ? GoalStatus.Completed : GoalStatus.Active
                },
                new()
                {
                    Id = "animal_happiness",
                    Name = "Animal Happiness",
                    Description = "Keep overall animal happiness at or above 80%.",
                    Category = GoalCategory.Animals,
                    CurrentValue = (int)Math.Round(_snapshot.AverageAnimalHappiness),
                    TargetValue = 80,
                    Status = _snapshot.AverageAnimalHappiness >= 80 ? GoalStatus.Completed : GoalStatus.Active
                },
                new()
                {
                    Id = "harvest_ready",
                    Name = "Harvest Ready",
                    Description = "Maintain at least 20 harvest-ready crops.",
                    Category = GoalCategory.Farming,
                    CurrentValue = _snapshot.ReadyCrops,
                    TargetValue = 20,
                    Status = _snapshot.ReadyCrops >= 20 ? GoalStatus.Completed : GoalStatus.Active
                }
            };

            _snapshot.Goals = goals;
        }
        private void UpdateDerivedSections()
        {
            UpdateDailyFlow();
            UpdateHighlights();
            UpdateCareTasks();
            UpdateEconomy();
            UpdateExploration();
            UpdateAlerts();
        }

        private void UpdateDailyFlow()
        {
            var history = LoadDailyHistory();

            var todayEntry = new DailyFlowEntry
            {
                Label = "Today",
                Earnings = Math.Max(0, _snapshot.TodayEarnings),
                Expenses = _snapshot.TodayEarnings < 0 ? Math.Abs(_snapshot.TodayEarnings) : 0
            };

            history.Insert(0, todayEntry);

            if (history.Count > DailyHistoryLength)
                history = history.Take(DailyHistoryLength).ToList();

            _snapshot.DailyEarnings = history;
        }

        private void UpdateHighlights()
        {
            var highlights = new List<string>();

            if (_snapshot.ReadyCrops > 0)
                highlights.Add($"{_snapshot.ReadyCrops} crops ready to harvest");

            if (_snapshot.ProductsReady > 0)
                highlights.Add($"{_snapshot.ProductsReady} animal products waiting");

            if (_readyCropValue + _readyAnimalValue > 0)
                highlights.Add($"Uncollected goods worth approximately {(_readyCropValue + _readyAnimalValue):N0}g");

            if (_unwateredCropTiles > 0)
                highlights.Add($"{_unwateredCropTiles} crop tiles need watering");

            if (_unpettedAnimalCount > 0)
                highlights.Add($"{_unpettedAnimalCount} animals need affection");

            if (highlights.Count == 0)
                highlights.Add("All quiet on the farm – great job!");

            _snapshot.Highlights = highlights;
        }

        private void UpdateCareTasks()
        {
            var tasks = new List<CareTaskEntry>
            {
                new()
                {
                    Label = "Collect ready produce",
                    Completed = _snapshot.ProductsReady == 0 && _snapshot.ReadyCrops == 0,
                    Details = _snapshot.ProductsReady + _snapshot.ReadyCrops == 0
                        ? "Nothing waiting"
                        : $"{_snapshot.ReadyCrops + _snapshot.ProductsReady} task(s) pending"
                },
                new()
                {
                    Label = "Water crops",
                    Completed = _unwateredCropTiles == 0,
                    Details = _unwateredCropTiles == 0 ? "All hydrated" : $"{_unwateredCropTiles} dry tiles"
                },
                new()
                {
                    Label = "Pet animals",
                    Completed = _unpettedAnimalCount == 0,
                    Details = _unpettedAnimalCount == 0 ? "Everyone is content" : $"{_unpettedAnimalCount} animal(s) waiting"
                },
                new()
                {
                    Label = "Plan replanting",
                    Completed = _estimatedReplantCost == 0,
                    Details = _estimatedReplantCost == 0 ? "Beds are healthy" : $"Approx. {_estimatedReplantCost:N0}g in seeds"
                }
            };

            _snapshot.CareTasks = tasks;

            _snapshot.FarmHealthMetrics = new List<FarmHealthMetric>
            {
                new()
                {
                    Label = "Fields hydrated",
                    Value = _snapshot.TotalCrops == 0 ? "--" : $"{100 - (int)Math.Round((double)_unwateredCropTiles / Math.Max(1, _snapshot.TotalCrops) * 100)}%",
                    Percentage = _snapshot.TotalCrops == 0 ? 1f : 1f - Math.Clamp((float)_unwateredCropTiles / Math.Max(1, _snapshot.TotalCrops), 0f, 1f),
                    Severity = _unwateredCropTiles > 0 ? "warning" : "good"
                },
                new()
                {
                    Label = "Animals happy",
                    Value = $"{_snapshot.AverageAnimalHappiness:F0}%",
                    Percentage = Math.Clamp(_snapshot.AverageAnimalHappiness / 100f, 0f, 1f),
                    Severity = _snapshot.AverageAnimalHappiness < 60 ? "warning" : "good"
                },
                new()
                {
                    Label = "Harvest captured",
                    Value = _snapshot.ReadyCrops + _snapshot.ProductsReady == 0 ? "0" : $"{_snapshot.ReadyCrops + _snapshot.ProductsReady}",
                    Percentage = (_snapshot.ReadyCrops + _snapshot.ProductsReady) == 0 ? 1f : 0.5f,
                    Severity = _snapshot.ReadyCrops + _snapshot.ProductsReady > 0 ? "warning" : "good"
                }
            };
        }

        private void UpdateEconomy()
        {
            var streams = new List<(string Label, int Amount)>
            {
                ("Today Earned", _snapshot.TodayEarnings),
                ("Ready Crops", _readyCropValue),
                ("Animal Products", _readyAnimalValue),
                ("Wallet", _snapshot.WalletBalance)
            };

            int total = streams.Sum(s => Math.Max(0, s.Amount));
            if (total <= 0)
                total = 1;

            _snapshot.RevenueStreams = streams
                .Select(s => new RevenueStreamEntry
                {
                    Label = s.Label,
                    Amount = Math.Max(0, s.Amount),
                    Percentage = (float)Math.Round(Math.Max(0, s.Amount) / total * 100f, 1)
                })
                .ToList();

            var expenses = new List<ExpenseEntry>
            {
                new()
                {
                    Label = "Season Earnings",
                    Amount = _snapshot.SeasonEarnings
                }
            };

            if (_estimatedReplantCost > 0)
            {
                expenses.Add(new ExpenseEntry
                {
                    Label = "Projected Replant Cost",
                    Amount = _estimatedReplantCost
                });
            }

            _snapshot.ExpenseEntries = expenses;

            var products = new List<ProductPerformanceEntry>();
            foreach (var status in _snapshot.CropStatuses.Where(s => s.ReadyCount > 0))
            {
                int approximateValue = status.ReadyCount > 0 && _snapshot.ReadyCrops > 0
                    ? (int)Math.Max(0, (double)_readyCropValue / Math.Max(1, _snapshot.ReadyCrops) * status.ReadyCount)
                    : 0;

                products.Add(new ProductPerformanceEntry
                {
                    Name = $"{status.Name} ({status.Location})",
                    Quantity = status.ReadyCount,
                    TotalValue = approximateValue
                });
            }

            int readyAnimals = _snapshot.AnimalStatuses.Count(a => a.ProduceReady);
            foreach (var animal in _snapshot.AnimalStatuses.Where(a => a.ProduceReady))
            {
                int approximateValue = readyAnimals > 0 ? _readyAnimalValue / readyAnimals : 0;
                products.Add(new ProductPerformanceEntry
                {
                    Name = $"{animal.Name} ({animal.Type})",
                    Quantity = 1,
                    TotalValue = approximateValue
                });
            }

            _snapshot.TopProducts = products
                .OrderByDescending(p => p.TotalValue)
                .ThenByDescending(p => p.Quantity)
                .Take(6)
                .ToList();
        }

        private void UpdateExploration()
        {
            var exploration = new ExplorationSnapshot
            {
                DeepestMineLevel = Game1.player?.deepestMineLevel ?? 0,
                DeepestSkullCavernLevel = MineShaft.lowestLevelReached,
                VolcanoDepth = Game1.player?.mailReceived.Contains("Island_VolcanoUnlocked") == true ? 10 : 0,
                TomorrowPlan = BuildTomorrowPlan()
            };

            string season = SDate.Now().Season;
            string seasonDisplay = string.IsNullOrEmpty(season)
                ? "Unknown"
                : char.ToUpperInvariant(season[0]) + (season.Length > 1 ? season.Substring(1) : string.Empty);

            exploration.ForagingProgress = new List<string>
            {
                $"Current season: {seasonDisplay}",
                $"Foraging level: {Game1.player?.ForagingLevel ?? 0}",
                $"Cooking recipes known: {Game1.player?.cookingRecipes.Count ?? 0}"
            };

            exploration.FishingHighlights = new List<string>
            {
                $"Fishing level: {Game1.player?.FishingLevel ?? 0}",
                $"Fish species caught: {Game1.player?.fishCaught.Count ?? 0}"
            };

            _snapshot.Exploration = exploration;
        }

        private void UpdateAlerts()
        {
            var alerts = new List<AlertEntry>();
            var today = SDate.Now();
            var tomorrow = today.AddDays(1);

            var birthdays = GetNpcBirthdays(tomorrow);
            if (birthdays.Count > 0)
            {
                alerts.Add(new AlertEntry
                {
                    Label = $"{tomorrow.ToLocaleString()}: {string.Join(", ", birthdays)} birthday",
                    Category = "Birthday"
                });
            }

            if (Utility.isFestivalDay(tomorrow.Day, tomorrow.Season))
            {
                alerts.Add(new AlertEntry
                {
                    Label = "Festival tomorrow",
                    Category = "Festival"
                });
            }

            if (Game1.player != null)
            {
                foreach (Quest quest in Game1.player.questLog)
                {
                    if (quest == null)
                        continue;

                    int daysLeft = quest.daysLeft.Value;
                    if (daysLeft > 0 && daysLeft <= 2)
                    {
                        alerts.Add(new AlertEntry
                        {
                            Label = $"{quest.GetName()} ({daysLeft} day(s) left)",
                            Category = "Quest"
                        });
                    }
                }
            }

            _snapshot.Alerts = alerts;
        }
        private string DescribeTodayWeather()
        {
            if (Game1.isLightning)
                return "Storm";
            if (Game1.isSnowing)
                return "Snow";
            if (Game1.isRaining)
                return "Rain";
            if (Game1.isDebrisWeather)
                return "Windy";
            return "Sunny";
        }
        private string DescribeWeather(int weather)
        {
            return weather switch
            {
                Game1.weather_rain => "Rain",
                Game1.weather_lightning => "Storm",
                Game1.weather_snow => "Snow",
                Game1.weather_debris => "Windy",
                Game1.weather_festival => "Festival",
                Game1.weather_sunny => "Sunny",
                _ => "Unknown"
            };
        }
        private (string Label, string Description) DescribeLuck(double luck)
        {
            string label;
            if (luck >= 0.07)
                label = "Great";
            else if (luck >= 0.02)
                label = "Good";
            else if (luck <= -0.07)
                label = "Terrible";
            else if (luck <= -0.02)
                label = "Poor";
            else
                label = "Neutral";

            string description = luck switch
            {
                >= 0.07 => "Spirits are very happy today!",
                >= 0.02 => "Spirits feel good about the day.",
                <= -0.07 => "Spirits are upset—expect setbacks.",
                <= -0.02 => "Spirits are displeased today.",
                _ => "Spirits are neutral."
            };

            return (label, description);
        }
        private void RecordPreviousDayEarnings()
        {
            if (!_isReady || Game1.player == null)
                return;

            if (_dayStartTotalMoney <= 0)
                return;

            long delta = Game1.player.totalMoneyEarned - _dayStartTotalMoney;
            var history = LoadDailyHistory();

            var yesterday = SDate.Now().AddDays(-1);
            string label = yesterday.ToLocaleString();

            history.Insert(0, new DailyFlowEntry
            {
                Label = label,
                Earnings = delta >= 0 ? (int)delta : 0,
                Expenses = delta < 0 ? (int)Math.Abs(delta) : 0
            });

            if (history.Count > DailyHistoryLength)
                history = history.Take(DailyHistoryLength).ToList();

            SaveDailyHistory(history);
        }

        private List<DailyFlowEntry> LoadDailyHistory()
        {
            var list = new List<DailyFlowEntry>();

            if (Game1.player == null)
                return list;

            if (!Game1.player.modData.TryGetValue(DailyHistoryKey, out var raw) || string.IsNullOrWhiteSpace(raw))
                return list;

            foreach (var token in raw.Split('|', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = token.Split(';');
                if (parts.Length == 3 && int.TryParse(parts[1], out var earnings) && int.TryParse(parts[2], out var expenses))
                {
                    list.Add(new DailyFlowEntry
                    {
                        Label = parts[0],
                        Earnings = earnings,
                        Expenses = expenses
                    });
                }
            }

            return list;
        }

        private void SaveDailyHistory(List<DailyFlowEntry> history)
        {
            if (Game1.player == null)
                return;

            if (history == null || history.Count == 0)
            {
                Game1.player.modData.Remove(DailyHistoryKey);
                return;
            }

            var builder = new StringBuilder();
            foreach (var entry in history)
            {
                string label = entry.Label.Replace('|', '/').Replace(';', ',');
                builder.Append(label).Append(';').Append(entry.Earnings).Append(';').Append(entry.Expenses).Append('|');
            }

            Game1.player.modData[DailyHistoryKey] = builder.ToString();
        }

        private string BuildTomorrowPlan()
        {
            var tomorrow = SDate.Now().AddDays(1);

            if (Utility.isFestivalDay(tomorrow.Day, tomorrow.Season))
                return $"Prepare for {Utility.getFestivalName(tomorrow.Day)}";

            if (Game1.weatherForTomorrow == Game1.weather_rain)
                return "Rain expected — great day for mining.";

            if (Game1.weatherForTomorrow == Game1.weather_debris)
                return "Windy tomorrow — consider beach foraging.";

            if (Game1.dailyLuck > 0.07)
                return "High luck! Visit the Skull Cavern.";

            if (Game1.dailyLuck < -0.07)
                return "Luck is low. Focus on farm maintenance.";

            return "Balance your day between fields and town.";
        }

        private List<string> GetNpcBirthdays(SDate date)
        {
            var list = new List<string>();
            foreach (NPC npc in Utility.getAllCharacters())
            {
                try
                {
                    if (npc?.isVillager() == true && npc.Birthday_Season == date.Season && npc.Birthday_Day == date.Day)
                        list.Add(npc.displayName);
                }
                catch
                {
                }
            }

            return list.Distinct().OrderBy(n => n).ToList();
        }
        private IEnumerable<(GameLocation Location, HoeDirt Dirt)> EnumerateCropTiles()
        {
            foreach (var location in Game1.locations)
            {
                if (location?.terrainFeatures != null)
                {
                    foreach (var pair in location.terrainFeatures.Pairs)
                    {
                        if (pair.Value is HoeDirt dirt && dirt.crop != null)
                            yield return (location, dirt);
                    }
                }

                if (location is BuildableGameLocation buildable)
                {
                    foreach (var building in buildable.buildings)
                    {
                        if (building?.indoors.Value is GameLocation indoors && indoors.terrainFeatures != null)
                        {
                            foreach (var pair in indoors.terrainFeatures.Pairs)
                            {
                                if (pair.Value is HoeDirt dirt && dirt.crop != null)
                                    yield return (indoors, dirt);
                            }
                        }
                    }
                }
            }
        }

        private static string GetLocationDisplayName(GameLocation location)
        {
            if (!string.IsNullOrWhiteSpace(location?.DisplayName))
                return location.DisplayName;

            return location?.Name ?? "Unknown";
        }

        private static string GetCropDisplayName(Crop crop)
        {
            if (crop == null)
                return "Crop";

            try
            {
                var item = ItemRegistry.Create(crop.indexOfHarvest.Value);
                if (item != null)
                    return item.DisplayName;
            }
            catch
            {
            }

            return "Crop";
        }

        private int GetDaysUntilHarvest(Crop crop, HoeDirt dirt)
        {
            if (crop == null)
                return -1;

            if (IsCropReadyToHarvest(dirt))
                return 0;

            if (crop.fullyGrown.Value && crop.regrowAfterHarvest.Value >= 0)
            {
                return Math.Max(0, crop.regrowAfterHarvest.Value - crop.dayOfCurrentPhase.Value);
            }

            int days = 0;
            int currentPhase = crop.currentPhase.Value;
            int dayOfPhase = crop.dayOfCurrentPhase.Value;
            var phaseDays = crop.phaseDays;
            if (phaseDays == null || currentPhase >= phaseDays.Count)
                return -1;

            days += phaseDays[currentPhase] - dayOfPhase;
            for (int i = currentPhase + 1; i < phaseDays.Count; i++)
                days += phaseDays[i];

            return Math.Max(0, days);
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

        private bool IsDirtUnwatered(HoeDirt dirt, GameLocation location)
        {
            if (dirt.crop == null)
                return false;
            if (dirt.state.Value > 0)
                return false;
            if (location.IsOutdoors && Game1.isRaining)
                return false;
            return true;
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
                    DaysUntilNextHarvest = ReadyCount > 0 ? 0 : DaysUntilNext ?? -1
                };
            }

            public HarvestForecastEntry ToForecastEntry()
            {
                return new HarvestForecastEntry
                {
                    Name = Name,
                    Quantity = GrowingCount,
                    DaysUntilReady = DaysUntilNext ?? -1,
                    Location = Location
                };
            }
        }
    }
}









