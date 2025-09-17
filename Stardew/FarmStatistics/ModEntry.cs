using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
#if STARDEWUI_AVAILABLE
using StardewUI;
using StardewUI.Framework;
#endif

namespace FarmStatistics
{
#if STARDEWUI_AVAILABLE
    // PlayerInfoMenu 클래스는 더 이상 필요하지 않습니다.
    // viewEngine.CreateMenuFromAsset()을 사용하여 직접 메뉴를 생성합니다.
#endif

    /// <summary>
    /// FarmStatistics 모드의 메인 진입점
    /// StardewUI를 사용하여 농장 통계를 표시하는 UI 모드
    /// </summary>
    public class ModEntry : Mod
    {
        private FarmStatisticsViewModel viewModel;
#if STARDEWUI_AVAILABLE
        private IViewEngine viewEngine;
#endif

        /// <summary>
        /// 모드 진입점 - SMAPI가 모드를 로드할 때 호출
        /// </summary>
        public override void Entry(IModHelper helper)
        {
            // 게임이 완전히 로드된 후에 UI를 초기화
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            
            // 키 입력 이벤트 구독 (UI 열기/닫기용)
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            
            // 실시간 데이터 업데이트를 위한 이벤트들
            helper.Events.GameLoop.TimeChanged += OnTimeChanged;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Player.InventoryChanged += OnInventoryChanged;
            helper.Events.World.ObjectListChanged += OnObjectListChanged;
            
            // 게임 상태가 변경될 때마다 데이터 업데이트 (백업용)
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            
            this.Monitor.Log("FarmStatistics 모드가 로드되었습니다. 'F' 키를 눌러 농장 통계를 확인하세요.", LogLevel.Info);
        }

        /// <summary>
        /// 게임이 완전히 로드된 후 호출되는 이벤트 핸들러
        /// </summary>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // StardewUI가 설치되어 있는지 확인
            if (this.Helper.ModRegistry.IsLoaded("focustense.StardewUI"))
            {
#if STARDEWUI_AVAILABLE
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

#if STARDEWUI_AVAILABLE
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
#else
            this.Monitor.Log("StardewUI가 컴파일 시점에 사용할 수 없습니다.", LogLevel.Warn);
#endif
        }

        /// <summary>
        /// 농장 통계 UI를 표시하는 메서드
        /// </summary>
        private void ShowFarmStatisticsUI()
        {
#if STARDEWUI_AVAILABLE
            try
            {
                if (viewEngine == null)
                {
                    this.Monitor.Log("ViewEngine이 초기화되지 않았습니다.", LogLevel.Error);
                    return;
                }

                // ViewModel 생성 (실제 데이터 사용)
                this.viewModel = new FarmStatisticsViewModel();
                this.viewModel.InitializeTabs();
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
#else
            this.Monitor.Log("StardewUI가 컴파일 시점에 사용할 수 없습니다.", LogLevel.Warn);
#endif
        }
    }
}
