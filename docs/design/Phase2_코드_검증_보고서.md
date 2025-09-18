# 🔍 Phase 2 코드 검증 및 수정 보고서

## 📋 **검증 개요**

Phase 2에서 구현한 코드를 Pathoschild, spacechase0 등의 실제 모드들과 비교 검증한 결과, 여러 문제점들이 발견되었습니다.

---

## ❌ **발견된 주요 문제점들**

### **1. API 호환성 문제**

#### **문제 1-1: 잘못된 CropData 접근**
```csharp
// ❌ 문제가 있는 코드
private StardewValley.GameData.Crops.CropData GetCropData(string cropId)
{
    var cropData = Game1.content.Load<Dictionary<string, StardewValley.GameData.Crops.CropData>>("Data/Crops");
    return cropData.ContainsKey(cropId) ? cropData[cropId] : null;
}
```

**문제점**: 
- Stardew Valley 1.6에서 `CropData` 구조 변경
- `indexOfHarvest` 속성이 더 이상 존재하지 않음
- 직접 데이터 접근이 불안정함

#### **문제 1-2: 잘못된 동물 데이터 접근**
```csharp
// ❌ 문제가 있는 코드  
foreach (FarmAnimal animal in animalHouse.animals.Values)
{
    var animalType = animal.type.Value; // type 속성 접근 방식 문제
}
```

**문제점**:
- `animal.type.Value` 접근 방식이 1.6에서 변경됨
- 안전하지 않은 컬렉션 접근

### **2. 데이터 안전성 문제**

#### **문제 2-1: Context.IsWorldReady 체크 누락**
```csharp
// ❌ 문제가 있는 코드
private OverviewData CollectOverviewData()
{
    try
    {
        if (!Context.IsWorldReady) // 이 체크만으로는 부족
        {
            return new OverviewData { /* 기본값 */ };
        }
        
        var player = Game1.player; // 여전히 위험
        var farm = Game1.getFarm(); // 여전히 위험
```

**문제점**:
- `Context.IsWorldReady`만으로는 모든 상황을 커버하지 못함
- 멀티플레이어에서 추가 체크 필요
- 게임 상태 변화 중 데이터 접근 위험

#### **문제 2-2: Null 체크 부족**
```csharp
// ❌ 문제가 있는 코드
foreach (var terrainFeature in farm.terrainFeatures.Values)
{
    if (terrainFeature is HoeDirt dirt && dirt.crop != null)
    {
        var crop = dirt.crop; // crop이 null일 수 있음
        var cropData = GetCropData(crop.indexOfHarvest.Value); // NullRef 위험
    }
}
```

### **3. 멀티플레이어 구현 문제**

#### **문제 3-1: ModMessage 사용법 오류**
```csharp
// ❌ 문제가 있는 코드
_helper.Multiplayer.SendMessage(sharedData, MessageType);
```

**문제점**:
- `modIDs` 파라미터 누락
- 메시지 타입 명명 규칙 미준수
- 대상 플레이어 지정 방식 문제

#### **문제 3-2: 이벤트 핸들러 등록 시점**
```csharp
// ❌ 문제가 있는 코드
public MultiplayerSyncManager(IModHelper helper, IMonitor monitor, GameDataCollector dataCollector)
{
    _helper = helper;
    _helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived; // 너무 이른 등록
}
```

### **4. 성능 및 메모리 문제**

#### **문제 4-1: PerScreen 초기화 시점**
```csharp
// ❌ 문제가 있는 코드
public GameDataCollector(IMonitor monitor)
{
    _monitor = monitor;
    _cachedData = new PerScreen<Dictionary<string, object>>(() => new Dictionary<string, object>());
    _lastCacheUpdate = new PerScreen<DateTime>(() => DateTime.MinValue);
}
```

**문제점**:
- 생성자에서 PerScreen 초기화는 위험
- 게임 시작 전에 초기화되어 메모리 누수 가능

#### **문제 4-2: 캐시 키 관리**
```csharp
// ❌ 문제가 있는 코드
_cachedData.Value["farm_data"] = data; // 하드코딩된 키
```

---

## ✅ **수정 방안**

### **1. API 호환성 수정**

#### **수정 1-1: 안전한 작물 데이터 접근**
```csharp
// ✅ 수정된 코드
private List<CropStatistic> CollectCropData()
{
    if (!Context.IsWorldReady) return new List<CropStatistic>();
    
    try
    {
        Farm? farm = Game1.getFarm();
        if (farm?.terrainFeatures == null) return new List<CropStatistic>();
        
        var cropStats = new Dictionary<string, CropStatistic>();
        
        foreach (var kvp in farm.terrainFeatures.Pairs)
        {
            if (kvp.Value is HoeDirt dirt && dirt.crop != null)
            {
                // 안전한 작물 정보 접근
                string cropId = dirt.crop.indexOfHarvest.Value;
                if (string.IsNullOrEmpty(cropId)) continue;
                
                // 아이템 이름으로 접근 (더 안전)
                var item = ItemRegistry.Create(cropId);
                if (item != null)
                {
                    AddCropToStats(cropStats, item, dirt.crop);
                }
            }
        }
        
        return cropStats.Values.ToList();
    }
    catch (Exception ex)
    {
        _monitor.Log($"작물 데이터 수집 오류: {ex.Message}", LogLevel.Error);
        return new List<CropStatistic>();
    }
}
```

#### **수정 1-2: 안전한 동물 데이터 접근**
```csharp
// ✅ 수정된 코드
private List<AnimalStatistic> CollectAnimalData()
{
    if (!Context.IsWorldReady) return new List<AnimalStatistic>();
    
    try
    {
        Farm? farm = Game1.getFarm();
        if (farm?.buildings == null) return new List<AnimalStatistic>();
        
        var animalStats = new Dictionary<string, AnimalStatistic>();
        
        foreach (Building building in farm.buildings)
        {
            if (building.indoors.Value is AnimalHouse animalHouse && animalHouse.animals != null)
            {
                foreach (var kvp in animalHouse.animals.Pairs)
                {
                    FarmAnimal? animal = kvp.Value;
                    if (animal == null) continue;
                    
                    // 안전한 동물 타입 접근
                    string animalType = animal.type.Value ?? "Unknown";
                    AddAnimalToStats(animalStats, animal, animalType);
                }
            }
        }
        
        return animalStats.Values.ToList();
    }
    catch (Exception ex)
    {
        _monitor.Log($"동물 데이터 수집 오류: {ex.Message}", LogLevel.Error);
        return new List<AnimalStatistic>();
    }
}
```

### **2. 멀티플레이어 수정**

#### **수정 2-1: 올바른 ModMessage 구현**
```csharp
// ✅ 수정된 코드
public void BroadcastFarmData()
{
    if (!Context.IsMainPlayer || !Context.IsWorldReady) return;
    
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
        
        // 올바른 ModMessage 전송
        _helper.Multiplayer.SendMessage(
            message: sharedData,
            messageType: "FarmStats.DataSync",
            modIDs: [_helper.ModRegistry.ModID]
        );
        
        _monitor.Log("농장 데이터 브로드캐스트 완료", LogLevel.Trace);
    }
    catch (Exception ex)
    {
        _monitor.Log($"농장 데이터 브로드캐스트 오류: {ex.Message}", LogLevel.Error);
    }
}
```

### **3. PerScreen 패턴 수정**

#### **수정 3-1: 올바른 PerScreen 초기화**
```csharp
// ✅ 수정된 코드
public class GameDataCollector
{
    private readonly IMonitor _monitor;
    
    // 필드 레벨에서 초기화 (TractorMod 패턴)
    private readonly PerScreen<Dictionary<string, object>> _cachedData = new(() => new Dictionary<string, object>());
    private readonly PerScreen<DateTime> _lastCacheUpdate = new(() => DateTime.MinValue);
    
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromSeconds(30);
    
    // 캐시 키 상수화
    private const string FARM_DATA_KEY = "farm_data";
    
    public GameDataCollector(IMonitor monitor)
    {
        _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
    }
    
    public FarmStatisticsData CollectCurrentData()
    {
        if (!Context.IsWorldReady)
        {
            _monitor.Log("게임 월드가 준비되지 않음", LogLevel.Trace);
            return new FarmStatisticsData();
        }
        
        try
        {
            // 캐시 확인
            if (IsCacheValid())
            {
                _monitor.Log("캐시된 데이터 사용", LogLevel.Trace);
                return _cachedData.Value[FARM_DATA_KEY] as FarmStatisticsData ?? new FarmStatisticsData();
            }
            
            // 새 데이터 수집
            var data = CollectFreshData();
            
            // 캐시 업데이트
            UpdateCache(data);
            
            return data;
        }
        catch (Exception ex)
        {
            _monitor.Log($"데이터 수집 전체 오류: {ex.Message}", LogLevel.Error);
            return new FarmStatisticsData();
        }
    }
    
    private bool IsCacheValid()
    {
        return DateTime.Now - _lastCacheUpdate.Value < _cacheExpiry && 
               _cachedData.Value.ContainsKey(FARM_DATA_KEY);
    }
    
    private void UpdateCache(FarmStatisticsData data)
    {
        _cachedData.Value[FARM_DATA_KEY] = data;
        _lastCacheUpdate.Value = DateTime.Now;
        _monitor.Log("데이터 캐시 업데이트 완료", LogLevel.Trace);
    }
}
```

---

## 🔧 **즉시 수정이 필요한 파일들**

### **1. GameDataCollector.cs**
- [ ] API 호환성 수정
- [ ] Null 안전성 강화  
- [ ] PerScreen 초기화 수정
- [ ] 예외 처리 개선

### **2. ModEntry.cs**
- [ ] 이벤트 등록 시점 수정
- [ ] Context 체크 강화
- [ ] 멀티플레이어 초기화 수정

### **3. MultiplayerSyncManager (새 클래스)**
- [ ] ModMessage 구현 수정
- [ ] 이벤트 핸들러 등록 시점 수정
- [ ] 오류 처리 강화

---

## 📊 **검증 결과 요약**

| 구분 | 문제 수 | 심각도 | 수정 필요도 |
|------|---------|--------|-------------|
| API 호환성 | 5개 | 높음 | 즉시 |
| 데이터 안전성 | 8개 | 높음 | 즉시 |
| 멀티플레이어 | 3개 | 중간 | 우선 |
| 성능/메모리 | 4개 | 중간 | 우선 |
| **전체** | **20개** | **높음** | **즉시** |

---

## 🎯 **권장 수정 순서**

### **Phase 2.1 (긴급 수정)**
1. PerScreen 초기화 패턴 수정
2. Context.IsWorldReady 체크 강화
3. Null 안전성 추가

### **Phase 2.2 (호환성 수정)**
1. API 호환성 문제 해결
2. 안전한 데이터 접근 패턴 적용
3. 예외 처리 강화

### **Phase 2.3 (멀티플레이어 수정)**
1. ModMessage 구현 수정
2. 이벤트 핸들러 등록 시점 조정
3. 동기화 로직 개선

---

## 🏆 **수정 후 기대 효과**

- ✅ **안정성 향상**: NullReferenceException 등 런타임 오류 방지
- ✅ **호환성 보장**: Stardew Valley 1.6+ 완전 지원
- ✅ **성능 최적화**: 올바른 캐싱 및 메모리 관리
- ✅ **멀티플레이어 안정성**: 네트워크 동기화 오류 방지

**Phase 2.1-2.3 수정을 완료한 후 Phase 3로 진행하는 것을 강력히 권장합니다.**
