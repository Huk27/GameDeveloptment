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
        
        // PerScreen을 필드 레벨에서 초기화 (TractorMod 패턴)
        private readonly PerScreen<Dictionary<string, object>> _cachedData = new(() => new Dictionary<string, object>());
        private readonly PerScreen<DateTime> _lastCacheUpdate = new(() => DateTime.MinValue);
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromSeconds(30);
        
        // 캐시 키 상수화
        private const string FARM_DATA_KEY = "farm_data";
        
        public GameDataCollector(IMonitor monitor)
        {
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
        }
        /// <summary>
        /// 현재 농장의 모든 데이터를 수집합니다 (Phase 2.1 - 안전성 강화)
        /// </summary>
        public FarmStatisticsData CollectCurrentData()
        {
            // 게임 월드 준비 상태 확인
            if (!Context.IsWorldReady)
            {
                _monitor?.Log("게임 월드가 준비되지 않음", LogLevel.Trace);
                return new FarmStatisticsData();
            }

            // 플레이어 및 기본 게임 상태 확인
            if (Game1.player == null)
            {
                _monitor?.Log("플레이어 데이터가 없음", LogLevel.Trace);
                return new FarmStatisticsData();
            }

            try
            {
                // 캐시 유효성 확인
                if (IsCacheValid())
                {
                    _monitor?.Log("캐시된 데이터 사용", LogLevel.Trace);
                    return _cachedData.Value[FARM_DATA_KEY] as FarmStatisticsData ?? new FarmStatisticsData();
                }
                
                _monitor?.Log("새로운 데이터 수집 시작", LogLevel.Trace);
                
                // 새 데이터 수집
                var data = CollectFreshData();
                
                // 캐시 업데이트
                UpdateCache(data);
                
                return data;
            }
            catch (Exception ex)
            {
                _monitor?.Log($"데이터 수집 전체 오류: {ex.Message}", LogLevel.Error);
                _monitor?.Log($"스택 트레이스: {ex.StackTrace}", LogLevel.Debug);
                return new FarmStatisticsData();
            }
        }

        /// <summary>
        /// 캐시 유효성 확인
        /// </summary>
        private bool IsCacheValid()
        {
            return DateTime.Now - _lastCacheUpdate.Value < _cacheExpiry && 
                   _cachedData.Value.ContainsKey(FARM_DATA_KEY);
        }

        /// <summary>
        /// 새로운 데이터 수집
        /// </summary>
        private FarmStatisticsData CollectFreshData()
        {
            return new FarmStatisticsData
            {
                OverviewData = CollectOverviewData(),
                CropStatistics = CollectCropData(),
                AnimalStatistics = CollectAnimalData(),
                TimeStatistics = CollectTimeData(),
                GoalStatistics = CollectGoalData()
            };
        }

        /// <summary>
        /// 캐시 업데이트
        /// </summary>
        private void UpdateCache(FarmStatisticsData data)
        {
            _cachedData.Value[FARM_DATA_KEY] = data;
            _lastCacheUpdate.Value = DateTime.Now;
            _monitor?.Log("데이터 캐시 업데이트 완료", LogLevel.Trace);
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
        /// 개요 탭 데이터 수집 (Phase 2.1 - 안전성 강화)
        /// </summary>
        private OverviewData CollectOverviewData()
        {
            try
            {
                // 기본 안전성 체크는 상위에서 이미 완료됨
                var player = Game1.player;
                if (player == null)
                {
                    return CreateEmptyOverviewData("플레이어 데이터 없음");
                }

                var farm = Game1.getFarm();
                if (farm == null)
                {
                    return CreateEmptyOverviewData("농장 데이터 없음");
                }
                
                // 안전한 데이터 수집
                int totalEarnings = CalculateTotalEarnings(player);
                int totalCropsHarvested = GetTotalCropsHarvested(player);
                int totalAnimalProducts = GetTotalAnimalProducts(farm);
                string playTime = GetFormattedPlayTime(player);
                string seasonInfo = GetSeasonComparison();
                
                return new OverviewData
                {
                    TotalEarnings = $"{totalEarnings:N0}G",
                    CurrentMoney = $"{player.Money:N0}G",
                    TotalCropsHarvested = $"{totalCropsHarvested:N0}개",
                    TotalAnimalProducts = $"{totalAnimalProducts:N0}개",
                    TotalPlayTime = playTime,
                    SeasonComparison = seasonInfo
                };
            }
            catch (Exception ex)
            {
                _monitor?.Log($"개요 데이터 수집 오류: {ex.Message}", LogLevel.Error);
                return CreateEmptyOverviewData("데이터 수집 오류");
            }
        }

        /// <summary>
        /// 빈 개요 데이터 생성
        /// </summary>
        private OverviewData CreateEmptyOverviewData(string reason)
        {
            return new OverviewData
            {
                TotalEarnings = reason,
                CurrentMoney = reason,
                TotalCropsHarvested = reason,
                TotalAnimalProducts = reason,
                TotalPlayTime = reason,
                SeasonComparison = reason
            };
        }

        /// <summary>
        /// 안전한 총 수입 계산
        /// </summary>
        private int CalculateTotalEarnings(Farmer player)
        {
            try
            {
                // 현재 돈 + 총 획득 돈에서 현재 돈을 뺀 값 (배송 등으로 벌어들인 돈)
                return player.Money + Math.Max(0, (int)(player.totalMoneyEarned - player.Money));
            }
            catch (Exception ex)
            {
                _monitor?.Log($"총 수입 계산 오류: {ex.Message}", LogLevel.Debug);
                return player?.Money ?? 0;
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
        /// 아이템이 동물 생산물인지 확인합니다 (Phase 2.2 - 안전성 강화)
        /// </summary>
        private bool IsAnimalProduct(Object obj)
        {
            try
            {
                if (obj == null) return false;
                
                return obj.Category == Object.AnimalProductCategory ||
                       (obj.Category == Object.ArtisanGoodsCategory && IsFromAnimal(obj));
            }
            catch (Exception ex)
            {
                _monitor?.Log($"동물 생산물 확인 중 오류: {ex.Message}", LogLevel.Debug);
                return false;
            }
        }
        
        /// <summary>
        /// 아이템이 동물 기원인지 확인합니다 (Phase 2.2 - 1.6 호환성 강화)
        /// </summary>
        private bool IsFromAnimal(Object obj)
        {
            try
            {
                if (obj == null) return false;

                // Stardew Valley 1.6+ 확장된 동물 생산물 ID들
                var animalProductIds = new HashSet<int>
                {
                    // 우유 관련
                    174, 186, 184, // 우유, 대형 우유, 염소 우유
                    424, 426,      // 치즈, 염소 치즈
                    
                    // 달걀 관련  
                    176, 180, 182, 442, // 달걀, 갈색 달걀, 대형 달걀, 오리 알
                    307,               // 마요네즈
                    
                    // 기타 동물 생산물
                    440, 446, 430,     // 양모, 토끼 발, 트뤼프
                    428                // 천
                };
                
                return animalProductIds.Contains(obj.ParentSheetIndex);
            }
            catch (Exception ex)
            {
                _monitor?.Log($"동물 기원 확인 중 오류: {ex.Message}", LogLevel.Debug);
                return false;
            }
        }


        #endregion

        #region 작물 데이터 수집

        /// <summary>
        /// 현재 농장의 작물 데이터 수집 (Phase 2.1 - 안전성 강화)
        /// </summary>
        private List<CropStatistic> CollectCropData()
        {
            try
            {
                var farm = Game1.getFarm();
                if (farm?.terrainFeatures == null)
                {
                    _monitor?.Log("농장 또는 지형 특징 데이터가 없음", LogLevel.Debug);
                    return new List<CropStatistic>();
                }
                
                var cropStats = new Dictionary<string, CropStatistic>();
                
                // 농장의 작물 데이터 안전하게 수집
                CollectFarmCrops(farm, cropStats);
                
                // 인벤토리의 작물들도 안전하게 추가
                AddInventoryCrops(cropStats);
                
                return cropStats.Values.ToList();
            }
            catch (Exception ex)
            {
                _monitor?.Log($"작물 데이터 수집 오류: {ex.Message}", LogLevel.Error);
                return CreateEmptyCropList("작물 데이터 수집 오류");
            }
        }

        /// <summary>
        /// 농장의 작물들을 안전하게 수집
        /// </summary>
        private void CollectFarmCrops(Farm farm, Dictionary<string, CropStatistic> cropStats)
        {
            try
            {
                foreach (var kvp in farm.terrainFeatures.Pairs)
                {
                    if (kvp.Value is HoeDirt dirt && dirt.crop != null)
                    {
                        ProcessSingleCrop(dirt.crop, cropStats);
                    }
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"농장 작물 수집 중 오류: {ex.Message}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 단일 작물 처리
        /// </summary>
        private void ProcessSingleCrop(Crop crop, Dictionary<string, CropStatistic> cropStats)
        {
            try
            {
                if (crop?.indexOfHarvest?.Value == null) return;

                string cropId = crop.indexOfHarvest.Value;
                
                // ItemRegistry를 사용한 안전한 아이템 접근
                Item item = null;
                try
                {
                    item = ItemRegistry.Create(cropId);
                }
                catch (Exception ex)
                {
                    _monitor?.Log($"작물 아이템 생성 실패 (ID: {cropId}): {ex.Message}", LogLevel.Debug);
                    return;
                }

                if (item == null) return;

                string cropName = item.DisplayName ?? item.Name ?? "알 수 없는 작물";
                
                // 작물 통계에 추가
                if (!cropStats.ContainsKey(cropName))
                {
                    cropStats[cropName] = new CropStatistic
                    {
                        CropName = cropName,
                        Harvested = 0,
                        Revenue = 0,
                        GrowthTime = GetSafeGrowthTime(crop),
                        Quality = GetSafeCropQuality(crop),
                        Sprite = null // UI에서 처리
                    };
                }
                
                // 수확 가능한 작물인지 안전하게 확인
                if (IsCropReadyForHarvest(crop))
                {
                    cropStats[cropName].Harvested++;
                    cropStats[cropName].Revenue += item.salePrice();
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"단일 작물 처리 중 오류: {ex.Message}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 안전한 성장 시간 계산
        /// </summary>
        private float GetSafeGrowthTime(Crop crop)
        {
            try
            {
                return crop?.phaseDays?.Sum() ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 안전한 작물 품질 확인
        /// </summary>
        private string GetSafeCropQuality(Crop crop)
        {
            try
            {
                if (crop?.currentPhase?.Value == null || crop?.phaseDays == null)
                    return "알 수 없음";

                if (IsCropReadyForHarvest(crop))
                {
                    return "수확 가능";
                }
                else
                {
                    int remainingPhases = Math.Max(0, crop.phaseDays.Count - 1 - crop.currentPhase.Value);
                    return remainingPhases > 0 ? $"{remainingPhases}단계 남음" : "성장 중";
                }
            }
            catch
            {
                return "알 수 없음";
            }
        }

        /// <summary>
        /// 수확 가능 상태 안전 확인
        /// </summary>
        private bool IsCropReadyForHarvest(Crop crop)
        {
            try
            {
                return crop?.currentPhase?.Value != null && 
                       crop?.phaseDays != null && 
                       crop.currentPhase.Value >= crop.phaseDays.Count - 1;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 빈 작물 리스트 생성
        /// </summary>
        private List<CropStatistic> CreateEmptyCropList(string reason)
        {
            return new List<CropStatistic>
            {
                new CropStatistic 
                { 
                    CropName = reason, 
                    Harvested = 0, 
                    Revenue = 0, 
                    GrowthTime = 0, 
                    Quality = "오류", 
                    Sprite = null 
                }
            };
        }
        
        /// <summary>
        /// 플레이 시간을 안전하게 포맷팅합니다 (Phase 2.2 - 정확성 및 국제화 강화)
        /// </summary>
        private string GetFormattedPlayTime(Farmer player)
        {
            try
            {
                if (player?.millisecondsPlayed == null) return "0시간";
                
                long totalMilliseconds = player.millisecondsPlayed;
                if (totalMilliseconds <= 0) return "0시간";
                
                // 더 정확한 시간 계산
                var totalSeconds = totalMilliseconds / 1000.0;
                var totalMinutes = totalSeconds / 60.0;
                var totalHours = totalMinutes / 60.0;
                
                var days = (int)(totalHours / 24);
                var hours = (int)(totalHours % 24);
                var minutes = (int)(totalMinutes % 60);
                
                // 상세한 시간 표시
                if (days > 0)
                {
                    if (hours > 0)
                        return $"{days}일 {hours}시간 {minutes}분";
                    else
                        return $"{days}일 {minutes}분";
                }
                else if (hours > 0)
                {
                    return $"{hours}시간 {minutes}분";
                }
                else
                {
                    return $"{minutes}분";
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"플레이 시간 계산 오류: {ex.Message}", LogLevel.Debug);
                return "시간 계산 오류";
            }
        }
        
        /// <summary>
        /// 인벤토리의 작물들을 안전하게 통계에 추가합니다
        /// </summary>
        private void AddInventoryCrops(Dictionary<string, CropStatistic> cropStats)
        {
            try
            {
                var player = Game1.player;
                if (player?.Items == null)
                {
                    _monitor?.Log("플레이어 인벤토리가 없음", LogLevel.Debug);
                    return;
                }
                
                foreach (var item in player.Items)
                {
                    if (item != null && IsSafeCropItem(item))
                    {
                        AddInventoryItemToStats(item, cropStats);
                    }
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"인벤토리 작물 수집 중 오류: {ex.Message}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 인벤토리 아이템을 통계에 안전하게 추가
        /// </summary>
        private void AddInventoryItemToStats(Item item, Dictionary<string, CropStatistic> cropStats)
        {
            try
            {
                string cropName = item.DisplayName ?? item.Name ?? "알 수 없는 작물";
                int stack = Math.Max(1, item.Stack);
                int salePrice = Math.Max(0, item.salePrice());
                
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
                
                cropStats[cropName].Harvested += stack;
                cropStats[cropName].Revenue += salePrice * stack;
            }
            catch (Exception ex)
            {
                _monitor?.Log($"인벤토리 아이템 추가 중 오류: {ex.Message}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 아이템이 작물인지 안전하게 확인
        /// </summary>
        private bool IsSafeCropItem(Item item)
        {
            try
            {
                if (item is not StardewValley.Object obj) return false;
                
                return obj.Category == StardewValley.Object.FruitsCategory || 
                       obj.Category == StardewValley.Object.VegetableCategory;
            }
            catch
            {
                return false;
            }
        }


        #endregion

        #region 동물 데이터 수집

        /// <summary>
        /// 현재 농장의 동물 데이터 수집 (Phase 2.1 - 안전성 강화)
        /// </summary>
        private List<AnimalStatistic> CollectAnimalData()
        {
            try
            {
                var farm = Game1.getFarm();
                if (farm?.buildings == null)
                {
                    _monitor?.Log("농장 또는 건물 데이터가 없음", LogLevel.Debug);
                    return new List<AnimalStatistic>();
                }
                
                var animalStats = new Dictionary<string, AnimalStatistic>();
                
                // 농장의 동물 건물들을 안전하게 순회
                CollectFarmAnimals(farm, animalStats);
                
                // 행복도 평균 계산
                CalculateAverageHappiness(animalStats);
                
                return animalStats.Values.ToList();
            }
            catch (Exception ex)
            {
                _monitor?.Log($"동물 데이터 수집 오류: {ex.Message}", LogLevel.Error);
                return CreateEmptyAnimalList("동물 데이터 수집 오류");
            }
        }

        /// <summary>
        /// 농장의 동물들을 안전하게 수집
        /// </summary>
        private void CollectFarmAnimals(Farm farm, Dictionary<string, AnimalStatistic> animalStats)
        {
            try
            {
                foreach (Building building in farm.buildings)
                {
                    ProcessAnimalBuilding(building, animalStats);
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"농장 동물 수집 중 오류: {ex.Message}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 동물 건물 처리
        /// </summary>
        private void ProcessAnimalBuilding(Building building, Dictionary<string, AnimalStatistic> animalStats)
        {
            try
            {
                if (building?.indoors?.Value is not AnimalHouse animalHouse) return;

                // 각 동물 안전하게 처리
                ProcessAnimalsInHouse(animalHouse, animalStats);

                // 생산물 안전하게 수집
                ProcessAnimalProducts(animalHouse, animalStats);
            }
            catch (Exception ex)
            {
                _monitor?.Log($"동물 건물 처리 중 오류: {ex.Message}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 동물 집의 동물들 처리
        /// </summary>
        private void ProcessAnimalsInHouse(AnimalHouse animalHouse, Dictionary<string, AnimalStatistic> animalStats)
        {
            try
            {
                if (animalHouse?.animals == null) return;

                foreach (var kvp in animalHouse.animals.Pairs)
                {
                    ProcessSingleAnimal(kvp.Value, animalStats);
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"동물 집 처리 중 오류: {ex.Message}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 단일 동물 안전 처리
        /// </summary>
        private void ProcessSingleAnimal(FarmAnimal animal, Dictionary<string, AnimalStatistic> animalStats)
        {
            try
            {
                if (animal?.type?.Value == null) return;

                string animalType = animal.type.Value;
                string displayName = GetAnimalDisplayName(animalType);

                if (!animalStats.ContainsKey(animalType))
                {
                    animalStats[animalType] = new AnimalStatistic
                    {
                        AnimalName = displayName,
                        Products = 0,
                        Revenue = 0,
                        Happiness = 0f,
                        Sprite = null
                    };
                }

                // 행복도 안전하게 계산
                float happiness = GetSafeAnimalHappiness(animal);
                animalStats[animalType].Happiness += happiness;
            }
            catch (Exception ex)
            {
                _monitor?.Log($"단일 동물 처리 중 오류: {ex.Message}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 안전한 동물 행복도 계산
        /// </summary>
        private float GetSafeAnimalHappiness(FarmAnimal animal)
        {
            try
            {
                if (animal?.happiness?.Value == null) return 0f;
                return (float)animal.happiness.Value / 255f * 100f;
            }
            catch
            {
                return 0f;
            }
        }


        /// <summary>
        /// 동물 생산물 처리
        /// </summary>
        private void ProcessAnimalProducts(AnimalHouse animalHouse, Dictionary<string, AnimalStatistic> animalStats)
        {
            try
            {
                if (animalHouse?.objects == null) return;

                foreach (var kvp in animalHouse.objects.Pairs)
                {
                    ProcessSingleProduct(kvp.Value, animalStats);
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"동물 생산물 처리 중 오류: {ex.Message}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 단일 생산물 처리
        /// </summary>
        private void ProcessSingleProduct(StardewValley.Object product, Dictionary<string, AnimalStatistic> animalStats)
        {
            try
            {
                if (product == null || !IsSafeAnimalProduct(product)) return;

                string productType = GetSafeAnimalTypeFromProduct(product);
                if (productType != null && animalStats.ContainsKey(productType))
                {
                    int stack = Math.Max(1, product.Stack);
                    int salePrice = Math.Max(0, product.salePrice());

                    animalStats[productType].Products += stack;
                    animalStats[productType].Revenue += salePrice * stack;
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"단일 생산물 처리 중 오류: {ex.Message}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 안전한 동물 생산물 확인
        /// </summary>
        private bool IsSafeAnimalProduct(StardewValley.Object obj)
        {
            try
            {
                return obj.Category == StardewValley.Object.AnimalProductCategory ||
                       (obj.Category == StardewValley.Object.ArtisanGoodsCategory && IsFromAnimal(obj));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 안전한 동물 타입 추정
        /// </summary>
        private string GetSafeAnimalTypeFromProduct(StardewValley.Object product)
        {
            try
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
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 평균 행복도 계산
        /// </summary>
        private void CalculateAverageHappiness(Dictionary<string, AnimalStatistic> animalStats)
        {
            try
            {
                foreach (var stat in animalStats.Values)
                {
                    var animalCount = GetSafeAnimalCount(stat.AnimalName);
                    if (animalCount > 0)
                    {
                        stat.Happiness /= animalCount;
                    }
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"평균 행복도 계산 중 오류: {ex.Message}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 안전한 동물 수 계산
        /// </summary>
        private int GetSafeAnimalCount(string displayName)
        {
            try
            {
                var farm = Game1.getFarm();
                if (farm?.buildings == null) return 0;

                int count = 0;
                foreach (Building building in farm.buildings)
                {
                    if (building?.indoors?.Value is AnimalHouse animalHouse && animalHouse.animals != null)
                    {
                        foreach (var kvp in animalHouse.animals.Pairs)
                        {
                            var animal = kvp.Value;
                            if (animal?.type?.Value != null && GetAnimalDisplayName(animal.type.Value) == displayName)
                            {
                                count++;
                            }
                        }
                    }
                }
                return count;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 빈 동물 리스트 생성
        /// </summary>
        private List<AnimalStatistic> CreateEmptyAnimalList(string reason)
        {
            return new List<AnimalStatistic>
            {
                new AnimalStatistic 
                { 
                    AnimalName = reason, 
                    Products = 0, 
                    Revenue = 0, 
                    Happiness = 0f, 
                    Sprite = null 
                }
            };
        }
        
        /// <summary>
        /// 동물 타입에서 표시명을 가져옵니다 (Phase 2.2 - 안전성 및 확장성 강화)
        /// </summary>
        private string GetAnimalDisplayName(string animalType)
        {
            try
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
                    // 1.6+ 새로운 동물들
                    "Ostrich" => "타조",
                    "GoldenChicken" => "황금 닭",
                    "VoidChicken" => "보이드 닭",
                    "BlueChicken" => "파란 닭",
                    _ => animalType ?? "알 수 없는 동물"
                };
            }
            catch (Exception ex)
            {
                _monitor?.Log($"동물 표시명 변환 중 오류: {ex.Message}", LogLevel.Debug);
                return animalType ?? "알 수 없는 동물";
            }
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
        /// 시간 통계 데이터 수집 (Phase 2.2 - 안전성 및 정확성 강화)
        /// </summary>
        private List<TimeStatistic> CollectTimeData()
        {
            try
            {
                if (!Context.IsWorldReady || Game1.player == null) 
                    return CreateEmptyTimeStatistics();
                
                var timeStats = new List<TimeStatistic>();
                
                // 안전한 플레이 타임 계산
                long millisPlayed = Game1.player.millisecondsPlayed;
                if (millisPlayed <= 0)
                {
                    _monitor?.Log("플레이 시간이 0 이하입니다", LogLevel.Debug);
                    return CreateEmptyTimeStatistics();
                }

                double totalHours = Math.Max(0, millisPlayed / (1000.0 * 60.0 * 60.0));
                
                // 통계 기반 활동 시간 분석 (Stardew Valley 1.6 스탯 활용)
                var stats = Game1.player.stats;
                var farmingTime = CalculateActivityTime(totalHours, 0.4f, "농업");
                var miningTime = CalculateActivityTime(totalHours, 0.2f, "채굴");
                var fishingTime = CalculateActivityTime(totalHours, 0.15f, "낚시");
                var socialTime = CalculateActivityTime(totalHours, 0.15f, "사교");
                var otherTime = Math.Max(0, totalHours - farmingTime - miningTime - fishingTime - socialTime);
                
                if (totalHours > 0)
                {
                    timeStats.Add(new TimeStatistic
                    {
                        Activity = "농업",
                        Hours = (int)farmingTime,
                        Percentage = (float)(farmingTime / totalHours * 100.0),
                        Color = "#4AFF4A"
                    });
                    
                    timeStats.Add(new TimeStatistic
                    {
                        Activity = "채굴",
                        Hours = (int)miningTime,
                        Percentage = (float)(miningTime / totalHours * 100.0),
                        Color = "#8B4513"
                    });
                    
                    timeStats.Add(new TimeStatistic
                    {
                        Activity = "낚시",
                        Hours = (int)fishingTime,
                        Percentage = (float)(fishingTime / totalHours * 100.0),
                        Color = "#4169E1"
                    });
                    
                    timeStats.Add(new TimeStatistic
                    {
                        Activity = "사교",
                        Hours = (int)socialTime,
                        Percentage = (float)(socialTime / totalHours * 100.0),
                        Color = "#FF69B4"
                    });
                    
                    if (otherTime > 0)
                    {
                        timeStats.Add(new TimeStatistic
                        {
                            Activity = "기타",
                            Hours = (int)otherTime,
                            Percentage = (float)(otherTime / totalHours * 100.0),
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
        /// 활동 시간 계산 (Phase 2.2 - 안전한 계산)
        /// </summary>
        private double CalculateActivityTime(double totalHours, float percentage, string activityName)
        {
            try
            {
                return Math.Max(0, totalHours * percentage);
            }
            catch (Exception ex)
            {
                _monitor?.Log($"{activityName} 시간 계산 중 오류: {ex.Message}", LogLevel.Debug);
                return 0;
            }
        }

        /// <summary>
        /// 빈 시간 통계 생성 (Phase 2.2)
        /// </summary>
        private List<TimeStatistic> CreateEmptyTimeStatistics()
        {
            return new List<TimeStatistic>
            {
                new TimeStatistic 
                { 
                    Activity = "게임 시작", 
                    Hours = 0, 
                    Percentage = 100f, 
                    Color = "#4AFF4A" 
                }
            };
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
        
        // 메시지 타입 상수
        private const string MessageType = "FarmStats.DataSync";
        private const string PlayerDataType = "FarmStats.PlayerData";
        private const string HeartbeatType = "FarmStats.Heartbeat";
        private const string RequestDataType = "FarmStats.RequestData";
        
        // Phase 2.3: 네트워크 상태 관리
        private readonly Dictionary<long, DateTime> _lastHeartbeat = new();
        private readonly Dictionary<long, int> _retryCount = new();
        private readonly TimeSpan _heartbeatInterval = TimeSpan.FromSeconds(30);
        private readonly TimeSpan _heartbeatTimeout = TimeSpan.FromSeconds(90);
        private const int MaxRetryCount = 3;
        
        public MultiplayerSyncManager(IModHelper helper, IMonitor monitor, GameDataCollector dataCollector)
        {
            _helper = helper;
            _monitor = monitor;
            _dataCollector = dataCollector;
            _helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
        }
        
        /// <summary>
        /// 호스트에서 모든 플레이어에게 농장 데이터 브로드캐스트 (Phase 2.1 수정)
        /// </summary>
        public void BroadcastFarmData()
        {
            if (!Context.IsMainPlayer || !Context.IsWorldReady) return;
            
            try
            {
                var farmData = _dataCollector?.CollectCurrentData();
                if (farmData == null)
                {
                    _monitor?.Log("브로드캐스트할 농장 데이터가 없음", LogLevel.Debug);
                    return;
                }

                var sharedData = new SharedFarmData
                {
                    TotalEarnings = farmData.OverviewData?.TotalEarnings ?? "0G",
                    CropStatistics = farmData.CropStatistics ?? new List<CropStatistic>(),
                    AnimalStatistics = farmData.AnimalStatistics ?? new List<AnimalStatistic>(),
                    Timestamp = DateTime.Now
                };
                
                // 올바른 ModMessage 전송 (TractorMod 패턴)
                _helper.Multiplayer.SendMessage(
                    message: sharedData,
                    messageType: MessageType,
                    modIDs: new[] { _helper.ModRegistry.ModID }
                );
                
                _monitor?.Log("농장 데이터 브로드캐스트 완료", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                _monitor?.Log($"농장 데이터 브로드캐스트 오류: {ex.Message}", LogLevel.Error);
                _monitor?.Log($"스택 트레이스: {ex.StackTrace}", LogLevel.Debug);
            }
        }
        
        /// <summary>
        /// 플레이어 개인 데이터를 호스트에게 전송 (Phase 2.1 수정)
        /// </summary>
        public void SendPlayerData()
        {
            if (Context.IsMainPlayer || !Context.IsWorldReady) return;
            
            try
            {
                var player = Game1.player;
                if (player == null)
                {
                    _monitor?.Log("플레이어 데이터가 없어 개인 데이터 전송 불가", LogLevel.Debug);
                    return;
                }

                var farmData = _dataCollector?.CollectCurrentData();
                if (farmData == null)
                {
                    _monitor?.Log("전송할 농장 데이터가 없음", LogLevel.Debug);
                    return;
                }

                var playerData = new PlayerStatisticsData
                {
                    PlayerId = player.UniqueMultiplayerID,
                    PlayerName = player.Name ?? "Unknown Player",
                    PersonalStats = farmData.TimeStatistics ?? new List<TimeStatistic>(),
                    Timestamp = DateTime.Now
                };
                
                // 안전한 마스터 플레이어 ID 확인
                var masterPlayer = Game1.MasterPlayer;
                if (masterPlayer == null)
                {
                    _monitor?.Log("마스터 플레이어를 찾을 수 없음", LogLevel.Debug);
                    return;
                }

                _helper.Multiplayer.SendMessage(
                    message: playerData, 
                    messageType: PlayerDataType, 
                    modIDs: new[] { _helper.ModRegistry.ModID },
                    playerIDs: new[] { masterPlayer.UniqueMultiplayerID }
                );
                
                _monitor?.Log($"플레이어 데이터 전송: {playerData.PlayerName}", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                _monitor?.Log($"플레이어 데이터 전송 오류: {ex.Message}", LogLevel.Error);
                _monitor?.Log($"스택 트레이스: {ex.StackTrace}", LogLevel.Debug);
            }
        }
        
        /// <summary>
        /// 다른 플레이어로부터 메시지를 받았을 때 처리 (Phase 2.3 - 강화된 메시지 처리)
        /// </summary>
        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(e.Type))
                {
                    _monitor?.Log("빈 메시지 타입 수신", LogLevel.Debug);
                    return;
                }

                switch (e.Type)
                {
                    case MessageType when !Context.IsMainPlayer:
                        HandleSharedFarmData(e);
                        break;
                        
                    case PlayerDataType when Context.IsMainPlayer:
                        HandlePlayerData(e);
                        break;
                        
                    case HeartbeatType when Context.IsMainPlayer:
                        HandleHeartbeat(e);
                        break;
                        
                    case RequestDataType when !Context.IsMainPlayer:
                        HandleDataRequest(e);
                        break;
                        
                    default:
                        _monitor?.Log($"알 수 없는 메시지 타입: {e.Type}", LogLevel.Debug);
                        break;
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"메시지 처리 오류: {ex.Message}", LogLevel.Error);
                _monitor?.Log($"메시지 타입: {e.Type}, 발신자: {e.FromPlayerID}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 공유 농장 데이터 처리 (Phase 2.3)
        /// </summary>
        private void HandleSharedFarmData(ModMessageReceivedEventArgs e)
        {
            try
            {
                var farmData = e.ReadAs<SharedFarmData>();
                if (farmData == null)
                {
                    _monitor?.Log("공유 농장 데이터가 null입니다", LogLevel.Debug);
                    return;
                }

                _monitor?.Log($"공유 농장 데이터 수신: {farmData.Timestamp} (발신자: {e.FromPlayerID})", LogLevel.Trace);
                
                // 데이터 유효성 검증
                if (IsValidFarmData(farmData))
                {
                    // TODO: UI 업데이트 로직 추가
                    // 예: ViewModel에 데이터 전달
                    _monitor?.Log($"유효한 농장 데이터 처리 완료: 작물 {farmData.CropStatistics?.Count ?? 0}개, 동물 {farmData.AnimalStatistics?.Count ?? 0}개", LogLevel.Trace);
                }
                else
                {
                    _monitor?.Log("유효하지 않은 농장 데이터 수신", LogLevel.Debug);
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"공유 농장 데이터 처리 중 오류: {ex.Message}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 플레이어 데이터 처리 (Phase 2.3)
        /// </summary>
        private void HandlePlayerData(ModMessageReceivedEventArgs e)
        {
            try
            {
                var playerData = e.ReadAs<PlayerStatisticsData>();
                if (playerData == null)
                {
                    _monitor?.Log("플레이어 데이터가 null입니다", LogLevel.Debug);
                    return;
                }

                _monitor?.Log($"플레이어 데이터 수신: {playerData.PlayerName} (ID: {playerData.PlayerId})", LogLevel.Trace);
                
                // 데이터 유효성 검증
                if (IsValidPlayerData(playerData))
                {
                    // 플레이어 데이터 저장/병합 로직
                    // TODO: 중앙 집중식 플레이어 데이터 관리
                    _monitor?.Log($"유효한 플레이어 데이터 처리 완료: {playerData.PlayerName}", LogLevel.Trace);
                }
                else
                {
                    _monitor?.Log($"유효하지 않은 플레이어 데이터: {playerData.PlayerName}", LogLevel.Debug);
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"플레이어 데이터 처리 중 오류: {ex.Message}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 하트비트 처리 (Phase 2.3)
        /// </summary>
        private void HandleHeartbeat(ModMessageReceivedEventArgs e)
        {
            try
            {
                var heartbeat = e.ReadAs<HeartbeatData>();
                if (heartbeat == null) return;

                // 하트비트 타임스탬프 업데이트
                _lastHeartbeat[heartbeat.PlayerId] = heartbeat.Timestamp;
                
                // 재시도 카운트 리셋
                if (_retryCount.ContainsKey(heartbeat.PlayerId))
                {
                    _retryCount[heartbeat.PlayerId] = 0;
                }

                _monitor?.Log($"하트비트 수신: 플레이어 {heartbeat.PlayerId}", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                _monitor?.Log($"하트비트 처리 중 오류: {ex.Message}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 데이터 재전송 요청 처리 (Phase 2.3)
        /// </summary>
        private void HandleDataRequest(ModMessageReceivedEventArgs e)
        {
            try
            {
                var request = e.ReadAs<DataRequestData>();
                if (request == null) return;

                _monitor?.Log($"데이터 재전송 요청 수신: {request.RequestType} (요청자: {request.RequesterId})", LogLevel.Debug);

                // 요청된 데이터 타입에 따라 재전송
                switch (request.RequestType)
                {
                    case "PlayerData":
                        SendPlayerData(); // 개인 데이터 재전송
                        break;
                    default:
                        _monitor?.Log($"알 수 없는 데이터 요청 타입: {request.RequestType}", LogLevel.Debug);
                        break;
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"데이터 요청 처리 중 오류: {ex.Message}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 농장 데이터 유효성 검증 (Phase 2.3)
        /// </summary>
        private bool IsValidFarmData(SharedFarmData farmData)
        {
            try
            {
                if (farmData == null) return false;
                if (farmData.Timestamp == default) return false;
                if (string.IsNullOrEmpty(farmData.TotalEarnings)) return false;
                
                // 타임스탬프가 너무 오래된 경우 거부 (5분 이상)
                if (DateTime.Now - farmData.Timestamp > TimeSpan.FromMinutes(5))
                {
                    _monitor?.Log("농장 데이터가 너무 오래됨", LogLevel.Debug);
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 플레이어 데이터 유효성 검증 (Phase 2.3)
        /// </summary>
        private bool IsValidPlayerData(PlayerStatisticsData playerData)
        {
            try
            {
                if (playerData == null) return false;
                if (playerData.PlayerId <= 0) return false;
                if (string.IsNullOrEmpty(playerData.PlayerName)) return false;
                if (playerData.Timestamp == default) return false;

                // 타임스탬프가 너무 오래된 경우 거부
                if (DateTime.Now - playerData.Timestamp > TimeSpan.FromMinutes(5))
                {
                    _monitor?.Log($"플레이어 데이터가 너무 오래됨: {playerData.PlayerName}", LogLevel.Debug);
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 정기적으로 데이터를 동기화합니다 (Phase 2.3 - 네트워크 상태 관리 포함)
        /// </summary>
        public void PeriodicSync()
        {
            try
            {
                if (Context.IsMainPlayer)
                {
                    // 호스트: 농장 데이터 브로드캐스트 및 하트비트 체크
                    BroadcastFarmData();
                    CheckPlayerHeartbeats();
                    CleanupDisconnectedPlayers();
                }
                else
                {
                    // 클라이언트: 개인 데이터 전송 및 하트비트 전송
                    SendPlayerData();
                    SendHeartbeat();
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"정기 동기화 중 오류: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 하트비트 전송 (Phase 2.3)
        /// </summary>
        private void SendHeartbeat()
        {
            try
            {
                if (Context.IsMainPlayer) return;

                var heartbeat = new HeartbeatData
                {
                    PlayerId = Game1.player?.UniqueMultiplayerID ?? 0,
                    Timestamp = DateTime.Now
                };

                _helper.Multiplayer.SendMessage(
                    message: heartbeat,
                    messageType: HeartbeatType,
                    modIDs: new[] { _helper.ModRegistry.ModID },
                    playerIDs: new[] { Game1.MasterPlayer?.UniqueMultiplayerID ?? 0 }
                );

                _monitor?.Log("하트비트 전송", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                _monitor?.Log($"하트비트 전송 오류: {ex.Message}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 플레이어 하트비트 상태 확인 (Phase 2.3)
        /// </summary>
        private void CheckPlayerHeartbeats()
        {
            try
            {
                if (!Context.IsMainPlayer) return;

                var now = DateTime.Now;
                var timeoutPlayers = new List<long>();

                foreach (var kvp in _lastHeartbeat.ToList())
                {
                    var playerId = kvp.Key;
                    var lastBeat = kvp.Value;

                    if (now - lastBeat > _heartbeatTimeout)
                    {
                        timeoutPlayers.Add(playerId);
                        _monitor?.Log($"플레이어 {playerId} 하트비트 타임아웃", LogLevel.Debug);
                    }
                }

                // 타임아웃된 플레이어들 처리
                foreach (var playerId in timeoutPlayers)
                {
                    HandlePlayerTimeout(playerId);
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"하트비트 체크 중 오류: {ex.Message}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 연결 해제된 플레이어 정리 (Phase 2.3)
        /// </summary>
        private void CleanupDisconnectedPlayers()
        {
            try
            {
                if (!Context.IsMainPlayer) return;

                var connectedPlayerIds = Game1.getOnlineFarmers()
                    .Select(f => f.UniqueMultiplayerID)
                    .ToHashSet();

                var disconnectedPlayers = _lastHeartbeat.Keys
                    .Where(id => !connectedPlayerIds.Contains(id))
                    .ToList();

                foreach (var playerId in disconnectedPlayers)
                {
                    _lastHeartbeat.Remove(playerId);
                    _retryCount.Remove(playerId);
                    _monitor?.Log($"연결 해제된 플레이어 {playerId} 정리", LogLevel.Debug);
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"연결 해제 플레이어 정리 중 오류: {ex.Message}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 플레이어 타임아웃 처리 (Phase 2.3)
        /// </summary>
        private void HandlePlayerTimeout(long playerId)
        {
            try
            {
                if (!_retryCount.ContainsKey(playerId))
                    _retryCount[playerId] = 0;

                _retryCount[playerId]++;

                if (_retryCount[playerId] >= MaxRetryCount)
                {
                    // 최대 재시도 횟수 초과 시 플레이어 제거
                    _lastHeartbeat.Remove(playerId);
                    _retryCount.Remove(playerId);
                    _monitor?.Log($"플레이어 {playerId} 최대 재시도 초과로 제거", LogLevel.Info);
                }
                else
                {
                    // 데이터 재전송 요청
                    RequestPlayerData(playerId);
                    _monitor?.Log($"플레이어 {playerId} 데이터 재요청 (시도: {_retryCount[playerId]}/{MaxRetryCount})", LogLevel.Debug);
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"플레이어 타임아웃 처리 중 오류: {ex.Message}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 특정 플레이어에게 데이터 재전송 요청 (Phase 2.3)
        /// </summary>
        private void RequestPlayerData(long playerId)
        {
            try
            {
                var request = new DataRequestData
                {
                    RequesterId = Game1.player?.UniqueMultiplayerID ?? 0,
                    RequestType = "PlayerData",
                    Timestamp = DateTime.Now
                };

                _helper.Multiplayer.SendMessage(
                    message: request,
                    messageType: RequestDataType,
                    modIDs: new[] { _helper.ModRegistry.ModID },
                    playerIDs: new[] { playerId }
                );
            }
            catch (Exception ex)
            {
                _monitor?.Log($"데이터 재요청 전송 오류: {ex.Message}", LogLevel.Debug);
            }
        }
    }
    
    #endregion

    #region Phase 2.3 - 멀티플레이어 데이터 클래스

    /// <summary>
    /// 하트비트 데이터 (Phase 2.3)
    /// </summary>
    public class HeartbeatData
    {
        public long PlayerId { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// 데이터 재전송 요청 (Phase 2.3)
    /// </summary>
    public class DataRequestData
    {
        public long RequesterId { get; set; }
        public string RequestType { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }

    #endregion
}
