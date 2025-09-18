# SMAPI Developer Guide

이 문서는 `ExternalLibraries/SMAPI` 소스 코드와 본 저장소의 실사용 예제(`Stardew/FarmDashboard`, `ExampleMods/MatrixFishingUI` 등)를 기반으로 SMAPI 모드 개발 시 알아야 할 핵심 개념과 활용 패턴을 정리합니다.

## 1. 엔트리 포인트와 기본 구조

- **Mod 기반 클래스**  
  `ExternalLibraries/SMAPI/src/SMAPI/Mod.cs`에서 확인할 수 있듯이 모든 모드는 `StardewModdingAPI.Mod`를 상속받고 `Entry(IModHelper helper)`를 구현합니다. `Helper`, `Monitor`, `ModManifest` 속성은 내부에서 주입됩니다.
- **manifest.json 필수 필드**  
  `Name`, `Author`, `Version`, `Description`, `UniqueID`, `EntryDll`, `MinimumApiVersion`은 필수입니다. 예시는 `Stardew/FarmDashboard/manifest.json` 참조.
- **구성 파일 로드**  
  `IModHelper.ReadConfig<T>()` / `WriteConfig<T>()` (`ExternalLibraries/SMAPI/src/SMAPI/IModHelper.cs`)로 `config.json`을 관리합니다. 실제 사용은 `Stardew/FarmDashboard/ModEntry.cs:21`에서 확인 가능합니다.

## 2. 이벤트 시스템

`IModHelper.Events` (`ExternalLibraries/SMAPI/src/SMAPI/IModHelper.cs`)는 게임 상태 변화에 대응하는 핵심입니다. 주요 그룹과 대표 이벤트:

| 그룹 | 인터페이스 | 대표 이벤트 | 예제 |
| --- | --- | --- | --- |
| GameLoop | `IGameLoopEvents` | `GameLaunched`, `SaveLoaded`, `DayStarted`, `UpdateTicked`, `TimeChanged` | `Stardew/FarmDashboard/ModEntry.cs:25-42`
| Display | `IDisplayEvents` | `RenderedHud`, `RenderedWorld`, `RenderingActiveMenu` | `Stardew/FarmDashboard/Hud/DashboardHudRenderer.cs:26`
| Input | `IInputEvents` | `ButtonPressed`, `ButtonsChanged`, `CursorMoved` | `ExampleMods/MatrixFishingUI/MatrixFishingUI/ModEntry.cs:46`
| Player | `IPlayerEvents` | `InventoryChanged`, `Warped`, `LevelChanged` | `ExampleMods/MatrixFishingUI/MatrixFishingUI/ModEntry.cs:114`
| World | `IWorldEvents` | `ObjectListChanged`, `DebrisListChanged`, `LocationListChanged` | 대규모 시스템 모드에서 활용

### 이벤트 우선순위
- 이벤트 인터페이스는 `EventPriority` 속성을 지원 (`ExternalLibraries/SMAPI/src/SMAPI/Events/EventPriority.cs`).  
- 고급 시나리오에서 `helper.Events.GameLoop.UpdateTicked += OnUpdateTicked` 대신 `helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked; helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;` 처럼 여러 핸들러를 등록해 순서를 조절할 수 있습니다.

## 3. 데이터 및 저장소 API

- **Mod Data (`IDataHelper`)**  
  `Helper.Data.ReadGlobalData<T>()`, `WriteGlobalData`, `ReadSaveData`, `WriteSaveData`. FarmDashboard는 `Game1.player.modData`를 직접 사용하지만, SMAPI API는 별도의 저장 공간을 제공합니다.
- **ModContent/ GameContent**  
  - `Helper.ModContent.Load<T>(path)`로 모드 폴더의 자산 로드.  
  - `Helper.GameContent.InvalidateCache` / `Load`로 게임 콘텐츠 재로드 (`ExternalLibraries/SMAPI/src/SMAPI/IGameContentHelper.cs`).
- **Content API 이벤트**  
  `helper.Events.Content.AssetRequested += ...`를 통해 자산 로드 전후 개입 가능 (`ExternalLibraries/SMAPI/src/SMAPI/Events/AssetRequestedEventArgs.cs`).

## 4. 명령어 / 콘솔 지원

- `ICommandHelper` (`ExternalLibraries/SMAPI/src/SMAPI/ICommandHelper.cs`)로 사용자 정의 콘솔 명령 추가 가능.
- SpaceCore는 `SpaceCore/SpaceCore.cs:112`에서 `helper.ConsoleCommands.Add("guidebook", ...)` 패턴을 사용하고 있으므로 참고하십시오.

## 5. 멀티플레이어/메시징

- `IMultiplayerHelper` (`ExternalLibraries/SMAPI/src/SMAPI/IMultiplayerHelper.cs`)는 다음을 포함합니다:
  - `SendMessage` / `ReceiveMessage` (`ModMessageReceived` 이벤트)  
  - 플레이어 정보 (`IsMainPlayer`, `IsConnected`, `GetConnectedPlayers()`)
- 메시지 예시는 `ExternalLibraries/SMAPI/src/SMAPI/Multiplayer/MultiplayerHelper.cs` 구현을 참고하십시오.

## 6. 로그와 디버깅

- `IMonitor` (`ExternalLibraries/SMAPI/src/SMAPI/IMonitor.cs`)의 `Log(string, LogLevel)`을 사용.  
- FarmDashboard는 `Game1.playSound` 등으로 UI 피드백을 주지만, 심화 디버깅은 `Monitor.Log`를 권장합니다.

## 7. 모드 간 API 연동

- `Helper.ModRegistry.GetApi<T>(modId)` (`ExternalLibraries/SMAPI/src/SMAPI/IModRegistry.cs`)로 다른 모드의 API 획득.  
- `Stardew/FarmDashboard/ModEntry.cs:39`는 Generic Mod Config Menu(`spacechase0.GenericModConfigMenu`)를 연동하는 예시입니다.

## 8. 실전 워크플로 체크리스트

1. `manifest.json` 작성 → `Mod` 상속 클래스 생성.  
2. `Entry`에서 `ReadConfig`, 이벤트 등록, 외부 API 확보.  
3. 이벤트 핸들러 곳곳에서 `Context.IsWorldReady`, `Game1.activeClickableMenu` 등 상태 체크.  
4. 데이터 누락 방지를 위해 세이브별 초기화(`SaveLoaded`, `DayStarted`).  
5. 필요 시 `Helper.Data`/`ModContent` 사용해 저장·자산 관리.  
6. 종료 시 별도 정리 로직이 필요하면 `IDisposable` 패턴 (`Mod.Dispose`) 활용.

## 9. 참고 코드

- 최소 HUD 구현: `Stardew/FarmDashboard/Hud/DashboardHudRenderer.cs`
- 메뉴/입력 연동: `ExampleMods/MatrixFishingUI/MatrixFishingUI/ModEntry.cs`
- 외부 API 연동: `ExternalLibraries/SpacechaseFrameworks/SpaceCore/SpaceCore.cs` (GMCM, Harmony 등 고급 예시)

위 구조를 바탕으로 SMAPI 모드를 설계하면, 이벤트 흐름과 데이터 관리가 일관되게 유지됩니다.
