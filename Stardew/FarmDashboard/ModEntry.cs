using System;
using System.Linq;
using FarmDashboard.Data;
using FarmDashboard.Hud;
using FarmDashboard.Menus;
using FarmDashboard.Services;
using FarmDashboard.StardewUI;
using FarmDashboard.UI;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewUI.Framework;
using StardewValley;

namespace FarmDashboard
{
    public class ModEntry : Mod
    {
        private FarmDataCollector? _collector;
        private DashboardHudRenderer? _hudRenderer;
        private ModConfig? _config;
        private IViewEngine? _viewEngine;
        private DashboardViewModel? _dashboardViewModel;
        private IMenuController? _dashboardMenuController;
        private string? _viewAssetPrefix;

        public override void Entry(IModHelper helper)
        {
            _config = helper.ReadConfig<ModConfig>();
            _collector = new FarmDataCollector(helper, this.Monitor, this.ModManifest.UniqueID);
            _hudRenderer = new DashboardHudRenderer(_collector, _config, this.Monitor);
            _dashboardViewModel = new DashboardViewModel();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.TimeChanged += this.OnTimeChanged;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Display.RenderedHud += this.OnRenderedHud;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            if (_config == null)
                return;

            var api = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (api != null)
            {
                api.Register(
                    mod: this.ModManifest,
                    reset: () => _config = new ModConfig(),
                    save: () => this.Helper.WriteConfig(_config));

                api.AddKeybindList(
                    mod: this.ModManifest,
                    name: () => "Toggle HUD",
                    tooltip: () => "Toggle the dashboard HUD overlay.",
                    getValue: () => CreateKeybind(_config!.ToggleHudKey),
                    setValue: value => _config!.ToggleHudKey = ExtractPrimaryButton(value));

                api.AddKeybindList(
                    mod: this.ModManifest,
                    name: () => "Open Dashboard Menu",
                    tooltip: () => "Open the Farm Dashboard management menu.",
                    getValue: () => CreateKeybind(_config!.OpenMenuKey),
                    setValue: value => _config!.OpenMenuKey = ExtractPrimaryButton(value));

                api.AddNumberOption(
                    mod: this.ModManifest,
                    name: () => "HUD X Offset",
                    tooltip: () => "Adjust the horizontal offset for the HUD.",
                    getValue: () => _config!.HudOffsetX,
                    setValue: value => _config!.HudOffsetX = value,
                    min: -200,
                    max: 1920);

                api.AddNumberOption(
                    mod: this.ModManifest,
                    name: () => "HUD Y Offset",
                    tooltip: () => "Adjust the vertical offset for the HUD.",
                    getValue: () => _config!.HudOffsetY,
                    setValue: value => _config!.HudOffsetY = value,
                    min: -200,
                    max: 1080);
            }

            InitializeStardewUi();
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            if (!Context.IsWorldReady || _collector == null)
                return;

            _collector.InitializeForSave();
            RefreshDashboardViewModel();
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            if (_collector == null)
                return;

            _collector.OnDayStarted();
            RefreshDashboardViewModelIfVisible();
        }

        private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
        {
            if (_collector == null)
                return;

            _collector.RefreshTimeSensitiveData();
            RefreshDashboardViewModelIfVisible();
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (_collector == null)
                return;

            _collector.OnUpdateTicked(e);

            if (e.IsMultipleOf(30))
                _collector.RefreshTimeSensitiveData();

            RefreshDashboardViewModelIfVisible();
        }

        private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
        {
            if (_hudRenderer == null || _config == null)
                return;

            _hudRenderer.Draw(e);
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.activeClickableMenu != null)
                return;

            if (_config == null || _collector == null)
                return;

            if (_config.ToggleHudKey != SButton.None && e.Button == _config.ToggleHudKey)
            {
                _config.ShowHud = !_config.ShowHud;
                this.Helper.WriteConfig(_config);
                Game1.playSound("smallSelect");
            }
            else if (_config.OpenMenuKey != SButton.None && e.Button == _config.OpenMenuKey)
            {
                ToggleDashboardMenu();
            }
        }

        private void ToggleDashboardMenu()
        {
            if (_collector == null)
                return;

            if (_viewEngine == null || _dashboardViewModel == null || string.IsNullOrEmpty(_viewAssetPrefix))
            {
                Game1.playSound("bigSelect");
                Game1.activeClickableMenu = new FarmDashboardMenu(() => _collector.GetSnapshot());
                return;
            }

            if (IsDashboardMenuOpen())
            {
                Game1.playSound("bigDeSelect");
                _dashboardMenuController?.Close();
                return;
            }

            DisposeDashboardMenuController();

            _dashboardViewModel.ResetToDefaultTab();
            RefreshDashboardViewModel();

            var assetName = $"{_viewAssetPrefix}/FarmDashboard";
            _dashboardMenuController = _viewEngine.CreateMenuControllerFromAsset(assetName, _dashboardViewModel);
            _dashboardMenuController.CloseSound = "bigDeSelect";
            _dashboardMenuController.DimmingAmount = 0.88f;
            _dashboardMenuController.CloseOnOutsideClick = true;
            _dashboardMenuController.EnableCloseButton();
            _dashboardMenuController.Closed += OnDashboardMenuClosed;

            Game1.playSound("bigSelect");
            _dashboardMenuController.Launch();
        }

        private void OnDashboardMenuClosed()
        {
            DisposeDashboardMenuController();
        }

        private void DisposeDashboardMenuController()
        {
            if (_dashboardMenuController == null)
                return;

            _dashboardMenuController.Closed -= OnDashboardMenuClosed;
            _dashboardMenuController.Dispose();
            _dashboardMenuController = null;
        }

        private bool IsDashboardMenuOpen()
        {
            return _dashboardMenuController?.Menu != null && ReferenceEquals(Game1.activeClickableMenu, _dashboardMenuController.Menu);
        }

        private void InitializeStardewUi()
        {
            if (!this.Helper.ModRegistry.IsLoaded("focustense.StardewUI"))
            {
                this.Monitor.Log("StardewUI not detected. Falling back to classic dashboard UI.", LogLevel.Warn);
                return;
            }

            var engine = this.Helper.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI");
            if (engine == null)
            {
                this.Monitor.Log("Failed to acquire StardewUI API. Falling back to classic dashboard UI.", LogLevel.Warn);
                return;
            }

            _viewAssetPrefix = $"Mods/{this.ModManifest.UniqueID}/Views";
            engine.RegisterViews(_viewAssetPrefix, "assets/views");
            engine.EnableHotReloading();

            _viewEngine = engine;
            _dashboardViewModel ??= new DashboardViewModel();

            this.Monitor.Log("StardewUI integration enabled for Farm Dashboard.", LogLevel.Info);
        }

        private void RefreshDashboardViewModel()
        {
            if (_collector == null || _dashboardViewModel == null)
                return;

            var snapshot = _collector.GetSnapshot();
            _dashboardViewModel.UpdateFromSnapshot(snapshot);
        }

        private void RefreshDashboardViewModelIfVisible()
        {
            if (IsDashboardMenuOpen())
            {
                RefreshDashboardViewModel();
            }
        }

        private static KeybindList CreateKeybind(SButton button)
        {
            return button == SButton.None
                ? new KeybindList()
                : KeybindList.ForSingle(button);
        }

        private static SButton ExtractPrimaryButton(KeybindList keybindList)
        {
            var keybind = keybindList.Keybinds.FirstOrDefault();
            if (keybind == null || keybind.Buttons.Length == 0)
                return SButton.None;

            return keybind.Buttons[0];
        }
    }

    public class ModConfig
    {
        public SButton ToggleHudKey { get; set; } = SButton.F2;
        public SButton OpenMenuKey { get; set; } = SButton.F3;
        public bool ShowHud { get; set; } = true;
        public int HudOffsetX { get; set; } = 40;
        public int HudOffsetY { get; set; } = 80;
        public int CardWidth { get; set; } = 260;
        public int CardHeight { get; set; } = 120;
    }

    public interface IGenericModConfigMenuApi
    {
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);

        void AddKeybindList(IManifest mod, Func<KeybindList> getValue, Action<KeybindList> setValue, Func<string> name, Func<string>? tooltip = null, string? fieldId = null);

        void AddNumberOption(IManifest mod, Func<int> getValue, Action<int> setValue, Func<string> name, Func<string>? tooltip = null, int? min = null, int? max = null, int? interval = null, Func<int, string>? formatValue = null, string? fieldId = null);
    }
}
