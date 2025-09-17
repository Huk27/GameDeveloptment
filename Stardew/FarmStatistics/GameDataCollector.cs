using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using StardewValley.Objects;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using Object = StardewValley.Object;

namespace FarmStatistics
{
    /// <summary>
    /// 실제 게임 데이터를 수집하는 클래스
    /// Phase 2: 실제 게임 데이터 수집 및 멀티플레이어 지원
    /// </summary>
    public class GameDataCollector
    {
        private readonly IMonitor _monitor;
        private readonly PerScreen<Dictionary<string, object>> _cachedData;
        private readonly PerScreen<DateTime> _lastCacheUpdate;
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromSeconds(30);
        
        public GameDataCollector(IMonitor monitor)
        {
            _monitor = monitor;
            _cachedData = new PerScreen<Dictionary<string, object>>(() => new Dictionary<string, object>());
            _lastCacheUpdate = new PerScreen<DateTime>(() => DateTime.MinValue);
        }
        /// <summary>
        /// 현재 농장의 모든 데이터를 수집합니다 (Phase 2 - 캐싱 지원)
        /// </summary>
        public FarmStatisticsData CollectCurrentData()
        {
            try
            {
                // 캐시 만료 확인
                if (DateTime.Now - _lastCacheUpdate.Value < _cacheExpiry && _cachedData.Value.Count > 0)
                {
                    _monitor.Log("캐시된 데이터 사용", LogLevel.Trace);
                    return _cachedData.Value["farm_data"] as FarmStatisticsData ?? new FarmStatisticsData();
                }
                
                _monitor.Log("새로운 데이터 수집 시작", LogLevel.Trace);
                
                var data = new FarmStatisticsData
                {
                    OverviewData = CollectOverviewData(),
                    CropStatistics = CollectCropData(),
                    AnimalStatistics = CollectAnimalData(),
                    TimeStatistics = CollectTimeData(),
                    GoalStatistics = CollectGoalData()
                };
                
                // 캐시 업데이트
                _cachedData.Value["farm_data"] = data;
                _lastCacheUpdate.Value = DateTime.Now;
                
                _monitor.Log("데이터 수집 완료", LogLevel.Trace);
                return data;
            }
            catch (Exception ex)
            {
                _monitor.Log($"데이터 수집 전체 오류: {ex.Message}", LogLevel.Error);
                return new FarmStatisticsData();
            }
        }
        
        /// <summary>
        /// 캐시를 강제로 지웁니다
        /// </summary>
        public void ClearCache()
        {
            _cachedData.Value.Clear();
            _lastCacheUpdate.Value = DateTime.MinValue;
            _monitor.Log("데이터 캐시 지움", LogLevel.Trace);
        }

        #region 개요 데이터 수집

        /// <summary>
        /// 개요 탭 데이터 수집 (Phase 2 - 실제 게임 데이터)
        /// </summary>
        private OverviewData CollectOverviewData()
        {
            try
            {
                if (!Context.IsWorldReady)
                {
                    return new OverviewData
                    {
                        TotalEarnings = "게임 로딩 중...",
                        CurrentMoney = "게임 로딩 중...",
                        TotalCropsHarvested = "게임 로딩 중...",
                        TotalAnimalProducts = "게임 로딩 중...",
                        TotalPlayTime = "게임 로딩 중...",
                        SeasonComparison = "게임 로딩 중..."
                    };
                }

                var player = Game1.player;
                var farm = Game1.getFarm();
                
                // 전체 수입 계산 (현재 돈 + 배송된 아이템 수익)
                int totalEarnings = player.Money + (int)(player.totalMoneyEarned - player.Money);
                
                // 수확된 작물 수 계산
                int totalCropsHarvested = GetTotalCropsHarvested(player);
                
                // 동물 생산물 수 계산
                int totalAnimalProducts = GetTotalAnimalProducts(farm);
                
                // 플레이 시간 계산
                var playTime = GetFormattedPlayTime();
                
                return new OverviewData
                {
                    TotalEarnings = $"{totalEarnings:N0}G",
                    CurrentMoney = $"{player.Money:N0}G",
                    TotalCropsHarvested = $"{totalCropsHarvested:N0}개",
                    TotalAnimalProducts = $"{totalAnimalProducts:N0}개",
                    TotalPlayTime = playTime,
                    SeasonComparison = GetSeasonComparison()
                };
            }
            catch (Exception ex)
            {
                _monitor.Log($"개요 데이터 수집 오류: {ex.Message}", LogLevel.Error);
                return new OverviewData
                {
                    TotalEarnings = "데이터 오류",
                    CurrentMoney = "데이터 오류",
                    TotalCropsHarvested = "데이터 오류",
                    TotalAnimalProducts = "데이터 오류",
                    TotalPlayTime = "데이터 오류",
                    SeasonComparison = "데이터 오류"
                };
            }
        }
        
        /// <summary>
        /// 수확된 작물 수를 계산합니다
        /// </summary>
        private int GetTotalCropsHarvested(Farmer player)
        {
            // 기본 수확 통계 + 인벤토리의 작물 아이템들
            int harvestedCount = 0;
            
            // 인벤토리에서 작물 아이템 수 계산
            foreach (var item in player.Items)
            {
                if (item != null && IsCropItem(item))
                {
                    harvestedCount += item.Stack;
                }
            }
            
            return harvestedCount;
        }
        
        /// <summary>
        /// 동물 생산물 수를 계산합니다
        /// </summary>
        private int GetTotalAnimalProducts(Farm farm)
        {
            int productCount = 0;
            
            // 동물 건물에서 생산물 수 계산
            foreach (Building building in farm.buildings)
            {
                if (building.indoors.Value is AnimalHouse animalHouse)
                {
                    foreach (var obj in animalHouse.objects.Values)
                    {
                        if (IsAnimalProduct(obj))
                        {
                            productCount += obj.Stack;
                        }
                    }
                }
            }
            
            return productCount;
        }
        
        /// <summary>
        /// 플레이 시간을 포맷팅합니다
        /// </summary>
        private string GetFormattedPlayTime()
        {
            var totalMilliseconds = Game1.player.millisecondsPlayed;
            var totalHours = totalMilliseconds / (1000 * 60 * 60);
            var hours = totalHours % 24;
            var days = totalHours / 24;
            
            if (days > 0)
                return $"{days}일 {hours}시간";
            else
                return $"{hours}시간";
        }
        
        /// <summary>
        /// 계절 비교 데이터를 생성합니다
        /// </summary>
        private string GetSeasonComparison()
        {
            var currentSeason = Game1.currentSeason;
            var currentYear = Game1.year;
            var dayOfMonth = Game1.dayOfMonth;
            
            return $"{currentYear}년 {GetSeasonName(currentSeason)} {dayOfMonth}일";
        }
        
        /// <summary>
        /// 계절 이름을 한국어로 변환합니다
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
        /// 아이템이 작물인지 확인합니다
        /// </summary>
        private bool IsCropItem(Item item)
        {
            if (item is not Object obj) return false;
            
            // 작물 카테고리 ID들 (Fruits, Vegetables)
            return obj.Category == Object.FruitsCategory || 
                   obj.Category == Object.VegetableCategory;
        }
        
        /// <summary>
        /// 아이템이 동물 생산물인지 확인합니다
        /// </summary>
        private bool IsAnimalProduct(Object obj)
        {
            // 동물 생산물 카테고리 (Animal Products, Artisan Goods)
            return obj.Category == Object.AnimalProductCategory ||
                   (obj.Category == Object.ArtisanGoodsCategory && IsFromAnimal(obj));
        }
        
        /// <summary>
        /// 아이템이 동물 기원인지 확인합니다
        /// </summary>
        private bool IsFromAnimal(Object obj)
        {
            // 주요 동물 기원 아이템 ID들
            var animalBasedIds = new[] { 174, 182, 184, 186, 424, 426, 428, 430 }; // 마요네즈, 치즈 등
            return animalBasedIds.Contains(obj.ParentSheetIndex);
        }


        #endregion

        #region 작물 데이터 수집

        /// <summary>
        /// 현재 농장의 작물 데이터 수집 (Phase 2 - 실제 게임 데이터)
        /// </summary>
        private List<CropStatistic> CollectCropData()
        {
            try
            {
                if (!Context.IsWorldReady) return new List<CropStatistic>();
                
                var cropStats = new Dictionary<string, CropStatistic>();
                var farm = Game1.getFarm();
                
                // 농장의 작물 데이터 수집
                foreach (var terrainFeature in farm.terrainFeatures.Values)
                {
                    if (terrainFeature is HoeDirt dirt && dirt.crop != null)
                    {
                        var crop = dirt.crop;
                        var cropData = GetCropData(crop.indexOfHarvest.Value);
                        if (cropData != null)
                        {
                            var cropName = cropData.DisplayName;
                            
                            if (!cropStats.ContainsKey(cropName))
                            {
                                cropStats[cropName] = new CropStatistic
                                {
                                    CropName = cropName,
                                    Harvested = 0,
                                    Revenue = 0,
                                    GrowthTime = crop.phaseDays.Sum(),
                                    Quality = GetCropQuality(crop),
                                    Sprite = null // UI에서 처리
                                };
                            }
                            
                            // 수확 가능한 작물 카운트
                            if (crop.currentPhase.Value >= crop.phaseDays.Count - 1)
                            {
                                cropStats[cropName].Harvested++;
                                cropStats[cropName].Revenue += cropData.Price;
                            }
                        }
                    }
                }
                
                // 인벤토리의 작물들도 추가
                AddInventoryCrops(cropStats);
                
                return cropStats.Values.ToList();
            }
            catch (Exception ex)
            {
                _monitor.Log($"작물 데이터 수집 오류: {ex.Message}", LogLevel.Error);
                return new List<CropStatistic>
                {
                    new CropStatistic 
                    { 
                        CropName = "데이터 오류", 
                        Harvested = 0, 
                        Revenue = 0, 
                        GrowthTime = 0, 
                        Quality = "오류", 
                        Sprite = null 
                    }
                };
            }
        }
        
        /// <summary>
        /// 작물 데이터를 가져옵니다
        /// </summary>
        private StardewValley.GameData.Crops.CropData GetCropData(string cropId)
        {
            try
            {
                var cropData = Game1.content.Load<Dictionary<string, StardewValley.GameData.Crops.CropData>>("Data/Crops");
                return cropData.ContainsKey(cropId) ? cropData[cropId] : null;
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// 작물 품질을 가져옵니다
        /// </summary>
        private string GetCropQuality(Crop crop)
        {
            // 기본적인 품질 평가 로직
            if (crop.currentPhase.Value >= crop.phaseDays.Count - 1)
            {
                return "수확 가능";
            }
            else
            {
                var remainingDays = crop.phaseDays.Count - 1 - crop.currentPhase.Value;
                return $"{remainingDays}일 남음";
            }
        }
        
        /// <summary>
        /// 인벤토리의 작물들을 통계에 추가합니다
        /// </summary>
        private void AddInventoryCrops(Dictionary<string, CropStatistic> cropStats)
        {
            var player = Game1.player;
            
            foreach (var item in player.Items)
            {
                if (item != null && IsCropItem(item))
                {
                    var cropName = item.DisplayName;
                    
                    if (!cropStats.ContainsKey(cropName))
                    {
                        cropStats[cropName] = new CropStatistic
                        {
                            CropName = cropName,
                            Harvested = 0,
                            Revenue = 0,
                            GrowthTime = 0,
                            Quality = "수확됨",
                            Sprite = null
                        };
                    }
                    
                    cropStats[cropName].Harvested += item.Stack;
                    cropStats[cropName].Revenue += item.salePrice() * item.Stack;
                }
            }
        }


        #endregion

        #region 동물 데이터 수집

        /// <summary>
        /// 현재 농장의 동물 데이터 수집 (Phase 2 - 실제 게임 데이터)
        /// </summary>
        private List<AnimalStatistic> CollectAnimalData()
        {
            try
            {
                if (!Context.IsWorldReady) return new List<AnimalStatistic>();
                
                var animalStats = new Dictionary<string, AnimalStatistic>();
                var farm = Game1.getFarm();
                
                // 농장의 동물 건물들을 순회
                foreach (Building building in farm.buildings)
                {
                    if (building.indoors.Value is AnimalHouse animalHouse)
                    {
                        // 각 동물 처리
                        foreach (FarmAnimal animal in animalHouse.animals.Values)
                        {
                            var animalType = animal.type.Value;
                            
                            if (!animalStats.ContainsKey(animalType))
                            {
                                animalStats[animalType] = new AnimalStatistic
                                {
                                    AnimalName = GetAnimalDisplayName(animalType),
                                    Products = 0,
                                    Revenue = 0,
                                    Happiness = 0f,
                                    Sprite = null
                                };
                            }
                            
                            // 행복도 평균 계산
                            animalStats[animalType].Happiness += (float)animal.happiness.Value / 255f * 100f;
                        }
                        
                        // 생산물 수집
                        foreach (var obj in animalHouse.objects.Values)
                        {
                            if (IsAnimalProduct(obj))
                            {
                                var productType = GetAnimalTypeFromProduct(obj);
                                if (productType != null && animalStats.ContainsKey(productType))
                                {
                                    animalStats[productType].Products += obj.Stack;
                                    animalStats[productType].Revenue += obj.salePrice() * obj.Stack;
                                }
                            }
                        }
                    }
                }
                
                // 행복도 평균 계산 완료
                foreach (var stat in animalStats.Values)
                {
                    var animalCount = GetAnimalCount(stat.AnimalName);
                    if (animalCount > 0)
                    {
                        stat.Happiness /= animalCount;
                    }
                }
                
                return animalStats.Values.ToList();
            }
            catch (Exception ex)
            {
                _monitor.Log($"동물 데이터 수집 오류: {ex.Message}", LogLevel.Error);
                return new List<AnimalStatistic>
                {
                    new AnimalStatistic 
                    { 
                        AnimalName = "데이터 오류", 
                        Products = 0, 
                        Revenue = 0, 
                        Happiness = 0f, 
                        Sprite = null 
                    }
                };
            }
        }
        
        /// <summary>
        /// 동물 타입에서 표시명을 가져옵니다
        /// </summary>
        private string GetAnimalDisplayName(string animalType)
        {
            return animalType switch
            {
                "Chicken" => "닭",
                "Cow" => "소",
                "Goat" => "염소",
                "Duck" => "오리",
                "Sheep" => "양",
                "Rabbit" => "토끼",
                "Pig" => "돼지",
                _ => animalType
            };
        }
        
        /// <summary>
        /// 생산물에서 동물 타입을 추정합니다
        /// </summary>
        private string GetAnimalTypeFromProduct(Object product)
        {
            return product.ParentSheetIndex switch
            {
                176 => "Chicken", // 달걀
                180 => "Chicken", // 갈색 달걀
                182 => "Chicken", // 대형 달걀
                174 => "Cow",     // 우유
                186 => "Cow",     // 대형 우유
                184 => "Goat",    // 염소 우유
                442 => "Duck",    // 오리 알
                440 => "Sheep",   // 양모
                446 => "Rabbit",  // 토끼 발
                430 => "Pig",     // 트리프
                _ => null
            };
        }
        
        /// <summary>
        /// 특정 동물 타입의 수를 세어줍니다
        /// </summary>
        private int GetAnimalCount(string displayName)
        {
            var farm = Game1.getFarm();
            int count = 0;
            
            foreach (Building building in farm.buildings)
            {
                if (building.indoors.Value is AnimalHouse animalHouse)
                {
                    foreach (FarmAnimal animal in animalHouse.animals.Values)
                    {
                        if (GetAnimalDisplayName(animal.type.Value) == displayName)
                        {
                            count++;
                        }
                    }
                }
            }
            
            return count;
        }


        #endregion

        #region 시간 & 목표 데이터 (임시 구현)

        /// <summary>
        /// 시간 통계 데이터 수집 (Phase 2 - 실제 게임 데이터)
        /// </summary>
        private List<TimeStatistic> CollectTimeData()
        {
            try
            {
                if (!Context.IsWorldReady) return new List<TimeStatistic>();
                
                var timeStats = new List<TimeStatistic>();
                var totalPlayTime = Game1.player.millisecondsPlayed / (1000 * 60 * 60); // 시간 단위
                
                // 간단한 활동 시간 분석 (추정치)
                var farmingTime = (int)(totalPlayTime * 0.4f); // 40% 농업
                var miningTime = (int)(totalPlayTime * 0.2f);  // 20% 채굴
                var fishingTime = (int)(totalPlayTime * 0.15f); // 15% 낚시
                var socialTime = (int)(totalPlayTime * 0.15f);  // 15% 사교
                var otherTime = totalPlayTime - farmingTime - miningTime - fishingTime - socialTime;
                
                if (totalPlayTime > 0)
                {
                    timeStats.Add(new TimeStatistic
                    {
                        Activity = "농업",
                        Hours = farmingTime,
                        Percentage = (float)farmingTime / totalPlayTime * 100f,
                        Color = "#4AFF4A"
                    });
                    
                    timeStats.Add(new TimeStatistic
                    {
                        Activity = "채굴",
                        Hours = miningTime,
                        Percentage = (float)miningTime / totalPlayTime * 100f,
                        Color = "#8B4513"
                    });
                    
                    timeStats.Add(new TimeStatistic
                    {
                        Activity = "낚시",
                        Hours = fishingTime,
                        Percentage = (float)fishingTime / totalPlayTime * 100f,
                        Color = "#4169E1"
                    });
                    
                    timeStats.Add(new TimeStatistic
                    {
                        Activity = "사교",
                        Hours = socialTime,
                        Percentage = (float)socialTime / totalPlayTime * 100f,
                        Color = "#FF69B4"
                    });
                    
                    if (otherTime > 0)
                    {
                        timeStats.Add(new TimeStatistic
                        {
                            Activity = "기타",
                            Hours = otherTime,
                            Percentage = (float)otherTime / totalPlayTime * 100f,
                            Color = "#CCCCCC"
                        });
                    }
                }
                else
                {
                    timeStats.Add(new TimeStatistic
                    {
                        Activity = "게임 시작",
                        Hours = 0,
                        Percentage = 100f,
                        Color = "#4AFF4A"
                    });
                }
                
                return timeStats;
            }
            catch (Exception ex)
            {
                _monitor.Log($"시간 데이터 수집 오류: {ex.Message}", LogLevel.Error);
                return new List<TimeStatistic>
                {
                    new TimeStatistic 
                    { 
                        Activity = "데이터 오류", 
                        Hours = 0, 
                        Percentage = 100f, 
                        Color = "#FF0000" 
                    }
                };
            }
        }

        /// <summary>
        /// 목표 통계 데이터 수집 (Phase 2 - 실제 게임 데이터)
        /// </summary>
        private List<GoalStatistic> CollectGoalData()
        {
            try
            {
                if (!Context.IsWorldReady) return new List<GoalStatistic>();
                
                var goals = new List<GoalStatistic>();
                var player = Game1.player;
                
                // 돈 목표
                var moneyGoal = new GoalStatistic
                {
                    GoalName = "돈 모으기",
                    Current = player.Money,
                    Target = 1000000 // 100만 G 목표
                };
                moneyGoal.UpdateProgress();
                goals.Add(moneyGoal);
                
                // 농업 레벨 목표
                var farmingLevel = player.FarmingLevel;
                var farmingGoal = new GoalStatistic
                {
                    GoalName = "농업 마스터",
                    Current = farmingLevel,
                    Target = 10
                };
                farmingGoal.UpdateProgress();
                goals.Add(farmingGoal);
                
                // 채굴 레벨 목표
                var miningLevel = player.MiningLevel;
                var miningGoal = new GoalStatistic
                {
                    GoalName = "채굴 마스터",
                    Current = miningLevel,
                    Target = 10
                };
                miningGoal.UpdateProgress();
                goals.Add(miningGoal);
                
                // 낚시 레벨 목표
                var fishingLevel = player.FishingLevel;
                var fishingGoal = new GoalStatistic
                {
                    GoalName = "낚시 마스터",
                    Current = fishingLevel,
                    Target = 10
                };
                fishingGoal.UpdateProgress();
                goals.Add(fishingGoal);
                
                // 전투 레벨 목표
                var combatLevel = player.CombatLevel;
                var combatGoal = new GoalStatistic
                {
                    GoalName = "전투 마스터",
                    Current = combatLevel,
                    Target = 10
                };
                combatGoal.UpdateProgress();
                goals.Add(combatGoal);
                
                // 수확 목표 (예시)
                var totalCropsHarvested = GetTotalCropsHarvested(player);
                var harvestGoal = new GoalStatistic
                {
                    GoalName = "작물 수확",
                    Current = totalCropsHarvested,
                    Target = 1000 // 1000개 목표
                };
                harvestGoal.UpdateProgress();
                goals.Add(harvestGoal);
                
                return goals;
            }
            catch (Exception ex)
            {
                _monitor.Log($"목표 데이터 수집 오류: {ex.Message}", LogLevel.Error);
                return new List<GoalStatistic>
                {
                    new GoalStatistic
                    {
                        GoalName = "데이터 오류",
                        Current = 0,
                        Target = 1
                    }
                };
            }
        }

        #endregion
    }

    #region 데이터 구조체들

    /// <summary>
    /// 전체 농장 통계 데이터
    /// </summary>
    public class FarmStatisticsData
    {
        public OverviewData OverviewData { get; set; } = new();
        public List<CropStatistic> CropStatistics { get; set; } = new();
        public List<AnimalStatistic> AnimalStatistics { get; set; } = new();
        public List<TimeStatistic> TimeStatistics { get; set; } = new();
        public List<GoalStatistic> GoalStatistics { get; set; } = new();
    }

    /// <summary>
    /// 개요 탭 데이터
    /// </summary>
    public class OverviewData
    {
        public string TotalEarnings { get; set; } = "";
        public string CurrentMoney { get; set; } = "";
        public string TotalCropsHarvested { get; set; } = "";
        public string TotalAnimalProducts { get; set; } = "";
        public string TotalPlayTime { get; set; } = "";
        public string SeasonComparison { get; set; } = "";
    }
    
    /// <summary>
    /// 멀티플레이어 공유 농장 데이터
    /// </summary>
    public class SharedFarmData
    {
        public string TotalEarnings { get; set; } = "";
        public List<CropStatistic> CropStatistics { get; set; } = new();
        public List<AnimalStatistic> AnimalStatistics { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }
    
    /// <summary>
    /// 멀티플레이어 개별 플레이어 데이터
    /// </summary>
    public class PlayerStatisticsData
    {
        public long PlayerId { get; set; }
        public string PlayerName { get; set; } = "";
        public List<TimeStatistic> PersonalStats { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }


    #endregion
    
    #region 멀티플레이어 지원 클래스
    
    /// <summary>
    /// 멀티플레이어 데이터 동기화 관리자
    /// </summary>
    public class MultiplayerSyncManager
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly GameDataCollector _dataCollector;
        private const string MessageType = "FarmStats.DataSync";
        private const string PlayerDataType = "FarmStats.PlayerData";
        
        public MultiplayerSyncManager(IModHelper helper, IMonitor monitor, GameDataCollector dataCollector)
        {
            _helper = helper;
            _monitor = monitor;
            _dataCollector = dataCollector;
            _helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
        }
        
        /// <summary>
        /// 호스트에서 모든 플레이어에게 농장 데이터 브로드캐스트
        /// </summary>
        public void BroadcastFarmData()
        {
            if (!Context.IsMainPlayer) return;
            
            try
            {
                var farmData = _dataCollector.CollectCurrentData();
                var sharedData = new SharedFarmData
                {
                    TotalEarnings = farmData.OverviewData.TotalEarnings,
                    CropStatistics = farmData.CropStatistics,
                    AnimalStatistics = farmData.AnimalStatistics,
                    Timestamp = DateTime.Now
                };
                
                _helper.Multiplayer.SendMessage(sharedData, MessageType);
                _monitor.Log("농장 데이터 브로드캐스트 완료", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                _monitor.Log($"농장 데이터 브로드캐스트 오류: {ex.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// 플레이어 개인 데이터를 호스트에게 전송
        /// </summary>
        public void SendPlayerData()
        {
            if (Context.IsMainPlayer) return;
            
            try
            {
                var farmData = _dataCollector.CollectCurrentData();
                var playerData = new PlayerStatisticsData
                {
                    PlayerId = Game1.player.UniqueMultiplayerID,
                    PlayerName = Game1.player.Name,
                    PersonalStats = farmData.TimeStatistics,
                    Timestamp = DateTime.Now
                };
                
                _helper.Multiplayer.SendMessage(
                    playerData, 
                    PlayerDataType, 
                    new[] { Game1.MasterPlayer.UniqueMultiplayerID }
                );
                _monitor.Log($"플레이어 데이터 전송: {playerData.PlayerName}", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                _monitor.Log($"플레이어 데이터 전송 오류: {ex.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// 다른 플레이어로부터 메시지를 받았을 때 처리
        /// </summary>
        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            try
            {
                if (e.Type == MessageType && !Context.IsMainPlayer)
                {
                    var farmData = e.ReadAs<SharedFarmData>();
                    _monitor.Log($"공유 농장 데이터 수신: {farmData.Timestamp}", LogLevel.Trace);
                    // TODO: UI 업데이트 로직 추가
                }
                else if (e.Type == PlayerDataType && Context.IsMainPlayer)
                {
                    var playerData = e.ReadAs<PlayerStatisticsData>();
                    _monitor.Log($"플레이어 데이터 수신: {playerData.PlayerName}", LogLevel.Trace);
                    // TODO: 플레이어 데이터 저장 로직 추가
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"메시지 처리 오류: {ex.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// 정기적으로 데이터를 동기화합니다
        /// </summary>
        public void PeriodicSync()
        {
            if (Context.IsMainPlayer)
            {
                BroadcastFarmData();
            }
            else
            {
                SendPlayerData();
            }
        }
    }
    
    #endregion
}
