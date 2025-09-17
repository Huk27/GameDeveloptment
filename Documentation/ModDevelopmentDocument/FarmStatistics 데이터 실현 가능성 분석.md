# 🔍 FarmStatistics 데이터 실현 가능성 및 멀티플레이어 지원 분석

> **목적**: 기획한 데이터가 실제로 게임에서 수집 가능한지, 그리고 멀티플레이어 환경에서 어떻게 작동할지 분석합니다.

## 📋 목차

1. [데이터 실현 가능성 분석](#데이터-실현-가능성-분석)
2. [멀티플레이어 지원 방안](#멀티플레이어-지원-방안)
3. [제한사항 및 대안](#제한사항-및-대안)
4. [구현 우선순위 재조정](#구현-우선순위-재조정)

---

## 🎯 데이터 실현 가능성 분석

### ✅ **완전히 구현 가능한 데이터**

#### 1. **개요 탭 - 기본 통계**
```csharp
// ✅ 100% 구현 가능
public class BasicFarmStats
{
    // 게임에서 직접 제공하는 데이터
    public int CurrentMoney => Game1.player.Money;                    // 현재 소지금
    public int TotalMoneyEarned => Game1.player.totalMoneyEarned;     // 총 획득 금액
    public int ItemsShipped => Game1.player.basicShipped.Sum();       // 총 출하량
    public long PlayTime => Game1.player.millisecondsPlayed;          // 플레이 시간
    
    // 계산 가능한 데이터
    public int TodayEarnings => CalculateTodayEarnings();             // 오늘 수익 (추적 필요)
    public int CropCount => CountAllCrops();                          // 작물 수량
    public int AnimalCount => CountAllAnimals();                      // 동물 수량
}
```

#### 2. **작물 탭 - 현재 상태**
```csharp
// ✅ 95% 구현 가능
public class CropData
{
    // 직접 접근 가능한 데이터
    public Dictionary<string, int> CurrentCrops => GetCurrentCrops(); // 현재 재배 중인 작물
    public Dictionary<string, int> ReadyToHarvest => GetReadyCrops(); // 수확 가능한 작물
    public Dictionary<string, CropQuality> QualityInfo => GetCropQualities(); // 품질 정보
    
    // 게임 데이터에서 추출 가능
    public int GrowthDays => GetCropGrowthDays(cropId);               // 성장 기간
    public List<Season> GrowingSeasons => GetCropSeasons(cropId);     // 재배 계절
    public int SellPrice => GetCropPrice(cropId, quality);           // 판매 가격
}

// 실제 구현 예시
private Dictionary<string, int> GetCurrentCrops()
{
    var crops = new Dictionary<string, int>();
    var farm = Game1.getFarm();
    
    foreach (var terrainFeature in farm.terrainFeatures.Values)
    {
        if (terrainFeature is HoeDirt dirt && dirt.crop != null)
        {
            string cropName = dirt.crop.indexOfHarvest.Value;
            crops[cropName] = crops.GetValueOrDefault(cropName, 0) + 1;
        }
    }
    
    return crops;
}
```

#### 3. **동물 탭 - 현재 상태**
```csharp
// ✅ 90% 구현 가능
public class AnimalData
{
    // 직접 접근 가능한 데이터
    public Dictionary<string, int> AnimalCounts => GetAnimalCounts();     // 동물 종류별 수량
    public Dictionary<string, float> AnimalHappiness => GetHappiness();   // 행복도
    public Dictionary<string, int> AnimalAges => GetAges();               // 나이
    public int TotalProducts => CalculateTodayProducts();                 // 오늘 생산량
}

// 실제 구현 예시
private Dictionary<string, int> GetAnimalCounts()
{
    var counts = new Dictionary<string, int>();
    var farm = Game1.getFarm();
    
    foreach (Building building in farm.buildings)
    {
        if (building.indoors is AnimalHouse animalHouse)
        {
            foreach (FarmAnimal animal in animalHouse.animals.Values)
            {
                string type = animal.type.Value;
                counts[type] = counts.GetValueOrDefault(type, 0) + 1;
            }
        }
    }
    
    return counts;
}
```

### ⚠️ **부분적으로 구현 가능한 데이터**

#### 1. **시간 추적 - 활동별 시간**
```csharp
// ⚠️ 60% 구현 가능 - 실시간 추적 필요
public class TimeTrackingData
{
    // ✅ 가능: 전체 플레이 시간
    public long TotalPlayTime => Game1.player.millisecondsPlayed;
    
    // ⚠️ 제한적: 활동별 시간 (실시간 추적 필요)
    public Dictionary<ActivityType, TimeSpan> ActivityTimes { get; private set; }
    
    // ❌ 불가능: 과거 데이터 (게임에서 제공하지 않음)
    public Dictionary<DateTime, TimeSpan> HistoricalData { get; private set; }
}

// 구현 방안: 실시간 추적 시스템
public class ActivityTracker
{
    private ActivityType _currentActivity;
    private DateTime _activityStartTime;
    private readonly Dictionary<ActivityType, TimeSpan> _dailyTimes = new();
    
    public void UpdateActivity()
    {
        var newActivity = DetectCurrentActivity();
        if (newActivity != _currentActivity)
        {
            // 이전 활동 시간 기록
            if (_currentActivity != ActivityType.None)
            {
                var elapsed = DateTime.Now - _activityStartTime;
                _dailyTimes[_currentActivity] = _dailyTimes.GetValueOrDefault(_currentActivity) + elapsed;
            }
            
            // 새로운 활동 시작
            _currentActivity = newActivity;
            _activityStartTime = DateTime.Now;
        }
    }
    
    private ActivityType DetectCurrentActivity()
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
                _ => ActivityType.Other
            };
        }
        
        // 위치 기반 추론
        return location switch
        {
            Farm => ActivityType.Farming,
            MineShaft => ActivityType.Mining,
            _ => ActivityType.Other
        };
    }
}
```

#### 2. **경제 분석 - 수익/비용 추적**
```csharp
// ⚠️ 70% 구현 가능 - 일부 추적 필요
public class EconomicData
{
    // ✅ 가능: 현재 상태
    public int CurrentMoney => Game1.player.Money;
    public int TotalEarned => Game1.player.totalMoneyEarned;
    
    // ⚠️ 제한적: 세부 수익원 (추적 필요)
    public Dictionary<string, int> RevenueBySource { get; private set; }
    
    // ❌ 불가능: 지출 내역 (게임에서 추적하지 않음)
    public Dictionary<string, int> ExpensesByCategory { get; private set; }
}

// 해결 방안: 이벤트 기반 추적
public class EconomicTracker
{
    private readonly Dictionary<string, int> _dailyRevenue = new();
    private readonly Dictionary<string, int> _dailyExpenses = new();
    
    public void OnItemSold(Item item, int quantity, int price)
    {
        string category = GetItemCategory(item);
        _dailyRevenue[category] = _dailyRevenue.GetValueOrDefault(category) + (price * quantity);
    }
    
    public void OnItemPurchased(Item item, int quantity, int price)
    {
        string category = GetItemCategory(item);
        _dailyExpenses[category] = _dailyExpenses.GetValueOrDefault(category) + (price * quantity);
    }
}
```

### ❌ **구현 불가능한 데이터**

#### 1. **과거 데이터**
```csharp
// ❌ 불가능: 게임에서 과거 데이터를 저장하지 않음
public class HistoricalData
{
    public Dictionary<DateTime, int> DailyEarnings { get; set; }          // 일별 수익 기록
    public Dictionary<DateTime, int> DailyHarvests { get; set; }          // 일별 수확량
    public Dictionary<Season, SeasonStats> SeasonalComparison { get; set; } // 계절별 비교
}
```

**해결 방안**: 모드에서 직접 데이터 저장
```csharp
public class HistoricalDataManager
{
    private readonly string _saveFilePath;
    private Dictionary<string, object> _historicalData = new();
    
    public void SaveDailyData()
    {
        var todayKey = $"{Game1.year}_{Game1.currentSeason}_{Game1.dayOfMonth}";
        _historicalData[todayKey] = new
        {
            Earnings = CalculateTodayEarnings(),
            Harvests = CalculateTodayHarvests(),
            PlayTime = GetTodayPlayTime()
        };
        
        // JSON으로 저장
        File.WriteAllText(_saveFilePath, JsonConvert.SerializeObject(_historicalData));
    }
}
```

#### 2. **세밀한 효율성 분석**
```csharp
// ❌ 불가능: 너무 세밀한 추적 필요
public class DetailedEfficiency
{
    public float GoldPerMinuteByActivity { get; set; }                    // 활동별 분당 수익
    public float ExperiencePerAction { get; set; }                       // 행동별 경험치
    public Dictionary<Tool, float> ToolEfficiency { get; set; }          // 도구별 효율성
}
```

---

## 🌐 멀티플레이어 지원 방안

### **멀티플레이어 환경의 특징**

#### 1. **데이터 소유권 문제**
```csharp
// 문제: 누구의 통계를 보여줄 것인가?
public class MultiplayerDataIssues
{
    // ❌ 문제가 되는 접근 방식
    public int FarmEarnings => Game1.player.Money;  // 현재 플레이어만의 돈
    
    // ✅ 올바른 접근 방식
    public int SharedFarmEarnings => Game1.getAllFarmers().Sum(f => f.Money);  // 모든 플레이어 합계
    public Dictionary<long, int> PlayerEarnings => GetEarningsByPlayer();       // 플레이어별 수익
}
```

#### 2. **권한 및 접근성 문제**
```csharp
// Context.IsMainPlayer를 활용한 권한 관리
public class MultiplayerPermissions
{
    public bool CanAccessFarmData => Context.IsMainPlayer || HasPermission();
    public bool CanModifyGoals => Context.IsMainPlayer;
    public bool CanViewOtherPlayersStats => GetPrivacySettings().AllowStatsSharing;
}
```

### **멀티플레이어 지원 구현 방안**

#### 1. **데이터 범위 선택 시스템**
```csharp
public enum DataScope
{
    MyData,          // 내 데이터만
    SharedFarm,      // 공유 농장 데이터
    AllPlayers,      // 모든 플레이어 데이터
    Comparison       // 플레이어 간 비교
}

public class MultiplayerFarmStatistics : FarmStatisticsViewModel
{
    public DataScope CurrentScope { get; set; } = DataScope.MyData;
    public long? SelectedPlayerId { get; set; }
    
    public void SwitchDataScope(DataScope scope, long? playerId = null)
    {
        CurrentScope = scope;
        SelectedPlayerId = playerId;
        RefreshData();
    }
    
    private void RefreshData()
    {
        switch (CurrentScope)
        {
            case DataScope.MyData:
                LoadPlayerData(Game1.player);
                break;
                
            case DataScope.SharedFarm:
                LoadSharedFarmData();
                break;
                
            case DataScope.AllPlayers:
                LoadAllPlayersData();
                break;
                
            case DataScope.Comparison:
                LoadComparisonData();
                break;
        }
    }
}
```

#### 2. **데이터 동기화 시스템**
```csharp
public class MultiplayerDataSync
{
    private readonly IModHelper _helper;
    private readonly string _messageType = "FarmStats.DataUpdate";
    
    public MultiplayerDataSync(IModHelper helper)
    {
        _helper = helper;
        _helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
    }
    
    // 데이터 전송 (호스트 → 팜핸드)
    public void BroadcastFarmData(FarmStatisticsData data)
    {
        if (Context.IsMainPlayer)
        {
            _helper.Multiplayer.SendMessage(data, _messageType);
        }
    }
    
    // 데이터 수신 (팜핸드)
    private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
    {
        if (e.Type == _messageType && !Context.IsMainPlayer)
        {
            var farmData = e.ReadAs<FarmStatisticsData>();
            UpdateLocalData(farmData);
        }
    }
    
    // 개인 데이터 전송 (팜핸드 → 호스트)
    public void SendPlayerData(PlayerStatisticsData data)
    {
        if (!Context.IsMainPlayer)
        {
            _helper.Multiplayer.SendMessage(data, "FarmStats.PlayerData", new[] { Game1.MasterPlayer.UniqueMultiplayerID });
        }
    }
}
```

#### 3. **PerScreen 패턴 활용**
```csharp
// Pathoschild 패턴을 참고한 멀티플레이어 지원
public class MultiplayerFarmStatisticsManager
{
    // 플레이어별 독립적인 UI 상태
    private readonly PerScreen<FarmStatisticsViewModel> _viewModel = new();
    private readonly PerScreen<DataScope> _currentScope = new(() => DataScope.MyData);
    private readonly PerScreen<bool> _isUIOpen = new();
    
    // 공유 데이터 (호스트에서 관리)
    private SharedFarmData _sharedData;
    
    public FarmStatisticsViewModel GetViewModel()
    {
        if (_viewModel.Value == null)
        {
            _viewModel.Value = CreateViewModelForCurrentPlayer();
        }
        return _viewModel.Value;
    }
    
    private FarmStatisticsViewModel CreateViewModelForCurrentPlayer()
    {
        var viewModel = new MultiplayerFarmStatistics();
        
        // 멀티플레이어 환경에서는 추가 탭 제공
        if (Context.IsMultiplayer)
        {
            viewModel.AddMultiplayerTabs();
        }
        
        return viewModel;
    }
}
```

### **멀티플레이어 전용 기능**

#### 1. **플레이어 비교 탭**
```csharp
public class PlayerComparisonData
{
    public List<PlayerStats> PlayerStatistics { get; set; }
    public string LeaderboardCategory { get; set; } = "TotalEarnings";
    
    // 순위 시스템
    public Dictionary<string, List<PlayerRanking>> Rankings { get; set; }
    
    // 협력 통계
    public CooperationStats Cooperation { get; set; }
}

public class PlayerStats
{
    public long PlayerId { get; set; }
    public string PlayerName { get; set; }
    public int TotalEarnings { get; set; }
    public int CropsHarvested { get; set; }
    public int AnimalProducts { get; set; }
    public TimeSpan PlayTime { get; set; }
    public Dictionary<string, int> Contributions { get; set; }  // 농장 기여도
}

public class CooperationStats
{
    public int SharedProjects { get; set; }                     // 공동 프로젝트 수
    public int ResourcesShared { get; set; }                    // 공유한 자원
    public float TeamworkScore { get; set; }                    // 팀워크 점수
}
```

#### 2. **권한 및 프라이버시 설정**
```csharp
public class PrivacySettings
{
    public bool AllowStatsSharing { get; set; } = true;         // 통계 공유 허용
    public bool ShowInLeaderboard { get; set; } = true;         // 순위표 참여
    public bool AllowGoalSharing { get; set; } = false;         // 목표 공유 허용
    public List<StatCategory> HiddenCategories { get; set; }    // 숨길 통계 카테고리
}
```

---

## ⚠️ 제한사항 및 대안

### **주요 제한사항**

#### 1. **과거 데이터 부족**
- **문제**: 게임에서 과거 통계를 저장하지 않음
- **대안**: 모드에서 직접 데이터 저장 및 관리
- **구현**: JSON 파일로 일별/계절별 데이터 누적

#### 2. **세밀한 활동 추적의 한계**
- **문제**: 게임에서 활동별 시간을 추적하지 않음
- **대안**: 실시간 활동 감지 시스템 구현
- **구현**: 위치, 도구, 행동 패턴 분석

#### 3. **정확한 수익/비용 계산의 어려움**
- **문제**: 게임에서 세부 거래 내역을 저장하지 않음
- **대안**: 이벤트 기반 실시간 추적
- **구현**: 판매/구매 이벤트 후킹

#### 4. **멀티플레이어 데이터 동기화**
- **문제**: 플레이어별 데이터가 분산되어 있음
- **대안**: 호스트 중심의 데이터 수집 및 배포
- **구현**: ModMessage API 활용

### **현실적인 대안**

#### 1. **단계별 구현**
```csharp
// Phase 1: 현재 상태 데이터만 (100% 구현 가능)
public class BasicFarmStatistics
{
    public CurrentFarmStatus FarmStatus { get; set; }            // 현재 농장 상태
    public CurrentCropData CropData { get; set; }                // 현재 작물 상태
    public CurrentAnimalData AnimalData { get; set; }            // 현재 동물 상태
}

// Phase 2: 실시간 추적 데이터 추가 (80% 구현 가능)
public class TrackedFarmStatistics : BasicFarmStatistics
{
    public DailyActivityData TodayActivity { get; set; }         // 오늘 활동 데이터
    public EconomicTrackingData Economics { get; set; }          // 경제 추적 데이터
}

// Phase 3: 과거 데이터 및 분석 (60% 구현 가능)
public class ComprehensiveFarmStatistics : TrackedFarmStatistics
{
    public HistoricalData History { get; set; }                 // 과거 데이터
    public TrendAnalysis Trends { get; set; }                   // 트렌드 분석
    public PredictiveAnalysis Predictions { get; set; }         // 예측 분석
}
```

#### 2. **멀티플레이어 지원 수준**
```csharp
// Level 1: 기본 지원 (개인 데이터만)
public class BasicMultiplayerSupport
{
    public PlayerStatistics MyStats { get; set; }               // 내 통계만
    public bool IsMultiplayer => Context.IsMultiplayer;         // 멀티플레이어 감지
}

// Level 2: 공유 데이터 지원
public class SharedDataSupport : BasicMultiplayerSupport
{
    public SharedFarmStatistics FarmStats { get; set; }         // 공유 농장 통계
    public List<string> ConnectedPlayers { get; set; }          // 접속 중인 플레이어
}

// Level 3: 완전한 멀티플레이어 기능
public class FullMultiplayerSupport : SharedDataSupport
{
    public PlayerComparison Comparison { get; set; }            // 플레이어 비교
    public CooperativeGoals SharedGoals { get; set; }           // 공동 목표
    public RealTimeSync DataSync { get; set; }                  // 실시간 동기화
}
```

---

## 🚀 수정된 구현 우선순위

### **Phase 1: 핵심 기능 (2주)** 🔥
```csharp
// 100% 구현 가능한 기능만
- ✅ 현재 농장 상태 (작물, 동물, 건물)
- ✅ 기본 경제 지표 (소지금, 총 수익)
- ✅ 플레이어 기본 정보 (레벨, 플레이 시간)
- ✅ 싱글플레이어 지원
```

### **Phase 2: 추적 시스템 (3주)** ⚡
```csharp
// 실시간 추적이 필요한 기능
- 🔄 일일 활동 추적 시스템
- 🔄 경제 거래 추적
- 🔄 목표 관리 시스템
- 🔄 기본 멀티플레이어 지원 (개인 데이터)
```

### **Phase 3: 고급 기능 (4주)** 🎯
```csharp
// 복잡한 분석이 필요한 기능
- 📈 과거 데이터 저장 및 분석
- 📊 트렌드 분석 및 예측
- 🌐 완전한 멀티플레이어 지원
- 🏆 플레이어 비교 및 순위
```

---

## 📊 결론

### **실현 가능성 요약**

| 기능 카테고리 | 구현 가능성 | 비고 |
|--------------|------------|------|
| 기본 통계 | ✅ 100% | 게임 데이터 직접 접근 |
| 현재 상태 | ✅ 95% | 일부 계산 필요 |
| 실시간 추적 | ⚠️ 70% | 별도 추적 시스템 필요 |
| 과거 데이터 | ❌ 30% | 모드에서 직접 저장 필요 |
| 멀티플레이어 | ⚠️ 60% | 단계별 구현 필요 |

### **권장 접근 방식**

1. **현실적인 범위**: 100% 구현 가능한 기능부터 시작
2. **점진적 확장**: 단계별로 기능 추가
3. **사용자 피드백**: 실제 사용자의 요구사항 반영
4. **성능 우선**: 게임 성능에 영향을 주지 않는 범위 내에서 구현

**결론**: 기획한 데이터의 약 70%는 구현 가능하며, 멀티플레이어 지원은 단계별 접근이 필요합니다.

---

**업데이트 날짜**: 2024년 7월 25일  
**작성자**: jinhyy  
**상태**: 분석 완료, 현실적인 구현 계획 수립 완료
