using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;
using StardewValley.TerrainFeatures;

namespace FarmStatistics
{
    /// <summary>
    /// Collects gameplay data in a defensive, ASCII-only manner so builds stay stable.
    /// </summary>
    public class GameDataCollector
    {
        private readonly IMonitor _monitor;

        public GameDataCollector(IMonitor monitor, IModHelper helper)
        {
            _monitor = monitor;
        }

        public async Task<FarmData> CollectCurrentDataAsync()
        {
            if (!Context.IsWorldReady || Game1.player == null)
                return CreateEmptyFarmData("World not ready");

            try
            {
                return await Task.Run(BuildSnapshot);
            }
            catch (Exception ex)
            {
                _monitor?.Log($"Failed to collect farm snapshot: {ex.Message}", LogLevel.Error);
                return CreateEmptyFarmData("Collection failure");
            }
        }

        private FarmData BuildSnapshot()
        {
            if (!TryGetFarm(out var farm) || farm == null)
                return CreateEmptyFarmData("Farm not loaded");

            var player = Game1.player;

            var snapshot = new FarmData
            {
                OverviewData = BuildOverviewData(player, farm),
                CropStatistics = BuildCropStatistics(farm),
                AnimalStatistics = BuildAnimalStatistics(farm),
                TimeStatistics = BuildTimeStatistics(player),
                GoalStatistics = BuildGoalStatistics(player, farm),
                Timestamp = DateTime.Now
            };

            return snapshot;
        }

        private OverviewData BuildOverviewData(Farmer player, Farm farm)
        {
            var hoursPlayed = player.millisecondsPlayed / 1000 / 3600;
            var minutesPlayed = (player.millisecondsPlayed / 1000 / 60) % 60;

            return new OverviewData
            {
                TotalEarnings = (int)player.totalMoneyEarned,
                TotalCropsHarvested = (int)player.stats.CropsShipped,
                TotalAnimalProducts = GetTotalAnimalProducts(farm),
                TotalPlayTime = $"{hoursPlayed}h {minutesPlayed}m",
                SeasonComparison = BuildSeasonLabel()
            };
        }

        private List<CropStatistic> BuildCropStatistics(Farm farm)
        {
            var result = new Dictionary<string, CropStatistic>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in farm.terrainFeatures.Pairs)
            {
                if (pair.Value is not HoeDirt dirt || dirt.crop == null)
                    continue;

                var crop = dirt.crop;
                var cropId = crop.indexOfHarvest.Value;
                var item = CreateItemSafe(cropId);
                var name = item?.DisplayName ?? cropId ?? "Crop";

                if (!result.TryGetValue(name, out var stat))
                {
                    stat = new CropStatistic
                    {
                        Name = name,
                        Season = Game1.currentSeason,
                        GrowthTime = 0,
                        Quality = crop.fullyGrown.Value ? "Ready" : "Growing"
                    };
                    result[name] = stat;
                }

                stat.Quantity += 1;
                stat.GrowthTime = Math.Max(stat.GrowthTime, GetGrowthDays(crop));
                if (item != null)
                {
                    stat.Value += item.salePrice();
                }
            }

            return result.Values
                .OrderByDescending(stat => stat.Quantity)
                .ToList();
        }

        private List<AnimalStatistic> BuildAnimalStatistics(Farm farm)
        {
            var stats = new Dictionary<string, AnimalStatistic>(StringComparer.OrdinalIgnoreCase);

            foreach (Building building in farm.buildings)
            {
                if (building?.indoors.Value is not AnimalHouse animalHouse)
                    continue;

                foreach (var entry in animalHouse.animals.Pairs)
                {
                    if (entry.Value is not FarmAnimal animal)
                        continue;

                    var name = GetAnimalDisplayName(animal);

                    if (!stats.TryGetValue(name, out var stat))
                    {
                        stat = new AnimalStatistic
                        {
                            Name = name,
                            Count = 0,
                            Value = 0,
                            Happiness = 0f
                        };
                        stats[name] = stat;
                    }

                    stat.Count += 1;
                    stat.Value += animal.sellPrice();
                    stat.Happiness += GetAnimalHappiness(animal);
                }
            }

            foreach (var stat in stats.Values)
            {
                if (stat.Count > 0)
                {
                    stat.Happiness /= stat.Count;
                }
            }

            return stats.Values
                .OrderByDescending(stat => stat.Count)
                .ToList();
        }

        private List<TimeStatistic> BuildTimeStatistics(Farmer player)
        {
            var totalHours = Math.Max(1f, player.millisecondsPlayed / (1000f * 60f * 60f));

            return new List<TimeStatistic>
            {
                CreateTimeStat("All Time", "Gameplay", totalHours, 100f, "DodgerBlue"),
                CreateTimeStat("Farming", "Crops", totalHours * 0.4f, 40f, "ForestGreen"),
                CreateTimeStat("Exploring", "Mining/Fishing", totalHours * 0.35f, 35f, "Goldenrod"),
                CreateTimeStat("Community", "Social", totalHours * 0.25f, 25f, "SlateGray")
            };
        }

        private List<GoalStatistic> BuildGoalStatistics(Farmer player, Farm farm)
        {
            var goals = new List<GoalStatistic>();

            goals.Add(CreateGoalStatistic(
                "Ship 1,000 crops",
                player.stats.CropsShipped,
                1000));

            goals.Add(CreateGoalStatistic(
                "Earn 1,000,000G",
                (int)player.totalMoneyEarned,
                1_000_000));

            goals.Add(CreateGoalStatistic(
                "Raise 20 animals",
                CountTotalAnimals(farm),
                20));

            return goals;
        }

        private bool TryGetFarm(out Farm? farm)
        {
            farm = null;

            if (!Context.IsWorldReady)
                return false;

            farm = Game1.locations?.OfType<Farm>().FirstOrDefault();
            if (farm == null)
            {
                try
                {
                    farm = Game1.getFarm();
                }
                catch (KeyNotFoundException)
                {
                    farm = null;
                }
            }

            return farm != null;
        }

        private OverviewData CreateOverviewFromReason(string reason)
        {
            return new OverviewData
            {
                TotalEarnings = 0,
                TotalCropsHarvested = 0,
                TotalAnimalProducts = 0,
                TotalPlayTime = string.Empty,
                SeasonComparison = reason
            };
        }

        private FarmData CreateEmptyFarmData(string reason)
        {
            return new FarmData
            {
                OverviewData = CreateOverviewFromReason(reason),
                CropStatistics = new List<CropStatistic>(),
                AnimalStatistics = new List<AnimalStatistic>(),
                TimeStatistics = new List<TimeStatistic>(),
                GoalStatistics = new List<GoalStatistic>(),
                Timestamp = DateTime.Now
            };
        }

        private int GetTotalAnimalProducts(Farm farm)
        {
            var total = 0;

            foreach (Building building in farm.buildings)
            {
                if (building?.indoors.Value is not AnimalHouse animalHouse)
                    continue;

                foreach (var produced in animalHouse.objects.Values)
                {
                    if (IsAnimalProduct(produced))
                    {
                        total += produced.Stack;
                    }
                }
            }

            return total;
        }

        private static bool IsAnimalProduct(SObject item)
        {
            return item != null && (item.Category == Object.Category_Egg || item.Category == Object.Category_Milk);
        }

        private static Item? CreateItemSafe(string rawId)
        {
            if (string.IsNullOrEmpty(rawId))
                return null;

            try
            {
                return ItemRegistry.Create(rawId);
            }
            catch
            {
                return null;
            }
        }

        private static int GetGrowthDays(Crop crop)
        {
            if (crop?.phaseDays == null)
                return 0;

            return crop.phaseDays.Sum();
        }

        private static float GetAnimalHappiness(FarmAnimal animal)
        {
            if (animal?.happiness?.Value == null)
                return 0f;

            return (float)Math.Round(animal.happiness.Value / 255f * 100f, 2);
        }

        private static string GetAnimalDisplayName(FarmAnimal animal)
        {
            if (!string.IsNullOrWhiteSpace(animal.displayName))
                return animal.displayName;

            if (!string.IsNullOrWhiteSpace(animal.type.Value))
                return animal.type.Value;

            return "Animal";
        }

        private static TimeStatistic CreateTimeStat(string period, string activity, float hours, float percent, string color)
        {
            return new TimeStatistic
            {
                Period = period,
                Activity = activity,
                Duration = $"{Math.Round(hours, 1)}h",
                Hours = hours,
                Percentage = percent,
                Color = color
            };
        }

        private static GoalStatistic CreateGoalStatistic(string name, int current, int target)
        {
            var goal = new GoalStatistic
            {
                Name = name,
                Target = target
            };

            goal.UpdateProgress(current, target);
            return goal;
        }

        private static int CountTotalAnimals(Farm farm)
        {
            var count = 0;

            foreach (Building building in farm.buildings)
            {
                if (building?.indoors.Value is not AnimalHouse animalHouse)
                    continue;

                count += animalHouse.animals.Count;
            }

            return count;
        }

        private static string BuildSeasonLabel()
        {
            var season = Game1.currentSeason;
            return season switch
            {
                "spring" => "Spring",
                "summer" => "Summer",
                "fall" => "Fall",
                "winter" => "Winter",
                _ => season
            };
        }
    }

    public class FarmData
    {
        public OverviewData OverviewData { get; set; } = new OverviewData();
        public List<CropStatistic> CropStatistics { get; set; } = new List<CropStatistic>();
        public List<AnimalStatistic> AnimalStatistics { get; set; } = new List<AnimalStatistic>();
        public List<TimeStatistic> TimeStatistics { get; set; } = new List<TimeStatistic>();
        public List<GoalStatistic> GoalStatistics { get; set; } = new List<GoalStatistic>();
        public DateTime Timestamp { get; set; } = DateTime.MinValue;
    }

    public class OverviewData
    {
        public int TotalEarnings { get; set; }
        public int TotalCropsHarvested { get; set; }
        public int TotalAnimalProducts { get; set; }
        public string TotalPlayTime { get; set; } = string.Empty;
        public string SeasonComparison { get; set; } = string.Empty;
    }
}