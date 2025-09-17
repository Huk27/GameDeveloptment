using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Threading.Tasks;
using StardewUI;
using StardewUI.Framework;

namespace FarmStatistics
{

    /// <summary>
    /// FarmStatistics 모드의 메인 진입점
    /// StardewUI를 사용하여 농장 통계를 표시하는 UI 모드
    /// </summary>
    public class ModEntry : Mod
    {
        private FarmStatisticsViewModel viewModel;
        private GameDataCollector dataCollector;
        private MultiplayerSyncManager syncManager;
        private IViewEngine viewEngine;

        /// <summary>
        /// 모드 진입점 - SMAPI가 모드를 로드할 때 호출 (Phase 2 - 멀티플레이어 지원)
        /// </summary>
        public override void Entry(IModHelper helper)
        {
            // Phase 2: 데이터 콜렉터 초기화
            dataCollector = new GameDataCollector(this.Monitor);
            
            // 게임이 완전히 로드된 후에 UI를 초기화
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            
            // 세이브 파일 로드 후 멀티플레이어 매니저 초기화
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
#else
                this.Monitor.Log("StardewUI가 감지되었습니다. UI 모드가 활성화됩니다.", LogLevel.Info);
#endif
            }
            else
            {
                this.Monitor.Log("StardewUI가 설치되지 않았습니다. 이 모드를 사용하려면 StardewUI가 필요합니다.", LogLevel.Error);
                this.Monitor.Log("StardewUI를 설치한 후 모드를 다시 빌드하세요.", LogLevel.Info);
            }
        }

        /// <summary>
        /// 세이브 파일이 로드된 후 멀티플레이어 매니저 초기화 - Phase 2.1 안전성 강화
        /// </summary>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            try
            {
                // Phase 2.1: 안전한 멀티플레이어 매니저 초기화
                if (dataCollector == null)
                {
                    this.Monitor.Log("데이터 콜렉터가 초기화되지 않음", LogLevel.Error);
                    return;
                }

                syncManager = new MultiplayerSyncManager(this.Helper, this.Monitor, dataCollector);
                
                this.Monitor.Log($"멀티플레이어 매니저 초기화 완료. 호스트: {Context.IsMainPlayer}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"멀티플레이어 매니저 초기화 오류: {ex.Message}", LogLevel.Error);
                this.Monitor.Log($"스택 트레이스: {ex.StackTrace}", LogLevel.Debug);
            }
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
            if (this.viewModel != null && Game1.activeClickableMenu != null)
            {
                this.viewModel.UpdateData();
                this.Monitor.Log($"시간 변경으로 데이터 업데이트: {e.NewTime}", LogLevel.Trace);
            }
        }

        /// <summary>
        /// 새로운 날이 시작될 때 호출되는 이벤트 핸들러
        /// </summary>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // 새로운 날이 시작되면 데이터 업데이트
            if (this.viewModel != null)
            {
                this.viewModel.UpdateData();
                this.Monitor.Log("새로운 날 시작으로 데이터 업데이트", LogLevel.Trace);
            }
        }

        /// <summary>
        /// 플레이어 인벤토리가 변경될 때 호출되는 이벤트 핸들러
        /// </summary>
        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            // 작물이나 동물 제품이 추가/제거된 경우에만 업데이트
            bool hasRelevantItems = e.Added.Any(item => IsCropOrAnimalProduct(item.Item)) ||
                                   e.Removed.Any(item => IsCropOrAnimalProduct(item.Item));

            if (hasRelevantItems && this.viewModel != null && Game1.activeClickableMenu != null)
            {
                this.viewModel.UpdateData();
                this.Monitor.Log("인벤토리 변경으로 데이터 업데이트", LogLevel.Trace);
            }
        }

        /// <summary>
        /// 월드 오브젝트가 변경될 때 호출되는 이벤트 핸들러
        /// </summary>
        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            // 농장에서 오브젝트가 변경된 경우에만 업데이트
            if (e.Location is Farm && this.viewModel != null && Game1.activeClickableMenu != null)
            {
                this.viewModel.UpdateData();
                this.Monitor.Log("농장 오브젝트 변경으로 데이터 업데이트", LogLevel.Trace);
            }
        }

        /// <summary>
        /// 게임 업데이트 틱 이벤트 핸들러 (백업용)
        /// </summary>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // 5초마다 데이터 업데이트 (다른 이벤트가 놓친 경우를 대비)
            if (e.Ticks % 300 == 0 && this.viewModel != null && Game1.activeClickableMenu != null)
            {
                this.viewModel.UpdateData();
            }
        }

        /// <summary>
        /// 아이템이 작물이나 동물 제품인지 확인하는 헬퍼 메서드
        /// </summary>
        private bool IsCropOrAnimalProduct(StardewValley.Item item)
        {
            if (item is StardewValley.Object obj)
            {
                // 작물 (과일: -79, 채소: -75) 또는 동물 제품 (-18)
                return obj.Category == -79 || obj.Category == -75 || obj.Category == -18;
            }
            return false;
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
            try
            {
                if (viewEngine == null)
                {
                    this.Monitor.Log("ViewEngine이 초기화되지 않았습니다.", LogLevel.Error);
                    return;
                }

                // Phase 2.1: 안전한 ViewModel 생성 (실제 데이터 사용)
                if (dataCollector == null)
                {
                    this.Monitor.Log("데이터 콜렉터가 초기화되지 않았습니다.", LogLevel.Error);
                    return;
                }

                this.viewModel = new FarmStatisticsViewModel(dataCollector);
                this.viewModel.UpdateData(); // 실제 게임 데이터로 업데이트

                // StardewUI를 사용하여 UI 생성 (탭 시스템 사용)
                string viewAssetPrefix = $"Mods/{this.ModManifest.UniqueID}/assets/views";
                var ui = viewEngine.CreateMenuFromAsset($"{viewAssetPrefix}/FarmStatistics", this.viewModel);

                // 게임의 활성 메뉴로 설정
                Game1.activeClickableMenu = ui;

                this.Monitor.Log("농장 통계 UI를 열었습니다.", LogLevel.Debug);
            }
            catch (System.Exception ex)
            {
                this.Monitor.Log($"UI 표시 중 오류 발생: {ex.Message}", LogLevel.Error);
                this.Monitor.Log($"스택 트레이스: {ex.StackTrace}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 멀티플레이어 피어 연결 시 호출 (Phase 2.3 - 강화된 연결 처리)
        /// </summary>
        private void OnPeerConnected(object sender, PeerConnectedEventArgs e)
        {
            try
            {
                this.Monitor.Log($"플레이어 연결됨: {e.Peer.PlayerID} (모드 버전: {e.Peer.GetMod(this.ModManifest.UniqueID)?.Version ?? "없음"})", LogLevel.Info);
                
                // 호스트인 경우 새 플레이어에게 즉시 농장 데이터 전송
                if (Context.IsMainPlayer && syncManager != null)
                {
                    // 약간의 지연 후 데이터 전송 (연결 안정화 대기)
                    Task.Delay(1000).ContinueWith(_ =>
                    {
                        try
                        {
                            syncManager.BroadcastFarmData();
                            this.Monitor.Log($"새 플레이어 {e.Peer.PlayerID}에게 농장 데이터 전송 완료", LogLevel.Debug);
                        }
                        catch (Exception ex)
                        {
                            this.Monitor.Log($"새 플레이어에게 데이터 전송 중 오류: {ex.Message}", LogLevel.Error);
                        }
                    });
                }
                
                // 데이터 캐시 클리어 (새 플레이어 정보 반영)
                if (dataCollector != null)
                {
                    dataCollector.ClearCache();
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"플레이어 연결 처리 중 오류: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 멀티플레이어 피어 연결 해제 시 호출 (Phase 2.3 - 강화된 해제 처리)
        /// </summary>
        private void OnPeerDisconnected(object sender, PeerDisconnectedEventArgs e)
        {
            try
            {
                this.Monitor.Log($"플레이어 연결 해제됨: {e.Peer.PlayerID}", LogLevel.Info);
                
                // 호스트인 경우 남은 플레이어들에게 업데이트된 정보 전송
                if (Context.IsMainPlayer && syncManager != null)
                {
                    // 약간의 지연 후 업데이트 (연결 해제 처리 완료 대기)
                    Task.Delay(2000).ContinueWith(_ =>
                    {
                        try
                        {
                            // 데이터 캐시 클리어
                            dataCollector?.ClearCache();
                            
                            // 남은 플레이어들에게 업데이트된 농장 데이터 전송
                            syncManager.BroadcastFarmData();
                            
                            this.Monitor.Log($"플레이어 {e.Peer.PlayerID} 해제 후 데이터 업데이트 완료", LogLevel.Debug);
                        }
                        catch (Exception ex)
                        {
                            this.Monitor.Log($"플레이어 해제 후 데이터 업데이트 중 오류: {ex.Message}", LogLevel.Error);
                        }
                    });
                }
                
                // UI가 열려있는 경우 데이터 새로고침
                if (this.viewModel != null && Game1.activeClickableMenu != null)
                {
                    this.viewModel.UpdateData();
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"플레이어 연결 해제 처리 중 오류: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 1초마다 호출 - 멀티플레이어 데이터 동기화 - Phase 2
        /// </summary>
        private void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            // 30초마다 멀티플레이어 데이터 동기화
            if (e.IsMultipleOf(30) && syncManager != null && Context.IsWorldReady)
            {
                syncManager.PeriodicSync();
            }
        }
    }
}
