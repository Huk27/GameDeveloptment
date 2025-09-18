using System;
using System.Collections.Generic;
using FarmDashboard.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace FarmDashboard.Menus
{
    public class FarmDashboardMenu : IClickableMenu
    {
        private readonly Func<FarmSnapshot> _snapshotProvider;
        private readonly Rectangle _contentBounds;

        public FarmDashboardMenu(Func<FarmSnapshot> snapshotProvider)
            : base(0, 0, 960, 600, showUpperRightCloseButton: true)
        {
            _snapshotProvider = snapshotProvider;
            this.xPositionOnScreen = (Game1.viewport.Width - this.width) / 2;
            this.yPositionOnScreen = (Game1.viewport.Height - this.height) / 2;
            _contentBounds = new Rectangle(this.xPositionOnScreen + 40, this.yPositionOnScreen + 80, this.width - 80, this.height - 120);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            Game1.playSound("bigDeSelect");
            exitThisMenu();
        }

        public override void receiveKeyPress(Microsoft.Xna.Framework.Input.Keys key)
        {
            if (key == Microsoft.Xna.Framework.Input.Keys.Escape)
            {
                exitThisMenu();
                return;
            }

            base.receiveKeyPress(key);
        }

        public override void draw(SpriteBatch b)
        {
            Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, speaker: false, drawOnlyBox: true);
            Utility.drawTextWithShadow(b, "Farm Operations Dashboard", Game1.dialogueFont, new Vector2(this.xPositionOnScreen + 60, this.yPositionOnScreen + 32), Game1.textColor);

            var snapshot = _snapshotProvider();

            DrawOverview(b, snapshot);
            DrawActivity(b, snapshot);
            DrawGoals(b, snapshot.Goals);

            this.drawMouse(b);
        }

        private void DrawOverview(SpriteBatch spriteBatch, FarmSnapshot snapshot)
        {
            var start = new Vector2(_contentBounds.X, _contentBounds.Y);
            float lineHeight = Game1.smallFont.LineSpacing + 6;

            spriteBatch.DrawString(Game1.smallFont, "개요", start, Color.Gold);
            start.Y += lineHeight;
            spriteBatch.DrawString(Game1.smallFont, $"오늘 수익: {snapshot.TodayEarnings:N0}g", start, Color.White);
            start.Y += lineHeight;
            spriteBatch.DrawString(Game1.smallFont, $"계절 수익: {snapshot.SeasonEarnings:N0}g", start, Color.White);
            start.Y += lineHeight;
            spriteBatch.DrawString(Game1.smallFont, $"누적 수익: {snapshot.LifetimeEarnings:N0}g", start, Color.White);
            start.Y += lineHeight * 2;
            spriteBatch.DrawString(Game1.smallFont, $"작물: 수확 대기 {snapshot.ReadyCrops} / 총 {snapshot.TotalCrops}", start, Color.White);
            start.Y += lineHeight;
            spriteBatch.DrawString(Game1.smallFont, $"동물: 총 {snapshot.TotalAnimals}, 행복도 {snapshot.AverageAnimalHappiness:F0}%", start, Color.White);
        }

        private void DrawActivity(SpriteBatch spriteBatch, FarmSnapshot snapshot)
        {
            var start = new Vector2(_contentBounds.X + _contentBounds.Width / 2, _contentBounds.Y);
            float lineHeight = Game1.smallFont.LineSpacing + 6;

            spriteBatch.DrawString(Game1.smallFont, "시간 통계", start, Color.Gold);
            start.Y += lineHeight;
            spriteBatch.DrawString(Game1.smallFont, $"오늘 플레이: {snapshot.TodayPlayTime.Hours}h {snapshot.TodayPlayTime.Minutes}m", start, Color.White);
            start.Y += lineHeight;
            spriteBatch.DrawString(Game1.smallFont, $"Gold per Hour: {snapshot.GoldPerHour:F1}", start, Color.White);
            start.Y += lineHeight * 2;

            foreach (var entry in snapshot.ActivityBreakdown)
            {
                spriteBatch.DrawString(Game1.smallFont, $"{entry.Activity}: {entry.TimeSpent.Hours}h {entry.TimeSpent.Minutes}m ({entry.Percentage:F1}%)", start, Color.White);
                start.Y += lineHeight;
            }
        }

        private void DrawGoals(SpriteBatch spriteBatch, List<GoalProgress> goals)
        {
            var start = new Vector2(_contentBounds.X, _contentBounds.Y + _contentBounds.Height / 2 + 20);
            float lineHeight = Game1.smallFont.LineSpacing + 12;

            spriteBatch.DrawString(Game1.smallFont, "목표 진행", start, Color.Gold);
            start.Y += lineHeight;

            foreach (var goal in goals)
            {
                spriteBatch.DrawString(Game1.smallFont, goal.Name, start, goal.Status == GoalStatus.Completed ? Color.LightGreen : Color.White);
                start.Y += Game1.smallFont.LineSpacing + 2;

                spriteBatch.DrawString(Game1.smallFont, goal.Description, new Vector2(start.X + 12, start.Y), Color.LightGray);
                start.Y += Game1.smallFont.LineSpacing + 2;

                DrawGoalBar(spriteBatch, new Rectangle((int)start.X + 12, (int)start.Y, 320, 18), goal);
                start.Y += 30;
            }
        }

        private void DrawGoalBar(SpriteBatch spriteBatch, Rectangle bounds, GoalProgress goal)
        {
            spriteBatch.Draw(Game1.staminaRect, bounds, Color.Black * 0.4f);
            int fillWidth = (int)(bounds.Width * Math.Clamp(goal.Percentage / 100f, 0f, 1f));
            var fillRect = new Rectangle(bounds.X, bounds.Y, fillWidth, bounds.Height);
            spriteBatch.Draw(Game1.staminaRect, fillRect, goal.Status == GoalStatus.Completed ? Color.LimeGreen * 0.8f : Color.CornflowerBlue * 0.8f);

            var label = $"{goal.CurrentValue}/{goal.TargetValue} ({goal.Percentage:F0}%)";
            var labelPos = new Vector2(bounds.X + 6, bounds.Y - 2);
            spriteBatch.DrawString(Game1.tinyFont, label, labelPos, Color.White);
        }
    }
}
