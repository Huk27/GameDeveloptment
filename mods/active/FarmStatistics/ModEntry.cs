using System;
using System.Linq;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewUI;
using StardewUI.Framework;
using FarmStatistics.Analysis;

namespace FarmStatistics
{

    /// <summary>
    /// FarmStatistics 모드의 메인 진입점
    /// StardewUI를 사용하여 농장 통계를 표시하는 UI 모드
    /// </summary>
    public class ModEntry : Mod
    {
        private FarmStatisticsViewModel? viewModel;
        private GameDataCollector? dataCollector;
        private AdvancedAnalysisManager? analysisManager; // 추가
        private IViewEngine? viewEngine;
        private IClickableMenu? ui;
        private MultiplayerSyncManager? syncManager;
        private FarmStatisticsViewModel? CurrentViewModel;
        private IClickableMenu? CurrentMenu;
        private int _ticksSinceLastSync = 0;

        /// <summary>
        /// 모드 진입점 - SMAPI가 모드를 로드할 때 호출 (Phase 2 - 멀티플레이어 지원)
        /// </summary>
        public override void Entry(IModHelper helper)
        {
            // 데이터 콜렉터 초기화
            dataCollector = new GameDataCollector(this.Monitor, helper);
            analysisManager = new AdvancedAnalysisManager(this.Monitor, dataCollector); // 추가
            syncManager = new MultiplayerSyncManager(this.Monitor, helper.Multiplayer, helper);
            
            // 게임이 완전히 로드된 후에 UI를 초기화
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            
            // 세이브 파일 로드 후 초기화
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            
            // 키 입력 이벤트 구독 (UI 열기/닫기용)
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            
            // Phase 2: 실시간 데이터 업데이트를 위한 이벤트들
            helper.Events.GameLoop.TimeChanged += OnTimeChanged;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Player.InventoryChanged += OnInventoryChanged;
            helper.Events.World.ObjectListChanged += OnObjectListChanged;
            
            // Phase 2: 멀티플레이어 이벤트들
            helper.Events.Multiplayer.PeerConnected += OnPeerConnected;
            helper.Events.Multiplayer.PeerDisconnected += OnPeerDisconnected;
            helper.Events.Multiplayer.ModMessageReceived += syncManager.OnModMessageReceived;
            
            // 주기적 데이터 동기화 (멀티플레이어용)
            helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;
            
            this.Monitor.Log("FarmStatistics Phase 2 모드가 로드되었습니다. (멀티플레이어 지원) 'F' 키를 눌러 농장 통계를 확인하세요.", LogLevel.Info);
        }

        /// <summary>
        /// 게임이 완전히 로드된 후 호출되는 이벤트 핸들러
        /// </summary>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // StardewUI가 설치되어 있는지 확인
            if (this.Helper.ModRegistry.IsLoaded("focustense.StardewUI"))
            {
                viewEngine = this.Helper.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI");
                if (viewEngine != null)
                {
                    // Register the mod's views folder
                    string viewAssetPrefix = $"Mods/{this.ModManifest.UniqueID}/assets/views";
                    viewEngine.RegisterViews(viewAssetPrefix, "assets/views");
                    
                    // Enable hot reloading for development
                    viewEngine.EnableHotReloading();
                    
                    this.Monitor.Log("StardewUI가 감지되었습니다. UI 모드가 활성화됩니다.", LogLevel.Info);
                    this.Monitor.Log("Hot Reloading이 활성화되었습니다. 모드 파일을 수정하면 자동으로 반영됩니다.", LogLevel.Info);
                }
                else
                {
                    this.Monitor.Log("StardewUI API를 가져올 수 없습니다.", LogLevel.Error);
                }
            }
            else
            {
                this.Monitor.Log("StardewUI가 설치되지 않았습니다. 이 모드를 사용하려면 StardewUI가 필요합니다.", LogLevel.Error);
                this.Monitor.Log("StardewUI를 설치한 후 모드를 다시 빌드하세요.", LogLevel.Info);
            }

            // 데이터 콜렉터 초기화
            dataCollector = new GameDataCollector(this.Monitor, Helper);
            analysisManager = new AdvancedAnalysisManager(this.Monitor, dataCollector); // 추가
            syncManager = new MultiplayerSyncManager(this.Monitor, Helper.Multiplayer, Helper);
            
            // ViewModel 생성 (실제 데이터 사용)
            if (CurrentViewModel == null)
            {
                if (dataCollector != null && analysisManager != null)
                {
                    // ViewModel 생성 (실제 데이터 사용)
                    CurrentViewModel = new FarmStatisticsViewModel(dataCollector, analysisManager);
                    CurrentViewModel.UpdateData(); // 실제 데이터로 업데이트
                }
            }
            else
            {
                // 게임이 완전히 로드된 후에 UI를 초기화
                Helper.Events.GameLoop.DayStarted += OnDayStarted;
                Helper.Events.Player.InventoryChanged += OnInventoryChanged;
                Helper.Events.World.ObjectListChanged += OnObjectListChanged;
            }
        }

        /// <summary>
        /// 세이브 파일이 로드된 후 멀티플레이어 매니저 초기화 - Phase 2.1 안전성 강화
        /// </summary>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            this.Monitor.Log("세이브 파일이 로드되었습니다.", LogLevel.Info);
            ShowFarmStatisticsUI();
        }

        /// <summary>
        /// 키 입력 이벤트 핸들러
        /// </summary>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // 게임이 준비되지 않았거나 플레이어가 자유롭지 않으면 무시
            if (!Context.IsWorldReady || !Context.IsPlayerFree)
                return;

            // 'F' 키가 눌렸을 때 UI 토글
            if (e.Button == SButton.F)
            {
                ToggleFarmStatisticsUI();
            }
        }

        /// <summary>
        /// 게임 시간이 변경될 때 호출되는 이벤트 핸들러 (10분마다)
        /// </summary>
        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            // UI가 열려있을 때만 데이터 업데이트
            if (CurrentViewModel != null && ReferenceEquals(Game1.activeClickableMenu, CurrentMenu))
            {
                CurrentViewModel.UpdateData();
                this.Monitor.Log($"시간 변경으로 데이터 업데이트: {e.NewTime}", LogLevel.Trace);
            }
        }

        /// <summary>
        /// 새로운 날이 시작될 때 호출되는 이벤트 핸들러
        /// </summary>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // 새로운 날이 시작되면 데이터 업데이트
            if (CurrentViewModel != null)
            {
                CurrentViewModel.UpdateData();
                this.Monitor.Log("새로운 날 시작으로 데이터 업데이트", LogLevel.Trace);
            }
        }

        /// <summary>
        /// 플레이어 인벤토리가 변경될 때 호출되는 이벤트 핸들러
        /// </summary>
        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            // 인벤토리 변경 시 데이터 업데이트
            if (CurrentViewModel != null && ReferenceEquals(Game1.activeClickableMenu, CurrentMenu))
            {
                CurrentViewModel.UpdateData();
                this.Monitor.Log("인벤토리 변경으로 데이터 업데이트", LogLevel.Trace);
            }
        }

        /// <summary>
        /// 월드 오브젝트가 변경될 때 호출되는 이벤트 핸들러
        /// </summary>
        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            // 농장에서 오브젝트가 변경된 경우에만 업데이트
            if (e.Location is Farm && CurrentViewModel != null && ReferenceEquals(Game1.activeClickableMenu, CurrentMenu))
            {
                CurrentViewModel.UpdateData();
                this.Monitor.Log("농장 오브젝트 변경으로 데이터 업데이트", LogLevel.Trace);
            }
        }

        /// <summary>
        /// 게임 업데이트 틱 이벤트 핸들러 (백업용)
        /// </summary>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // 5초마다 데이터 업데이트 (다른 이벤트가 놓친 경우를 대비)
            if (e.Ticks % 300 == 0 && CurrentViewModel != null && ReferenceEquals(Game1.activeClickableMenu, CurrentMenu))
            {
                CurrentViewModel.UpdateData();
            }
        }


        /// <summary>
        /// 농장 통계 UI를 토글하는 메서드
        /// </summary>
        private void ToggleFarmStatisticsUI()
        {
            // StardewUI가 설치되지 않은 경우
            if (!this.Helper.ModRegistry.IsLoaded("focustense.StardewUI"))
            {
                this.Monitor.Log("StardewUI가 설치되지 않았습니다. UI를 열 수 없습니다.", LogLevel.Warn);
                return;
            }

            try
            {
                // 현재 활성 메뉴가 우리 모드의 UI인지 확인
                if (Game1.activeClickableMenu is ViewMenu currentMenu && 
                    currentMenu.GetType().Name.Contains("FarmStatistics"))
                {
                    // 이미 열려있으면 닫기
                    Game1.exitActiveMenu();
                    this.Monitor.Log("농장 통계 UI를 닫았습니다.", LogLevel.Debug);
                }
                else
                {
                    // UI 열기
                    ShowFarmStatisticsUI();
                }
            }
            catch (System.Exception ex)
            {
                this.Monitor.Log($"UI 토글 중 오류 발생: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 농장 통계 UI를 표시하는 메서드
        /// </summary>
        private void ShowFarmStatisticsUI()
        {
            if (!Context.IsWorldReady)
                return;

            if (CurrentViewModel == null)
            {
                if (dataCollector != null && analysisManager != null)
                {
                    // ViewModel 생성 (실제 데이터 사용)
                    CurrentViewModel = new FarmStatisticsViewModel(dataCollector, analysisManager);
                    CurrentViewModel.UpdateData(); // 실제 데이터로 업데이트
                }
            }
            else
            {
                // UI가 이미 열려있으면 갱신만 수행
                CurrentViewModel.UpdateData();
            }

            if (CurrentMenu == null)
            {
                if (this.viewEngine != null && CurrentViewModel != null)
                {
                    // UI 생성
                    CurrentMenu = this.viewEngine.CreateMenuFromAsset(@"assets/views/FarmStatistics.sml", CurrentViewModel);
                }
            }
            
            Game1.activeClickableMenu = CurrentMenu;
        }

        /// <summary>
        /// 멀티플레이어 피어 연결 시 호출 (Phase 2.3 - 강화된 연결 처리)
        /// </summary>
        private void OnPeerConnected(object sender, PeerConnectedEventArgs e)
        {
            this.Monitor.Log($"플레이어 연결됨: {e.Peer.PlayerID}", LogLevel.Info);
        }

        /// <summary>
        /// 멀티플레이어 피어 연결 해제 시 호출 (Phase 2.3 - 강화된 해제 처리)
        /// </summary>
        private void OnPeerDisconnected(object sender, PeerDisconnectedEventArgs e)
        {
            this.Monitor.Log($"플레이어 연결 해제됨: {e.Peer.PlayerID}", LogLevel.Info);
        }

        /// <summary>
        /// 1초마다 호출 - 멀티플레이어 데이터 동기화 - Phase 2
        /// </summary>
        private void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            _ticksSinceLastSync++;
            if (_ticksSinceLastSync >= 30)
            {
                _ticksSinceLastSync = 0;
                Task.Run(async () =>
                {
                    if (dataCollector != null && syncManager != null)
                    {
                        var farmData = await dataCollector.CollectCurrentDataAsync();
                        syncManager.SyncFarmData(farmData);
                    }
                });
            }
        }
    }
}



