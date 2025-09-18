using System;
using FarmDashboard.Data;
using FarmDashboard.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace FarmDashboard.Hud
{
    public class DashboardHudRenderer
    {
        private readonly FarmDataCollector _collector;
        private readonly ModConfig _config;
        private readonly IMonitor _monitor;

        public DashboardHudRenderer(FarmDataCollector collector, ModConfig config, IMonitor monitor)
        {
            _collector = collector;
            _config = config;
            _monitor = monitor;
        }

        public void Draw(RenderedHudEventArgs e)
        {
            if (!_config.ShowHud)
                return;

            if (!Context.IsWorldReady || Game1.eventUp || Game1.activeClickableMenu != null || !Context.IsPlayerFree)
                return;

            var snapshot = _collector.GetSnapshot();
            var spriteBatch = e.SpriteBatch;

            Vector2 basePos = new(_config.HudOffsetX, _config.HudOffsetY);
            DrawEarningsCard(spriteBatch, basePos, snapshot);
            DrawCropCard(spriteBatch, basePos + new Vector2(_config.CardWidth + 12, 0), snapshot);
            DrawAnimalCard(spriteBatch, basePos + new Vector2(0, _config.CardHeight + 10), snapshot);
            DrawTimeCard(spriteBatch, basePos + new Vector2(_config.CardWidth + 12, _config.CardHeight + 10), snapshot);
        }

        private void DrawEarningsCard(SpriteBatch spriteBatch, Vector2 position, FarmSnapshot snapshot)
        {
            string title = "오늘 수익";
            string value = FormatGold(snapshot.TodayEarnings);
            string subtitle = $"계절: {FormatGold(snapshot.SeasonEarnings)} | 누적: {FormatGold(snapshot.LifetimeEarnings)}";
            DrawCard(spriteBatch, position, title, value, subtitle);
        }

        private void DrawCropCard(SpriteBatch spriteBatch, Vector2 position, FarmSnapshot snapshot)
        {
            string title = "작물 현황";
            string value = $"수확 대기 {snapshot.ReadyCrops}";
            string subtitle = $"심은 작물 {snapshot.TotalCrops}, 성장 중 {snapshot.GrowingCrops}, 시든 {snapshot.WiltedCrops}";
            DrawCard(spriteBatch, position, title, value, subtitle);
        }

        private void DrawAnimalCard(SpriteBatch spriteBatch, Vector2 position, FarmSnapshot snapshot)
        {
            string title = "동물 현황";
            string value = $"총 {snapshot.TotalAnimals} (성체 {snapshot.AdultAnimals})";
            string subtitle = $"행복도 {snapshot.AverageAnimalHappiness:F0}%, 생산 준비 {snapshot.ProductsReady}";
            DrawCard(spriteBatch, position, title, value, subtitle);
        }

        private void DrawTimeCard(SpriteBatch spriteBatch, Vector2 position, FarmSnapshot snapshot)
        {
            string title = "시간 관리";
            string value = $"{snapshot.TodayPlayTime.Hours}h {snapshot.TodayPlayTime.Minutes}m";
            string subtitle = $"Gold/h {snapshot.GoldPerHour:F0}, 주요 활동: {GetTopActivity(snapshot)}";
            DrawCard(spriteBatch, position, title, value, subtitle);
        }

        private void DrawCard(SpriteBatch spriteBatch, Vector2 position, string title, string value, string subtitle)
        {
            var bounds = new Rectangle((int)position.X, (int)position.Y, _config.CardWidth, _config.CardHeight);
            IClickableMenu.drawTextureBox(spriteBatch, bounds.X, bounds.Y, bounds.Width, bounds.Height, Color.White);

            var titlePos = new Vector2(bounds.X + 18, bounds.Y + 16);
            var valuePos = new Vector2(bounds.X + 18, bounds.Y + 48);
            var subtitlePos = new Vector2(bounds.X + 18, bounds.Y + 78);

            spriteBatch.DrawString(Game1.smallFont, title, titlePos, Color.SandyBrown);
            spriteBatch.DrawString(Game1.dialogueFont, value, valuePos, Color.White);
            spriteBatch.DrawString(Game1.smallFont, subtitle, subtitlePos, Color.LightGray);
        }

        private string GetTopActivity(FarmSnapshot snapshot)
        {
            if (snapshot.ActivityBreakdown.Count == 0)
                return "-";

            var top = snapshot.ActivityBreakdown[0];
            return $"{top.Activity} {top.Percentage:F1}%";
        }

        private static string FormatGold(int amount)
        {
            return string.Format("{0:N0}g", amount);
        }
    }
}
