# 📊 FarmStatistics 모드 데이터 설계 기획서

> **목적**: FarmStatistics 모드에서 표시할 데이터의 구조와 수집 방법을 체계적으로 기획하여 의미 있고 유용한 통계를 제공합니다.

## 📋 목차

1. [전체 데이터 아키텍처](#전체-데이터-아키텍처)
2. [탭별 상세 데이터 설계](#탭별-상세-데이터-설계)
3. [게임 데이터 소스 분석](#게임-데이터-소스-분석)
4. [데이터 수집 및 처리 전략](#데이터-수집-및-처리-전략)
5. [성능 최적화 방안](#성능-최적화-방안)

---

## 🏗️ 전체 데이터 아키텍처

### 📊 **데이터 분류 체계**

```
FarmStatistics
├── RealTimeData (실시간)        # 매 틱마다 업데이트
├── SessionData (세션별)         # 게임 세션 동안 누적
├── DailyData (일별)            # 하루 단위로 누적
├── SeasonalData (계절별)       # 계절 단위로 누적  
└── LifetimeData (전체)         # 게임 전체 기간 누적
```

### 🎯 **핵심 설계 원칙**

1. **실용성**: 플레이어가 실제로 활용할 수 있는 데이터
2. **정확성**: 게임 내 실제 데이터와 100% 일치
3. **성능**: 게임 성능에 영향을 주지 않는 효율적 수집
4. **확장성**: 새로운 데이터 추가가 용이한 구조
5. **시각화**: 직관적으로 이해할 수 있는 표현

---

## 📑 탭별 상세 데이터 설계

### 1. 🏡 **개요 탭 (Overview)**

#### **핵심 지표 (4개 카드)**

##### 💰 **총 수익 (Total Earnings)**
```csharp
public class EarningsData
{
    // 실시간 계산
    public int TodayEarnings { get; set; }           // 오늘 수익
    public int WeeklyEarnings { get; set; }          // 이번 주 수익
    public int SeasonalEarnings { get; set; }        // 이번 계절 수익
    public int LifetimeEarnings { get; set; }        // 전체 수익
    
    // 수익원별 분류
    public int CropEarnings { get; set; }            // 작물 판매
    public int AnimalEarnings { get; set; }          // 동물 제품
    public int MiningEarnings { get; set; }          // 채광 수익
    public int FishingEarnings { get; set; }         // 낚시 수익
    public int ArtisanEarnings { get; set; }         // 가공품 수익
    
    // 표시 형식
    public string DisplayText => $"{TodayEarnings:N0}g (오늘)";
    public string Tooltip => $"이번 계절: {SeasonalEarnings:N0}g\n전체: {LifetimeEarnings:N0}g";
}
```

**데이터 소스:**
- `Game1.player.Money` (현재 소지금)
- `Game1.player.totalMoneyEarned` (총 획득 금액)
- 판매 이벤트 추적 (`Game1.player.itemsShipped`)

##### 🌱 **작물 현황 (Crop Status)**
```csharp
public class CropStatusData
{
    // 현재 농장 상태
    public int TotalCropsPlanted { get; set; }       // 심어진 작물 수
    public int ReadyToHarvest { get; set; }          // 수확 가능한 작물
    public int GrowingCrops { get; set; }            // 성장 중인 작물
    public int WiltedCrops { get; set; }             // 시든 작물
    
    // 수확 통계
    public int TodayHarvested { get; set; }          // 오늘 수확량
    public int SeasonHarvested { get; set; }         // 이번 계절 수확량
    public int LifetimeHarvested { get; set; }       // 전체 수확량
    
    // 품질 분포
    public Dictionary<int, int> QualityDistribution { get; set; } // 품질별 수량
    
    public string DisplayText => $"{ReadyToHarvest}개 수확 대기";
    public string Tooltip => $"심어진 작물: {TotalCropsPlanted}개\n오늘 수확: {TodayHarvested}개";
}
```

**데이터 소스:**
- `Farm.terrainFeatures` (농장의 작물 데이터)
- `HoeDirt.crop` (각 타일의 작물 정보)
- `Crop.currentPhase`, `Crop.fullyGrown` (성장 단계)

##### 🐄 **동물 현황 (Animal Status)**
```csharp
public class AnimalStatusData
{
    // 동물 수량
    public Dictionary<string, int> AnimalCounts { get; set; }     // 동물 종류별 수량
    public int TotalAnimals { get; set; }                        // 총 동물 수
    public int AdultAnimals { get; set; }                        // 성체 동물
    public int BabyAnimals { get; set; }                         // 새끼 동물
    
    // 행복도 및 건강
    public float AverageHappiness { get; set; }                  // 평균 행복도
    public int SickAnimals { get; set; }                         // 아픈 동물 수
    
    // 생산량
    public int TodayProducts { get; set; }                       // 오늘 생산량
    public int WeeklyProducts { get; set; }                      // 이번 주 생산량
    public Dictionary<string, int> ProductTypes { get; set; }    // 제품 종류별 생산량
    
    public string DisplayText => $"{TotalAnimals}마리 ({AverageHappiness:F1}% 행복)";
    public string Tooltip => $"오늘 생산: {TodayProducts}개\n아픈 동물: {SickAnimals}마리";
}
```

**데이터 소스:**
- `Farm.buildings` (농장 건물)
- `Building.indoors.animals` (건물 내 동물)
- `FarmAnimal.happiness`, `FarmAnimal.health` (행복도, 건강)

##### ⏰ **시간 효율성 (Time Efficiency)**
```csharp
public class TimeEfficiencyData
{
    // 시간 분배
    public Dictionary<ActivityType, TimeSpan> ActivityTime { get; set; }
    public TimeSpan TotalPlayTime { get; set; }
    public TimeSpan TodayPlayTime { get; set; }
    
    // 효율성 지표
    public float GoldPerHour { get; set; }                       // 시간당 수익
    public float ItemsPerHour { get; set; }                      // 시간당 아이템 획득
    public float ExperiencePerHour { get; set; }                 // 시간당 경험치
    
    // 목표 달성률
    public Dictionary<string, float> GoalProgress { get; set; }   // 목표별 진행률
    
    public string DisplayText => $"{TodayPlayTime.Hours}시간 {TodayPlayTime.Minutes}분";
    public string Tooltip => $"시간당 수익: {GoldPerHour:F0}g/h\n총 플레이: {TotalPlayTime.TotalHours:F0}시간";
}

public enum ActivityType
{
    Farming, Mining, Fishing, Combat, Foraging, Social, Other
}
```

#### **계절 비교 차트**
```csharp
public class SeasonalComparisonData
{
    public Dictionary<Season, SeasonStats> SeasonData { get; set; }
    public Season CurrentSeason { get; set; }
    public float SeasonProgress { get; set; }                    // 계절 진행률 (0-100%)
    
    // 전년 대비 성장률
    public float EarningsGrowth { get; set; }                    // 수익 성장률
    public float ProductivityGrowth { get; set; }                // 생산성 성장률
    
    public string DisplayText => $"이번 계절 수익: {SeasonData[CurrentSeason].TotalEarnings:N0}g";
    public string GrowthText => $"전년 대비: {EarningsGrowth:+0.0;-0.0;+0.0}%";
}

public class SeasonStats
{
    public int TotalEarnings { get; set; }
    public int ItemsHarvested { get; set; }
    public int AnimalProducts { get; set; }
    public TimeSpan TimeSpent { get; set; }
}
```

### 2. 🌾 **작물 탭 (Crops)**

#### **작물 통계 상세 설계**

```csharp
public class DetailedCropStatistic
{
    // 기본 정보
    public string CropId { get; set; }                           // 작물 ID
    public string CropName { get; set; }                         // 작물 이름
    public string LocalizedName { get; set; }                    // 현지화된 이름
    public Texture2D Sprite { get; set; }                        // 작물 스프라이트
    public Rectangle SourceRect { get; set; }                    // 스프라이트 영역
    
    // 재배 통계
    public int TotalPlanted { get; set; }                        // 총 심은 수량
    public int TotalHarvested { get; set; }                      // 총 수확 수량
    public int CurrentlyGrowing { get; set; }                    // 현재 재배 중
    public float SuccessRate { get; set; }                       // 성공률 (수확/심기)
    
    // 경제 지표
    public int TotalRevenue { get; set; }                        // 총 수익
    public int TotalCost { get; set; }                          // 총 비용 (씨앗값)
    public int NetProfit { get; set; }                          // 순이익
    public float ProfitMargin { get; set; }                     // 이익률
    public float ProfitPerDay { get; set; }                     // 일당 수익
    
    // 품질 분석
    public Dictionary<CropQuality, int> QualityBreakdown { get; set; }
    public float AverageQuality { get; set; }                    // 평균 품질
    public int IridiumStarCount { get; set; }                    // 이리듐 스타 개수
    
    // 시간 효율성
    public int GrowthDays { get; set; }                         // 성장 기간
    public float DaysToBreakEven { get; set; }                  // 손익분기점
    public bool IsMultiHarvest { get; set; }                    // 다중 수확 가능
    public int HarvestsPerSeason { get; set; }                  // 계절당 수확 횟수
    
    // 계절 정보
    public List<Season> GrowingSeasons { get; set; }            // 재배 가능 계절
    public Season BestSeason { get; set; }                      // 최적 재배 계절
    
    // 표시 형식
    public string RevenueDisplay => $"{TotalRevenue:N0}g";
    public string ProfitDisplay => $"{NetProfit:N0}g ({ProfitMargin:+0.0;-0.0;+0.0}%)";
    public string EfficiencyDisplay => $"{ProfitPerDay:F0}g/일";
}

public enum CropQuality
{
    Normal = 0,
    Silver = 1,
    Gold = 2,
    Iridium = 4
}
```

#### **작물 분석 뷰**

1. **수익성 순위**: 일당 수익 기준 정렬
2. **효율성 분석**: 투자 대비 수익률
3. **계절별 최적 작물**: 각 계절 추천 작물
4. **품질 향상 팁**: 높은 품질 달성 방법

**데이터 소스:**
- `Game1.cropData` (작물 기본 데이터)
- `Game1.objectData` (아이템 정보)
- `Crop` 클래스 (개별 작물 상태)
- `Farm.terrainFeatures` (농장 타일 정보)

### 3. 🐄 **동물 탭 (Animals)**

#### **동물 통계 상세 설계**

```csharp
public class DetailedAnimalStatistic
{
    // 기본 정보
    public string AnimalType { get; set; }                       // 동물 종류
    public string LocalizedName { get; set; }                    // 현지화된 이름
    public Texture2D Sprite { get; set; }                        // 동물 스프라이트
    public Rectangle SourceRect { get; set; }                    // 스프라이트 영역
    
    // 개체 수 정보
    public int TotalCount { get; set; }                          // 총 개체 수
    public int AdultCount { get; set; }                          // 성체 개체 수
    public int BabyCount { get; set; }                           // 새끼 개체 수
    public int AverageAge { get; set; }                          // 평균 나이 (일)
    
    // 건강 및 행복도
    public float AverageHappiness { get; set; }                  // 평균 행복도
    public float AverageHealth { get; set; }                     // 평균 건강도
    public int SickCount { get; set; }                           // 아픈 개체 수
    public float MoodScore { get; set; }                         // 종합 컨디션 점수
    
    // 생산성 통계
    public int TodayProduction { get; set; }                     // 오늘 생산량
    public int WeeklyProduction { get; set; }                    // 주간 생산량
    public int MonthlyProduction { get; set; }                   // 월간 생산량
    public float ProductionRate { get; set; }                    // 생산률 (%)
    public Dictionary<string, int> ProductBreakdown { get; set; } // 제품별 생산량
    
    // 경제 지표
    public int TotalRevenue { get; set; }                        // 총 수익
    public int MaintenanceCost { get; set; }                     // 유지비용
    public int NetProfit { get; set; }                          // 순이익
    public float ROI { get; set; }                              // 투자 수익률
    public float RevenuePerAnimal { get; set; }                 // 개체당 수익
    
    // 케어 통계
    public int TimesFed { get; set; }                           // 먹이 준 횟수
    public int TimesPetted { get; set; }                        // 쓰다듬은 횟수
    public float CareScore { get; set; }                        // 케어 점수
    public DateTime LastInteraction { get; set; }               // 마지막 상호작용
    
    // 환경 요소
    public string HousingType { get; set; }                     // 주거 타입
    public bool HasHeater { get; set; }                         // 히터 보유
    public bool HasAutoFeeder { get; set; }                     // 자동급식기 보유
    public float HousingQuality { get; set; }                   // 주거 품질 점수
    
    // 표시 형식
    public string ProductionDisplay => $"{TodayProduction}개 (오늘)";
    public string HappinessDisplay => $"{AverageHappiness:F1}%";
    public string ProfitDisplay => $"{NetProfit:N0}g ({ROI:+0.0;-0.0;+0.0}% ROI)";
}
```

#### **동물 관리 인사이트**

1. **생산성 최적화**: 행복도와 생산량의 상관관계
2. **케어 추천**: 관리가 필요한 동물 알림
3. **투자 분석**: 새로운 동물 구매 추천
4. **계절별 관리**: 계절에 따른 특별 관리 사항

**데이터 소스:**
- `Farm.buildings` (농장 건물)
- `AnimalHouse.animals` (동물 컬렉션)
- `FarmAnimal` 클래스 (개별 동물 데이터)
- `Building` 클래스 (건물 정보)

### 4. ⏰ **시간 탭 (Time)**

#### **활동 시간 추적 설계**

```csharp
public class ActivityTimeTracker
{
    // 실시간 추적
    private Dictionary<ActivityType, DateTime> ActivityStartTimes { get; set; }
    private ActivityType CurrentActivity { get; set; }
    
    // 누적 시간 데이터
    public Dictionary<ActivityType, TimeSpan> DailyTime { get; set; }
    public Dictionary<ActivityType, TimeSpan> WeeklyTime { get; set; }
    public Dictionary<ActivityType, TimeSpan> SeasonalTime { get; set; }
    public Dictionary<ActivityType, TimeSpan> LifetimeTime { get; set; }
    
    // 효율성 지표
    public Dictionary<ActivityType, float> EfficiencyScores { get; set; }
    public Dictionary<ActivityType, int> ItemsGained { get; set; }
    public Dictionary<ActivityType, int> ExperienceGained { get; set; }
    
    // 시간 분석
    public ActivityType MostTimeSpent { get; set; }              // 가장 많은 시간 활동
    public ActivityType MostEfficient { get; set; }             // 가장 효율적인 활동
    public float ProductivityScore { get; set; }                // 전체 생산성 점수
    
    // 패턴 분석
    public Dictionary<int, ActivityType> HourlyPattern { get; set; } // 시간대별 주요 활동
    public List<string> Recommendations { get; set; }            // 시간 관리 추천사항
}

public class DetailedTimeStatistic
{
    public ActivityType Activity { get; set; }
    public string ActivityName { get; set; }
    public string LocalizedName { get; set; }
    public Color ActivityColor { get; set; }
    public string IconPath { get; set; }
    
    // 시간 데이터
    public TimeSpan TodayTime { get; set; }
    public TimeSpan AverageDaily { get; set; }
    public TimeSpan TotalTime { get; set; }
    public float DailyPercentage { get; set; }
    
    // 성과 데이터
    public int ItemsObtained { get; set; }
    public int ExperienceEarned { get; set; }
    public int GoldEarned { get; set; }
    public float EfficiencyRating { get; set; }
    
    // 비교 데이터
    public float ComparedToLastWeek { get; set; }                // 지난주 대비
    public float ComparedToAverage { get; set; }                 // 평균 대비
    
    // 표시 형식
    public string TimeDisplay => $"{TodayTime.Hours}h {TodayTime.Minutes}m";
    public string PercentageDisplay => $"{DailyPercentage:F1}%";
    public string EfficiencyDisplay => $"{EfficiencyRating:F1}/10";
}
```

#### **활동 추적 로직**

```csharp
// 활동 감지 시스템
public class ActivityDetector
{
    public ActivityType DetectCurrentActivity()
    {
        var player = Game1.player;
        var location = Game1.currentLocation;
        
        // 도구 사용 중인 경우
        if (player.UsingTool)
        {
            return player.CurrentTool switch
            {
                Hoe or WateringCan => ActivityType.Farming,
                Pickaxe => ActivityType.Mining,
                FishingRod => ActivityType.Fishing,
                MeleeWeapon => ActivityType.Combat,
                Axe => ActivityType.Foraging,
                _ => ActivityType.Other
            };
        }
        
        // 위치 기반 추론
        return location switch
        {
            Farm => ActivityType.Farming,
            MineShaft => ActivityType.Mining,
            _ when location.waterTiles != null => ActivityType.Fishing,
            _ => ActivityType.Other
        };
    }
}
```

### 5. 🎯 **목표 탭 (Goals)**

#### **목표 시스템 설계**

```csharp
public class GoalSystem
{
    public List<Goal> ActiveGoals { get; set; }
    public List<Goal> CompletedGoals { get; set; }
    public List<Goal> FailedGoals { get; set; }
    
    // 목표 카테고리
    public Dictionary<GoalCategory, List<Goal>> GoalsByCategory { get; set; }
    
    // 성취도 통계
    public float OverallProgress { get; set; }
    public int GoalsCompletedToday { get; set; }
    public int GoalsCompletedThisSeason { get; set; }
    public float CompletionRate { get; set; }
}

public class Goal
{
    // 기본 정보
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public GoalCategory Category { get; set; }
    public GoalType Type { get; set; }
    public GoalPriority Priority { get; set; }
    
    // 진행 상황
    public int CurrentValue { get; set; }
    public int TargetValue { get; set; }
    public float Progress => (float)CurrentValue / TargetValue * 100f;
    public GoalStatus Status { get; set; }
    
    // 시간 관리
    public DateTime CreatedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public TimeSpan? TimeRemaining => DueDate - DateTime.Now;
    
    // 보상 시스템
    public int ExperienceReward { get; set; }
    public int GoldReward { get; set; }
    public List<string> ItemRewards { get; set; }
    
    // 표시 형식
    public string ProgressText => $"{CurrentValue}/{TargetValue} ({Progress:F0}%)";
    public string StatusText => Status switch
    {
        GoalStatus.Active => "진행 중",
        GoalStatus.Completed => "완료",
        GoalStatus.Failed => "실패",
        GoalStatus.Paused => "일시 정지",
        _ => "알 수 없음"
    };
}

public enum GoalCategory
{
    Farming, Animals, Mining, Fishing, Combat, Social, Economic, Exploration
}

public enum GoalType
{
    Daily, Weekly, Seasonal, Lifetime, Achievement
}

public enum GoalPriority
{
    Low, Medium, High, Critical
}

public enum GoalStatus
{
    Active, Completed, Failed, Paused
}
```

#### **기본 제공 목표 예시**

```csharp
public class DefaultGoals
{
    public static List<Goal> GetDailyGoals()
    {
        return new List<Goal>
        {
            new Goal
            {
                Id = "daily_watering",
                Name = "작물 물주기",
                Description = "농장의 모든 작물에 물을 주세요",
                Category = GoalCategory.Farming,
                Type = GoalType.Daily,
                TargetValue = 1,
                Priority = GoalPriority.High
            },
            new Goal
            {
                Id = "daily_animal_care",
                Name = "동물 돌보기", 
                Description = "모든 동물을 쓰다듬고 먹이를 주세요",
                Category = GoalCategory.Animals,
                Type = GoalType.Daily,
                TargetValue = 1,
                Priority = GoalPriority.High
            },
            new Goal
            {
                Id = "daily_earnings",
                Name = "일일 수익 목표",
                Description = "오늘 10,000g 이상 벌어보세요",
                Category = GoalCategory.Economic,
                Type = GoalType.Daily,
                TargetValue = 10000,
                Priority = GoalPriority.Medium
            }
        };
    }
    
    public static List<Goal> GetSeasonalGoals()
    {
        return new List<Goal>
        {
            new Goal
            {
                Id = "seasonal_crops",
                Name = "계절 작물 마스터",
                Description = "이번 계절 모든 작물을 한 번씩 재배해보세요",
                Category = GoalCategory.Farming,
                Type = GoalType.Seasonal,
                TargetValue = GetSeasonalCropCount(),
                Priority = GoalPriority.Medium
            }
        };
    }
}
```

---

## 🗃️ 게임 데이터 소스 분석

### **핵심 게임 클래스 및 속성**

#### 1. **농장 데이터 (Farm Class)**
```csharp
// 농장 기본 정보
Farm farm = Game1.getFarm();
- farm.terrainFeatures          // 농장 타일별 특성 (작물, 나무 등)
- farm.objects                  // 농장 내 오브젝트 (기계, 상자 등)  
- farm.buildings                // 농장 건물들
- farm.resourceClumps           // 자원 덩어리 (나무, 바위 등)

// 작물 데이터 접근
foreach (var feature in farm.terrainFeatures.Values)
{
    if (feature is HoeDirt dirt && dirt.crop != null)
    {
        Crop crop = dirt.crop;
        // crop.indexOfHarvest, crop.currentPhase, crop.dayOfCurrentPhase
    }
}
```

#### 2. **플레이어 데이터 (Farmer Class)**
```csharp
Farmer player = Game1.player;
- player.Money                  // 현재 소지금
- player.totalMoneyEarned       // 총 획득 금액
- player.itemsShipped           // 출하한 아이템들
- player.stats                  // 각종 통계 데이터
- player.achievements           // 업적 데이터
- player.millisecondsPlayed     // 플레이 시간

// 스킬 정보
- player.farmingLevel           // 농업 레벨
- player.miningLevel            // 채광 레벨
- player.fishingLevel           // 낚시 레벨
- player.foragingLevel          // 채집 레벨
- player.combatLevel            // 전투 레벨
```

#### 3. **동물 데이터 (FarmAnimal Class)**
```csharp
// 건물별 동물 접근
foreach (Building building in farm.buildings)
{
    if (building.indoors is AnimalHouse animalHouse)
    {
        foreach (FarmAnimal animal in animalHouse.animals.Values)
        {
            // animal.happiness, animal.health
            // animal.age, animal.type
            // animal.currentProduce, animal.daysSinceLastLay
        }
    }
}
```

#### 4. **게임 시간 및 날짜**
```csharp
- Game1.year                    // 현재 년도
- Game1.currentSeason           // 현재 계절
- Game1.dayOfMonth              // 현재 일
- Game1.timeOfDay               // 현재 시간
- Game1.stats                   // 전역 통계
```

### **데이터 수집 최적화 전략**

#### 1. **이벤트 기반 수집**
```csharp
// 효율적인 데이터 수집을 위한 이벤트 활용
helper.Events.Player.InventoryChanged += OnInventoryChanged;
helper.Events.GameLoop.DayStarted += OnDayStarted;
helper.Events.GameLoop.TimeChanged += OnTimeChanged;
helper.Events.World.ObjectListChanged += OnObjectListChanged;
```

#### 2. **캐싱 전략**
```csharp
public class FarmDataCache
{
    private readonly Dictionary<string, (object Data, DateTime Expiry)> _cache = new();
    
    public T GetOrCompute<T>(string key, Func<T> computer, TimeSpan expiry)
    {
        if (_cache.TryGetValue(key, out var cached) && DateTime.Now < cached.Expiry)
            return (T)cached.Data;
            
        T newData = computer();
        _cache[key] = (newData, DateTime.Now + expiry);
        return newData;
    }
}
```

#### 3. **배치 처리**
```csharp
public class BatchDataProcessor
{
    private readonly Queue<DataCollectionTask> _taskQueue = new();
    
    public void ProcessBatch(int maxItems = 50)
    {
        int processed = 0;
        while (_taskQueue.Count > 0 && processed < maxItems)
        {
            var task = _taskQueue.Dequeue();
            task.Execute();
            processed++;
        }
    }
}
```

---

## 🚀 구현 우선순위

### **Phase 1: 기본 데이터 수집** (1-2주)
1. ✅ 농장 기본 정보 수집
2. ✅ 작물 현황 분석  
3. ✅ 동물 상태 추적
4. ✅ 플레이어 통계 연동

### **Phase 2: 고급 분석** (2-3주)  
1. 📊 시간 추적 시스템 구현
2. 🎯 목표 관리 시스템 구축
3. 💹 경제 분석 기능 추가
4. 📈 트렌드 분석 및 예측

### **Phase 3: 최적화 및 고도화** (1-2주)
1. ⚡ Pathoschild 패턴 적용
2. 🎨 UI/UX 개선
3. 🔧 성능 최적화
4. 🌐 다국어 지원 완성

---

**업데이트 날짜**: 2024년 7월 25일  
**작성자**: jinhyy  
**상태**: 기획 완료, 구현 준비 완료
