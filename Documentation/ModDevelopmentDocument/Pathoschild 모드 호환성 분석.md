# 🔍 Pathoschild 모드 호환성 분석 (Stardew 1.6+ / SMAPI 4.0+)

> **목적**: Pathoschild 모드들의 Stardew Valley 1.6 및 SMAPI 4.0+ 호환성을 분석하여 참고 가능한 최신 코드만을 식별합니다.

## 📋 호환성 분석 결과

### ✅ **최신 호환성 모드 (참고 권장)**

#### 1. **LookupAnything** - 최신 버전 ⭐⭐⭐
```json
{
    "MinimumApiVersion": "4.3.1",
    "MinimumGameVersion": "1.6.15"
}
```
- **상태**: ✅ 완전 호환
- **참고 가치**: ⭐⭐⭐ 매우 높음
- **주요 기능**: 실시간 데이터 분석, 게임 객체 정보 추출
- **우리 프로젝트 적용**: FarmStatistics의 데이터 분석 패턴

#### 2. **Automate** - 호환 가능 ⭐⭐⭐
```json
{
    "MinimumApiVersion": "4.1.10"
}
```
- **상태**: ✅ 호환 (SMAPI 4.1.10+)
- **참고 가치**: ⭐⭐⭐ 매우 높음
- **주요 기능**: 자동화 시스템, 기계-상자 연동
- **우리 프로젝트 적용**: 자동화 패턴, 성능 최적화

#### 3. **ChestsAnywhere** - 호환 가능 ⭐⭐⭐
```json
{
    "MinimumApiVersion": "4.1.10"
}
```
- **상태**: ✅ 호환 (SMAPI 4.1.10+)
- **참고 가치**: ⭐⭐⭐ 매우 높음
- **주요 기능**: 복잡한 UI 시스템, 검색/필터링
- **우리 프로젝트 적용**: UI 패턴, 데이터 관리

#### 4. **ContentPatcher** - 호환 가능 ⭐⭐
```json
{
    "MinimumApiVersion": "4.1.10"
}
```
- **상태**: ✅ 호환 (SMAPI 4.1.10+)
- **참고 가치**: ⭐⭐ 높음
- **주요 기능**: 콘텐츠 패칭, 조건부 로딩
- **우리 프로젝트 적용**: 설정 시스템, 조건부 기능

### ⚠️ **제한적 참고 모드**

#### 1. **DataLayers, FastAnimations, CropsAnytimeAnywhere**
```json
{
    "MinimumApiVersion": "4.1.10"
}
```
- **상태**: ⚠️ 최소 호환 (SMAPI 4.1.10)
- **참고 가치**: ⭐ 보통 (기본 패턴만 참고)
- **주의사항**: 최신 API 기능 미사용 가능성

### ❌ **참고 금지 모드**

#### 1. **_archived 폴더의 모드들**
- **TheLongNight**: 아카이브됨 (Stardew 1.3.20에서 중단)
- **RotateToolbar**: 아카이브됨 (게임 기본 기능으로 통합)
- **상태**: ❌ 완전히 오래됨
- **참고 가치**: ❌ 참고 금지

---

## 🔍 최신 코드 패턴 분석

### **1. 최신 Game1 API 사용 패턴**

#### ✅ **LookupAnything에서 발견된 최신 패턴**
```csharp
// ✅ 최신: 1.6+ 호환 메뉴 시스템
if (Game1.activeClickableMenu is LookupMenu menu)
{
    // 최신 메뉴 처리 로직
}

// ✅ 최신: IScrollableMenu 인터페이스 활용
(Game1.activeClickableMenu as IScrollableMenu)?.ScrollUp();

// ✅ 최신: 스프라이트 배치 시스템
this.DebugInterface.Value.Draw(Game1.spriteBatch);
```

#### ❌ **피해야 할 오래된 패턴**
```csharp
// ❌ 오래됨: 1.5 이하 방식
Game1.player.freezePause = Game1.currentGameTime.ElapsedGameTime.Milliseconds + 1;

// ❌ 오래됨: 구식 도구 시스템
Game1.player.shiftToolbar(next);
Game1.player.CurrentToolIndex = int.MaxValue;
```

### **2. 멀티플레이어 호환성 패턴**

#### ✅ **TractorMod의 최신 멀티플레이어 패턴**
```csharp
// ✅ 최신: Context.IsMainPlayer 사용
if (Context.IsMainPlayer)
{
    // 호스트 전용 로직
}

// ✅ 최신: PerScreen 패턴
private PerScreen<TractorManager> TractorManagerImpl = new();

// ✅ 최신: ModMessage API
this.Helper.Multiplayer.SendMessage(data, messageType);

// ✅ 최신: 호스트 버전 확인
ISemanticVersion? hostVersion = this.Helper.Multiplayer
    .GetConnectedPlayer(Game1.MasterPlayer.UniqueMultiplayerID)?
    .GetMod(this.ModManifest.UniqueID)?.Version;
```

### **3. 이벤트 시스템 최신 패턴**

#### ✅ **최신 이벤트 처리 (모든 현대 모드에서 공통)**
```csharp
public override void Entry(IModHelper helper)
{
    // ✅ 최신: 표준 이벤트 구독
    helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
    helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
    
    // ✅ 최신: 멀티플레이어 이벤트
    helper.Events.Multiplayer.ModMessageReceived += this.OnModMessageReceived;
}
```

### **4. 데이터 접근 최신 패턴**

#### ✅ **LookupAnything의 최신 데이터 접근**
```csharp
// ✅ 최신: 1.6+ 게임 데이터 접근
using StardewValley.GameData.Crops;
using StardewValley.GameData.FishPonds;
using StardewValley.GameData.Locations;
using StardewValley.ItemTypeDefinitions;

// ✅ 최신: 확장 메서드 사용
using StardewValley.Extensions;

// ✅ 최신: 새로운 객체 시스템
using StardewValley.Objects;
using SObject = StardewValley.Object;
```

---

## 🚀 FarmStatistics에 적용할 최신 패턴

### **1. 데이터 수집 최신 패턴**

```csharp
// ✅ LookupAnything 패턴 적용
public class ModernFarmDataCollector
{
    /// <summary>최신 1.6+ API를 사용한 농장 데이터 수집</summary>
    public FarmData CollectFarmData()
    {
        var farm = Game1.getFarm();
        var data = new FarmData();
        
        // ✅ 최신: 1.6+ 작물 데이터 접근
        foreach (var terrainFeature in farm.terrainFeatures.Values)
        {
            if (terrainFeature is HoeDirt dirt && dirt.crop != null)
            {
                var crop = dirt.crop;
                var cropData = Game1.cropData.GetValueOrDefault(crop.indexOfHarvest.Value);
                
                data.Crops.Add(new CropInfo
                {
                    Id = crop.indexOfHarvest.Value,
                    Name = cropData?.DisplayName ?? "Unknown",
                    Phase = crop.currentPhase.Value,
                    DaysToMaturity = crop.phaseDays.Count - crop.currentPhase.Value,
                    Quality = CalculateCropQuality(crop)
                });
            }
        }
        
        return data;
    }
}
```

### **2. 멀티플레이어 지원 최신 패턴**

```csharp
// ✅ TractorMod 패턴 적용
public class ModernMultiplayerSupport
{
    private readonly PerScreen<FarmStatisticsViewModel> _viewModel = new();
    private readonly IModHelper _helper;
    
    public ModernMultiplayerSupport(IModHelper helper)
    {
        _helper = helper;
        
        // ✅ 최신: 멀티플레이어 메시지 구독
        _helper.Events.Multiplayer.ModMessageReceived += this.OnModMessageReceived;
    }
    
    /// <summary>호스트에서 통계 데이터 브로드캐스트</summary>
    public void BroadcastFarmStatistics()
    {
        if (Context.IsMainPlayer)
        {
            var stats = CollectSharedFarmStatistics();
            _helper.Multiplayer.SendMessage(stats, "FarmStats.Update");
        }
    }
    
    /// <summary>팜핸드에서 개인 데이터 전송</summary>
    public void SendPlayerStatistics()
    {
        if (!Context.IsMainPlayer)
        {
            var playerStats = CollectPlayerStatistics();
            _helper.Multiplayer.SendMessage(
                playerStats, 
                "FarmStats.PlayerData",
                new[] { Game1.MasterPlayer.UniqueMultiplayerID }
            );
        }
    }
}
```

### **3. UI 시스템 최신 패턴**

```csharp
// ✅ ChestsAnywhere 패턴 적용
public class ModernUISystem
{
    /// <summary>최신 StardewUI 패턴</summary>
    public void ShowStatisticsUI()
    {
        // ✅ 최신: 메뉴 상태 확인
        if (Game1.activeClickableMenu is ViewMenu currentMenu)
        {
            Game1.exitActiveMenu();
            return;
        }
        
        // ✅ 최신: ViewModel 생성
        var viewModel = _viewModel.Value ??= CreateViewModel();
        
        // ✅ 최신: StardewUI 메뉴 생성
        var menu = _viewEngine.CreateMenuFromAsset(
            $"Mods/{this.ModManifest.UniqueID}/assets/views/FarmStatistics",
            viewModel
        );
        
        Game1.activeClickableMenu = menu;
    }
}
```

---

## 📊 권장 참고 우선순위

### **1순위: 필수 참고 모드** 🔥
1. **LookupAnything** (MinimumGameVersion: 1.6.15)
   - 데이터 분석 패턴
   - 최신 게임 API 사용법
   - 성능 최적화 기법

2. **Automate** (MinimumApiVersion: 4.1.10)
   - 자동화 시스템 아키텍처
   - 배치 처리 패턴
   - 캐싱 전략

3. **ChestsAnywhere** (MinimumApiVersion: 4.1.10)
   - 복잡한 UI 시스템
   - 검색/필터링 기능
   - 데이터 관리 패턴

### **2순위: 선택적 참고 모드** ⚡
- **ContentPatcher**: 설정 시스템
- **DataLayers**: 오버레이 시스템
- **TractorMod**: 멀티플레이어 패턴

### **3순위: 참고 금지** ❌
- `_archived/` 폴더의 모든 모드
- 15개월 이상 업데이트되지 않은 코드
- MinimumApiVersion < 4.0인 코드

---

## 🎯 결론 및 권장사항

### **✅ 안전한 참고 기준**
1. **MinimumApiVersion >= 4.1.10**
2. **MinimumGameVersion >= 1.6.0** (가능하면)
3. **최근 6개월 내 업데이트된 코드**
4. **활성 상태의 모드** (아카이브되지 않은)

### **🚀 FarmStatistics 구현 전략**
1. **LookupAnything**의 데이터 분석 패턴을 기반으로 구현
2. **Automate**의 성능 최적화 패턴 적용
3. **ChestsAnywhere**의 UI 시스템 패턴 활용
4. **TractorMod**의 멀티플레이어 지원 패턴 참고

이제 **검증된 최신 코드**만을 기반으로 FarmStatistics를 구현할 수 있습니다!

---

**업데이트 날짜**: 2024년 7월 25일  
**작성자**: jinhyy  
**상태**: 호환성 분석 완료, 최신 패턴 식별 완료
