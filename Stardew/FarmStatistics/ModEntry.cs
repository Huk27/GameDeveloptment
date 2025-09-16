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
            
            // 게임 상태가 변경될 때마다 데이터 업데이트
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
        /// 게임 업데이트 틱 이벤트 핸들러
        /// </summary>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // 60틱(1초)마다 데이터 업데이트
            if (e.Ticks % 60 == 0 && this.viewModel != null)
            {
                this.viewModel.UpdateData();
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

                // ViewModel 생성 (데모 데이터 사용)
                this.viewModel = FarmStatisticsViewModel.LoadDemoData();

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
