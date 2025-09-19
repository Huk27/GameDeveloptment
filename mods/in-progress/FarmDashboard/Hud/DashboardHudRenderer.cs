using System;
using System.Collections.Generic;
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
        private readonly HudViewModel _viewModel = new();
        private readonly List<CardHitRegion> _hitRegions = new();

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
            _viewModel.Update(snapshot, _config);

            var spriteBatch = e.SpriteBatch;
            Vector2 basePos = new(_config.HudOffsetX, _config.HudOffsetY);
            _hitRegions.Clear();

            for (int i = 0; i < _viewModel.Cards.Count; i++)
            {
                var card = _viewModel.Cards[i];
                int column = i % 2;
                int row = i / 2;
                Vector2 offset = new(column * (_config.CardWidth + 12), row * (_config.CardHeight + 12));
                var cardPos = basePos + offset;
                DrawCard(spriteBatch, cardPos, card);

                string targetTab = card.Title switch
                {
                    "Gold Today" => "overview",
                    "Crop Pulse" => "crops",
                    "Animal Mood" => "animals",
                    "Activity Clock" => "time",
                    _ => "overview"
                };

                _hitRegions.Add(new CardHitRegion(new Rectangle((int)cardPos.X, (int)cardPos.Y, _config.CardWidth, _config.CardHeight), targetTab));
            }

            if (_config.EnableHudAlerts)
            {
                Vector2 alertPos = basePos + new Vector2(0, 2 * (_config.CardHeight + 12));
                DrawAlerts(spriteBatch, alertPos, _viewModel.Alerts);
            }
        }

        public string? HitTest(Vector2 cursor)
        {
            if (!_config.ShowHud || _hitRegions.Count == 0)
                return null;

            foreach (var region in _hitRegions)
            {
                if (region.Bounds.Contains((int)cursor.X, (int)cursor.Y))
                    return region.TargetTab;
            }

            return null;
        }

        private void DrawCard(SpriteBatch spriteBatch, Vector2 position, HudCardView card)
        {
            var bounds = new Rectangle((int)position.X, (int)position.Y, _config.CardWidth, _config.CardHeight);
            Color borderColor = card.IsWarning ? new Color(255, 99, 71) : Color.White;
            IClickableMenu.drawTextureBox(spriteBatch, bounds.X, bounds.Y, bounds.Width, bounds.Height, borderColor);

            // Accent stripe
            var stripe = new Rectangle(bounds.X + 8, bounds.Y + 10, 6, bounds.Height - 20);
            spriteBatch.Draw(Game1.fadeToBlackRect, stripe, card.AccentColor * 0.8f);

            var titlePos = new Vector2(bounds.X + 24, bounds.Y + 16);
            var valuePos = new Vector2(bounds.X + 24, bounds.Y + 46);
            var secondaryPos = new Vector2(bounds.X + 24, bounds.Y + 76);
            var detailPos = new Vector2(bounds.X + 24, bounds.Y + 96);

            spriteBatch.DrawString(Game1.smallFont, card.Title, titlePos, card.AccentColor);
            spriteBatch.DrawString(Game1.dialogueFont, card.PrimaryValue, valuePos, Color.White);

            if (!string.IsNullOrWhiteSpace(card.SecondaryValue))
                spriteBatch.DrawString(Game1.smallFont, card.SecondaryValue, secondaryPos, Color.LightGray);

            if (!string.IsNullOrWhiteSpace(card.Detail))
                spriteBatch.DrawString(Game1.smallFont, card.Detail, detailPos, card.DetailColor);
        }

        private void DrawAlerts(SpriteBatch spriteBatch, Vector2 position, IReadOnlyList<string> alerts)
        {
            if (alerts.Count == 0 || !_config.EnableHudAlerts)
                return;

            int width = (_config.CardWidth * 2) + 12;
            int lineHeight = Game1.smallFont.LineSpacing;
            int height = alerts.Count * (lineHeight + 2) + 20;

            var bounds = new Rectangle((int)position.X, (int)position.Y, width, height);
            IClickableMenu.drawTextureBox(spriteBatch, bounds.X, bounds.Y, bounds.Width, bounds.Height, new Color(255, 255, 255, 220));

            Vector2 textPos = new(bounds.X + 18, bounds.Y + 12);
            foreach (var alert in alerts)
            {
                spriteBatch.DrawString(Game1.smallFont, $"• {alert}", textPos, Color.LightGoldenrodYellow);
                textPos.Y += lineHeight + 2;
            }
        }
    }
}

internal sealed record CardHitRegion(Rectangle Bounds, string TargetTab);
