# 🚀 FarmStatistics 모드 개발 계획서

> **목적**: 다운받은 프레임워크와 예제 모드들을 참고하여 FarmStatistics 모드의 체계적이고 현실적인 개발 계획을 수립합니다.

## 📋 목차

1. [현재 상황 분석](#현재-상황-분석)
2. [참고할 수 있는 패턴들](#참고할-수-있는-패턴들)
3. [단계별 개발 계획](#단계별-개발-계획)
4. [기술적 구현 전략](#기술적-구현-전략)
5. [멀티플레이어 지원 전략](#멀티플레이어-지원-전략)
6. [성능 최적화 방안](#성능-최적화-방안)

---

## 📊 현재 상황 분석

### ✅ **이미 완료된 것들**

#### 1. **기본 UI 프레임워크**
- ✅ **StardewUI 기반 UI**: `.sml` 파일로 선언적 UI 구성
- ✅ **ViewModel 패턴**: `FarmStatisticsViewModel.cs`로 데이터 바인딩
- ✅ **탭 시스템**: 5개 탭 (개요, 작물, 동물, 시간, 목표) 구조 완성
- ✅ **데모 데이터**: 모든 탭의 샘플 데이터 구현

#### 2. **데이터 모델**
- ✅ **CropStatistic**: 작물 통계 데이터 구조
- ✅ **AnimalStatistic**: 동물 통계 데이터 구조  
- ✅ **TimeStatistic**: 시간 추적 데이터 구조
- ✅ **GoalStatistic**: 목표 관리 데이터 구조
- ✅ **INotifyPropertyChanged**: 데이터 바인딩 지원

#### 3. **문서화**
- ✅ **데이터 설계 기획서**: 상세한 데이터 구조 설계 완료
- ✅ **실현 가능성 분석**: 구현 가능성 70% 분석 완료
- ✅ **멀티플레이어 분석**: PerScreen 패턴 활용 방안 수립

### 🔄 **현재 부족한 것들**

#### 1. **실제 게임 데이터 연동**
- ❌ **데모 데이터만 존재**: `LoadDemoData()` 메서드만 구현됨
- ❌ **실시간 데이터 수집**: `UpdateData()` 메서드가 비어있음
- ❌ **게임 이벤트 연동**: SMAPI 이벤트 후킹 미구현

#### 2. **멀티플레이어 지원**
- ❌ **PerScreen 패턴**: 플레이어별 독립적 데이터 관리 미구현
- ❌ **데이터 동기화**: ModMessage API 활용한 데이터 공유 미구현
- ❌ **권한 관리**: 호스트/팜핸드별 권한 분리 미구현

#### 3. **성능 최적화**
- ❌ **캐싱 시스템**: 데이터 캐싱 및 배치 처리 미구현
- ❌ **이벤트 최적화**: 불필요한 업데이트 방지 미구현

---

## 🎯 참고할 수 있는 패턴들

### 📚 **활용 가능한 참고 자료**

#### 1. **Pathoschild 모드들** (멀티플레이어 & 성능)
```csharp
// PerScreen 패턴 (LookupAnything/ModEntry.cs:61-64)
private readonly PerScreen<Stack<IClickableMenu>> PreviousMenus = new(() => new());
private PerScreen<DebugInterface>? DebugInterface;

// 적용 방안
private readonly PerScreen<FarmStatisticsViewModel> ViewModel = new();
private readonly PerScreen<DataCollectionManager> DataManager = new();
```

#### 2. **SpaceCore 프레임워크** (데이터 수집 & 이벤트)
```csharp
// 게임 데이터 접근 패턴
public static class FarmDataHelper
{
    public static Dictionary<string, int> GetCurrentCrops()
    {
        var farm = Game1.getFarm();
        var crops = new Dictionary<string, int>();
        
        foreach (var feature in farm.terrainFeatures.Values)
        {
            if (feature is HoeDirt dirt && dirt.crop != null)
            {
                string cropId = dirt.crop.indexOfHarvest.Value;
                crops[cropId] = crops.GetValueOrDefault(cropId, 0) + 1;
            }
        }
        return crops;
    }
}
```

#### 3. **GenericModConfigMenu** (UI 관리)
```csharp
// 동적 UI 생성 패턴
internal readonly ModConfigManager ConfigManager = new();

// 적용 방안: 설정 UI 통합
public void RegisterWithGMCM()
{
    var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
    if (gmcm != null)
    {
        gmcm.Register(ModManifest, ResetConfig, SaveConfig);
        gmcm.AddBoolOption(ModManifest, "ShowRealTimeData", "실시간 데이터 표시");
    }
}
```

#### 4. **StardewUI 패턴** (현재 사용 중)
```csharp
// 현재 구현된 패턴 활용
viewEngine.RegisterViewsFromDirectory(Path.Combine(Helper.DirectoryPath, "assets", "views"));
var view = viewEngine.CreateMenuFromAsset("FarmStatistics.sml");
```

---

## 🚀 단계별 개발 계획

### **Phase 1: 실제 데이터 연동** (2주) 🔥

#### **목표**: 데모 데이터를 실제 게임 데이터로 교체

#### **1.1 기본 데이터 수집 시스템**
```csharp
// 새로 구현할 클래스
public class GameDataCollector
{
    private readonly IModHelper _helper;
    
    public FarmStatisticsData CollectCurrentData()
    {
        return new FarmStatisticsData
        {
            OverviewData = CollectOverviewData(),
            CropData = CollectCropData(), 
            AnimalData = CollectAnimalData(),
            TimeData = CollectTimeData(),
            GoalData = CollectGoalData()
        };
    }
    
    private OverviewData CollectOverviewData()
    {
        var player = Game1.player;
        return new OverviewData
        {
            TotalEarnings = player.totalMoneyEarned.ToString("N0") + "g",
            CurrentMoney = player.Money.ToString("N0") + "g",
            TotalPlayTime = FormatPlayTime(player.millisecondsPlayed),
            TotalCropsHarvested = GetTotalHarvested().ToString("N0") + "개",
            TotalAnimalProducts = GetTotalAnimalProducts().ToString("N0") + "개"
        };
    }
}
```

#### **1.2 이벤트 기반 업데이트**
```csharp
// ModEntry.cs에 추가할 이벤트 핸들러
public override void Entry(IModHelper helper)
{
    // 기존 코드...
    
    // 데이터 업데이트 이벤트
    helper.Events.GameLoop.TimeChanged += OnTimeChanged;
    helper.Events.GameLoop.DayStarted += OnDayStarted;
    helper.Events.Player.InventoryChanged += OnInventoryChanged;
    helper.Events.World.ObjectListChanged += OnObjectListChanged;
}

private void OnTimeChanged(object sender, TimeChangedEventArgs e)
{
    // 시간별 데이터 업데이트 (10분마다)
    if (e.NewTime % 100 == 0) // 매시 정각
    {
        _dataCollector.UpdateTimeData();
        _viewModel.Value?.UpdateData();
    }
}
```

#### **1.3 실시간 작물/동물 데이터**
```csharp
public class CropDataCollector
{
    public List<CropStatistic> GetCurrentCrops()
    {
        var farm = Game1.getFarm();
        var cropCounts = new Dictionary<string, CropInfo>();
        
        foreach (var feature in farm.terrainFeatures.Values)
        {
            if (feature is HoeDirt dirt && dirt.crop != null)
            {
                var crop = dirt.crop;
                string cropId = crop.indexOfHarvest.Value;
                
                if (!cropCounts.ContainsKey(cropId))
                    cropCounts[cropId] = new CropInfo();
                    
                cropCounts[cropId].TotalCount++;
                
                if (crop.fullyGrown.Value)
                    cropCounts[cropId].ReadyToHarvest++;
                else
                    cropCounts[cropId].Growing++;
            }
        }
        
        return cropCounts.Select(kvp => CreateCropStatistic(kvp.Key, kvp.Value)).ToList();
    }
}
```

### **Phase 2: 고급 기능 구현** (3주) ⚡

#### **목표**: 시간 추적, 목표 관리, 기본 멀티플레이어 지원

#### **2.1 활동 시간 추적 시스템**
```csharp
public class ActivityTracker
{
    private readonly PerScreen<ActivityState> _currentState = new(() => new());
    private readonly Dictionary<ActivityType, TimeSpan> _dailyTimes = new();
    
    public void UpdateActivity()
    {
        var newActivity = DetectCurrentActivity();
        var state = _currentState.Value;
        
        if (newActivity != state.CurrentActivity)
        {
            // 이전 활동 시간 기록
            if (state.CurrentActivity != ActivityType.None)
            {
                var elapsed = DateTime.Now - state.StartTime;
                _dailyTimes[state.CurrentActivity] = 
                    _dailyTimes.GetValueOrDefault(state.CurrentActivity) + elapsed;
            }
            
            // 새 활동 시작
            state.CurrentActivity = newActivity;
            state.StartTime = DateTime.Now;
        }
    }
    
    private ActivityType DetectCurrentActivity()
    {
        var player = Game1.player;
        var location = Game1.currentLocation;
        
        // 도구 기반 감지
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
        
        // 위치 기반 감지
        return location switch
        {
            Farm => ActivityType.Farming,
            MineShaft => ActivityType.Mining,
            Beach when location.waterTiles?.Count > 0 => ActivityType.Fishing,
            _ => ActivityType.Other
        };
    }
}
```

#### **2.2 목표 관리 시스템**
```csharp
public class GoalManager
{
    private readonly List<Goal> _activeGoals = new();
    private readonly IModHelper _helper;
    
    public void InitializeDefaultGoals()
    {
        _activeGoals.AddRange(new[]
        {
            new Goal
            {
                Id = "daily_watering",
                Name = "작물 물주기",
                Type = GoalType.Daily,
                TargetValue = GetTotalCropsNeedingWater(),
                CurrentValue = 0
            },
            new Goal
            {
                Id = "daily_earnings",
                Name = "일일 수익 목표", 
                Type = GoalType.Daily,
                TargetValue = 10000,
                CurrentValue = GetTodayEarnings()
            }
        });
    }
    
    public void UpdateGoalProgress()
    {
        foreach (var goal in _activeGoals.Where(g => g.Status == GoalStatus.Active))
        {
            goal.CurrentValue = goal.Id switch
            {
                "daily_watering" => GetWateredCropsCount(),
                "daily_earnings" => GetTodayEarnings(),
                _ => goal.CurrentValue
            };
            
            if (goal.CurrentValue >= goal.TargetValue)
            {
                goal.Status = GoalStatus.Completed;
                goal.CompletedDate = DateTime.Now;
                ShowGoalCompletedNotification(goal);
            }
        }
    }
}
```

#### **2.3 기본 멀티플레이어 지원**
```csharp
public class MultiplayerDataManager
{
    private readonly PerScreen<FarmStatisticsViewModel> _viewModel = new();
    private readonly IModHelper _helper;
    
    public FarmStatisticsViewModel GetViewModel()
    {
        if (_viewModel.Value == null)
        {
            _viewModel.Value = CreateViewModelForPlayer();
        }
        return _viewModel.Value;
    }
    
    private FarmStatisticsViewModel CreateViewModelForPlayer()
    {
        var viewModel = new FarmStatisticsViewModel();
        
        // 멀티플레이어 환경에서는 플레이어별 데이터 로드
        if (Context.IsMultiplayer)
        {
            LoadPlayerSpecificData(viewModel, Game1.player.UniqueMultiplayerID);
        }
        else
        {
            LoadSinglePlayerData(viewModel);
        }
        
        return viewModel;
    }
    
    // 플레이어별 독립 데이터 로드
    private void LoadPlayerSpecificData(FarmStatisticsViewModel viewModel, long playerId)
    {
        var playerData = GetPlayerData(playerId);
        viewModel.LoadFromPlayerData(playerData);
    }
}
```

### **Phase 3: 최적화 및 고도화** (2주) 🎯

#### **목표**: 성능 최적화, 완전한 멀티플레이어 지원, 고급 분석

#### **3.1 Pathoschild 캐싱 패턴 적용**
```csharp
public class FarmDataCache
{
    private readonly Dictionary<string, CacheEntry> _cache = new();
    private readonly TimeSpan _defaultExpiry = TimeSpan.FromMinutes(5);
    
    public T GetOrCompute<T>(string key, Func<T> computer, TimeSpan? expiry = null)
    {
        var expiryTime = expiry ?? _defaultExpiry;
        
        if (_cache.TryGetValue(key, out var cached) && 
            DateTime.Now < cached.Expiry)
        {
            return (T)cached.Data;
        }
        
        T newData = computer();
        _cache[key] = new CacheEntry(newData, DateTime.Now + expiryTime);
        return newData;
    }
    
    // 사용 예시
    public List<CropStatistic> GetCropStatistics()
    {
        return GetOrCompute(
            "crop_statistics", 
            () => _cropCollector.GetCurrentCrops(),
            TimeSpan.FromMinutes(2)
        );
    }
}
```

#### **3.2 배치 처리 시스템**
```csharp
public class BatchDataProcessor
{
    private readonly Queue<DataUpdateTask> _taskQueue = new();
    private readonly Timer _processingTimer;
    
    public BatchDataProcessor()
    {
        _processingTimer = new Timer(ProcessBatch, null, 
            TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }
    
    public void QueueUpdate(DataUpdateTask task)
    {
        _taskQueue.Enqueue(task);
    }
    
    private void ProcessBatch(object state)
    {
        const int maxBatchSize = 10;
        int processed = 0;
        
        while (_taskQueue.Count > 0 && processed < maxBatchSize)
        {
            var task = _taskQueue.Dequeue();
            try
            {
                task.Execute();
                processed++;
            }
            catch (Exception ex)
            {
                ModEntry.Instance.Monitor.Log($"배치 처리 오류: {ex.Message}", LogLevel.Error);
            }
        }
    }
}
```

#### **3.3 완전한 멀티플레이어 지원**
```csharp
public class MultiplayerSyncManager
{
    private readonly IModHelper _helper;
    private const string MessageType = "FarmStats.DataSync";
    
    public MultiplayerSyncManager(IModHelper helper)
    {
        _helper = helper;
        _helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
    }
    
    // 호스트에서 모든 플레이어에게 농장 데이터 브로드캐스트
    public void BroadcastFarmData()
    {
        if (!Context.IsMainPlayer) return;
        
        var farmData = new SharedFarmData
        {
            TotalEarnings = GetSharedEarnings(),
            CropStatistics = GetSharedCropData(),
            AnimalStatistics = GetSharedAnimalData(),
            Timestamp = DateTime.Now
        };
        
        _helper.Multiplayer.SendMessage(farmData, MessageType);
    }
    
    // 플레이어 개인 데이터를 호스트에게 전송
    public void SendPlayerData()
    {
        if (Context.IsMainPlayer) return;
        
        var playerData = new PlayerStatisticsData
        {
            PlayerId = Game1.player.UniqueMultiplayerID,
            PlayerName = Game1.player.Name,
            PersonalStats = GetPersonalStats(),
            Timestamp = DateTime.Now
        };
        
        _helper.Multiplayer.SendMessage(
            playerData, 
            "FarmStats.PlayerData", 
            new[] { Game1.MasterPlayer.UniqueMultiplayerID }
        );
    }
    
    private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
    {
        if (e.Type == MessageType && !Context.IsMainPlayer)
        {
            var farmData = e.ReadAs<SharedFarmData>();
            UpdateSharedData(farmData);
        }
        else if (e.Type == "FarmStats.PlayerData" && Context.IsMainPlayer)
        {
            var playerData = e.ReadAs<PlayerStatisticsData>();
            UpdatePlayerData(playerData);
        }
    }
}
```

---

## 🎨 기술적 구현 전략

### **1. 데이터 수집 최적화**

#### **이벤트 기반 수집**
```csharp
// 효율적인 이벤트 활용
helper.Events.Player.InventoryChanged += (s, e) => {
    // 인벤토리 변경 시에만 관련 통계 업데이트
    if (e.Added.Any(item => IsCrop(item.Item)))
        UpdateCropStatistics();
};

helper.Events.World.ObjectListChanged += (s, e) => {
    // 농장 오브젝트 변경 시에만 업데이트
    if (e.Location is Farm)
        QueueFarmDataUpdate();
};
```

#### **지연 로딩 패턴**
```csharp
public class LazyDataLoader<T>
{
    private T _data;
    private DateTime _lastUpdate;
    private readonly TimeSpan _cacheTime;
    private readonly Func<T> _loader;
    
    public T Data
    {
        get
        {
            if (_data == null || DateTime.Now - _lastUpdate > _cacheTime)
            {
                _data = _loader();
                _lastUpdate = DateTime.Now;
            }
            return _data;
        }
    }
}
```

### **2. UI 성능 최적화**

#### **가상화된 리스트**
```csharp
// 대량 데이터 표시를 위한 가상화
public class VirtualizedCropList : INotifyPropertyChanged
{
    private readonly List<CropStatistic> _allCrops;
    private readonly int _pageSize = 20;
    private int _currentPage = 0;
    
    public IReadOnlyList<CropStatistic> VisibleCrops =>
        _allCrops.Skip(_currentPage * _pageSize).Take(_pageSize).ToList();
        
    public void NextPage()
    {
        if ((_currentPage + 1) * _pageSize < _allCrops.Count)
        {
            _currentPage++;
            OnPropertyChanged(nameof(VisibleCrops));
        }
    }
}
```

#### **조건부 렌더링**
```xml
<!-- FarmStatistics.sml에서 조건부 표시 -->
<panel name="CropDetails" visible="{ShowCropsTab}">
    <grid name="CropList" item-layout="48px" 
          items="{CropStatistics}" 
          max-visible-items="10">
        <!-- 필요한 경우에만 렌더링 -->
    </grid>
</panel>
```

---

## 🌐 멀티플레이어 지원 전략

### **1. 데이터 범위 관리**

#### **범위별 데이터 구조**
```csharp
public enum DataScope
{
    Personal,    // 개인 통계만
    Shared,      // 공유 농장 통계
    Comparative  // 플레이어 간 비교
}

public class ScopedDataManager
{
    private readonly Dictionary<DataScope, IDataProvider> _providers = new();
    
    public void RegisterProvider(DataScope scope, IDataProvider provider)
    {
        _providers[scope] = provider;
    }
    
    public FarmStatisticsData GetData(DataScope scope)
    {
        return _providers[scope].GetData();
    }
}
```

### **2. 실시간 동기화**

#### **효율적인 동기화 전략**
```csharp
public class IncrementalSyncManager
{
    private readonly Dictionary<string, object> _lastSentData = new();
    
    public void SyncIfChanged<T>(string key, T currentData)
    {
        if (!_lastSentData.TryGetValue(key, out var lastData) || 
            !EqualityComparer<T>.Default.Equals((T)lastData, currentData))
        {
            SendUpdate(key, currentData);
            _lastSentData[key] = currentData;
        }
    }
}
```

---

## ⚡ 성능 최적화 방안

### **1. 메모리 최적화**

#### **오브젝트 풀링**
```csharp
public class StatisticObjectPool<T> where T : class, new()
{
    private readonly ConcurrentQueue<T> _objects = new();
    
    public T Get()
    {
        return _objects.TryDequeue(out T item) ? item : new T();
    }
    
    public void Return(T item)
    {
        if (item is IResettable resettable)
            resettable.Reset();
        _objects.Enqueue(item);
    }
}
```

### **2. 계산 최적화**

#### **증분 계산**
```csharp
public class IncrementalStatsCalculator
{
    private int _lastKnownMoney;
    private int _todayEarnings;
    
    public void UpdateEarnings(int newMoney)
    {
        if (newMoney > _lastKnownMoney)
        {
            _todayEarnings += (newMoney - _lastKnownMoney);
        }
        _lastKnownMoney = newMoney;
    }
}
```

---

## 📅 구현 일정

### **Week 1-2: Phase 1 구현**
- [ ] GameDataCollector 클래스 구현
- [ ] 실제 작물/동물 데이터 수집
- [ ] 기본 이벤트 핸들러 구현
- [ ] 데모 데이터를 실제 데이터로 교체

### **Week 3-4: Phase 2 구현**
- [ ] ActivityTracker 시스템 구현
- [ ] GoalManager 시스템 구현
- [ ] 기본 멀티플레이어 지원 추가
- [ ] PerScreen 패턴 적용

### **Week 5-6: Phase 3 구현**
- [ ] 캐싱 시스템 구현
- [ ] 배치 처리 시스템 구현
- [ ] 완전한 멀티플레이어 동기화
- [ ] 성능 최적화 적용

### **Week 7: 테스트 및 최적화**
- [ ] 싱글플레이어 테스트
- [ ] 멀티플레이어 테스트
- [ ] 성능 프로파일링
- [ ] 버그 수정 및 최적화

---

## 🎯 성공 기준

### **Phase 1 완료 기준**
- ✅ 모든 탭에서 실제 게임 데이터 표시
- ✅ 실시간 데이터 업데이트 작동
- ✅ 기본적인 통계 계산 정확성 확보

### **Phase 2 완료 기준**
- ✅ 활동 시간 추적 시스템 작동
- ✅ 목표 관리 시스템 완전 기능
- ✅ 멀티플레이어 환경에서 개인 데이터 표시

### **Phase 3 완료 기준**
- ✅ 게임 성능에 영향 없는 수준의 최적화
- ✅ 멀티플레이어 데이터 동기화 완전 작동
- ✅ 대용량 데이터 처리 시 안정성 확보

---

**업데이트 날짜**: 2024년 9월 17일  
**작성자**: jinhyy  
**상태**: 개발 계획 수립 완료, Phase 1 구현 준비 완료
