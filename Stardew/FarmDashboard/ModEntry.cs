using System;
using FarmDashboard.Data;
using FarmDashboard.Hud;
using FarmDashboard.Menus;
using FarmDashboard.Services;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace FarmDashboard
{
    public class ModEntry : Mod
    {
        private FarmDataCollector? _collector;
        private DashboardHudRenderer? _hudRenderer;
        private ModConfig? _config;

        public override void Entry(IModHelper helper)
        {
            _config = helper.ReadConfig<ModConfig>();
            _collector = new FarmDataCollector(helper, this.Monitor, this.ModManifest.UniqueID);
            _hudRenderer = new DashboardHudRenderer(_collector, _config, this.Monitor);

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
            if (api == null)
                return;

            api.Register(
                mod: this.ModManifest,
                reset: () => _config = new ModConfig(),
                save: () => this.Helper.WriteConfig(_config));

            api.AddKeybind(
                mod: this.ModManifest,
                name: () => "HUD 토글",
                tooltip: () => "HUD 표시 여부를 토글합니다.",
                getValue: () => _config!.ToggleHudKey,
                setValue: value => _config!.ToggleHudKey = value);

            api.AddKeybind(
                mod: this.ModManifest,
                name: () => "대시보드 메뉴 열기",
                tooltip: () => "Farm Dashboard 메뉴를 엽니다.",
                getValue: () => _config!.OpenMenuKey,
                setValue: value => _config!.OpenMenuKey = value);

            api.AddNumberOption(
                mod: this.ModManifest,
                name: () => "HUD X 위치",
                tooltip: () => "HUD의 가로 위치를 조정합니다.",
                getValue: () => _config!.HudOffsetX,
                setValue: value => _config!.HudOffsetX = (int)value,
                min: -200,
                max: 1920);

            api.AddNumberOption(
                mod: this.ModManifest,
                name: () => "HUD Y 위치",
                tooltip: () => "HUD의 세로 위치를 조정합니다.",
                getValue: () => _config!.HudOffsetY,
                setValue: value => _config!.HudOffsetY = (int)value,
                min: -200,
                max: 1080);
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            if (!Context.IsWorldReady || _collector == null)
                return;

            _collector.InitializeForSave();
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            if (_collector == null)
                return;

            _collector.OnDayStarted();
        }

        private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
        {
            if (_collector == null)
                return;

            _collector.RefreshTimeSensitiveData();
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (_collector == null)
                return;

            _collector.OnUpdateTicked(e);

            if (e.IsMultipleOf(30))
                _collector.RefreshTimeSensitiveData();
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

            if (e.Button == _config.ToggleHudKey)
            {
                _config.ShowHud = !_config.ShowHud;
                this.Helper.WriteConfig(_config);
                Game1.playSound("smallSelect");
            }
            else if (e.Button == _config.OpenMenuKey)
            {
                Game1.playSound("bigSelect");
                Game1.activeClickableMenu = new FarmDashboardMenu(() => _collector.GetSnapshot());
            }
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
        void Register(IManifest mod, Action reset, Action save);

        void AddKeybind(IManifest mod, Func<string> name, Func<string>? tooltip, Func<SButton> getValue, Action<SButton> setValue);

        void AddNumberOption(IManifest mod, Func<string> name, Func<string>? tooltip, Func<double> getValue, Action<double> setValue, double min, double max, double interval = 1);
    }
}
