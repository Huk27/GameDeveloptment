using System;
using System.Collections.Generic;
using System.Linq;
using FarmDashboard.Data;
using FarmDashboard.UI;
using FarmDashboard;
using Microsoft.Xna.Framework;

namespace FarmDashboard.Hud;

internal sealed class HudViewModel
{
    private readonly List<HudCardView> _cards = new();
    private readonly List<string> _alerts = new();

    public IReadOnlyList<HudCardView> Cards => _cards;

    public IReadOnlyList<string> Alerts => _alerts;

    public void Update(FarmSnapshot snapshot, ModConfig config)
    {
        _cards.Clear();
        _alerts.Clear();

        _cards.Add(BuildGoldCard(snapshot));
        _cards.Add(BuildCropCard(snapshot, config));
        _cards.Add(BuildAnimalCard(snapshot));
        _cards.Add(BuildActivityCard(snapshot));

        if (config.EnableHudAlerts)
        {
            foreach (var highlight in snapshot.Highlights.Take(2))
                _alerts.Add(highlight);

            var urgentTask = snapshot.CareTasks.FirstOrDefault(t => !t.Completed);
            if (urgentTask != null)
                _alerts.Add($"{urgentTask.Label}: {urgentTask.Details}");
        }
    }

    private static HudCardView BuildGoldCard(FarmSnapshot snapshot)
    {
        string primary = DashboardFormatting.FormatMoney(snapshot.TodayEarnings);
        string secondary = $"Season {DashboardFormatting.FormatMoney(snapshot.SeasonEarnings)}";
        string detail = $"Wallet {DashboardFormatting.FormatMoney(snapshot.WalletBalance)}";

        var activeGoal = snapshot.Goals.FirstOrDefault(g => g.Status == GoalStatus.Active && g.TargetValue > 0);
        if (activeGoal != null)
            detail = $"{activeGoal.Name}: {activeGoal.Percentage:F0}%";

        return new HudCardView(
            "Gold Today",
            primary,
            secondary,
            detail,
            Color.Gold,
            Color.LightGray,
            false);
    }

    private static HudCardView BuildCropCard(FarmSnapshot snapshot, ModConfig config)
    {
        string primary = snapshot.TotalCrops == 0
            ? "--"
            : $"Ready {snapshot.ReadyCrops}";

        string secondary = snapshot.TotalCrops == 0
            ? "No crops planted"
            : $"Planted {snapshot.TotalCrops}, Growing {snapshot.GrowingCrops}";

        var waterTask = snapshot.CareTasks.FirstOrDefault(t => string.Equals(t.Label, "Water crops", StringComparison.OrdinalIgnoreCase));
        string detail = waterTask?.Details ?? "Fields hydrated";
        var upcoming = snapshot.HarvestForecasts
            .Where(f => f.DaysUntilReady >= 0)
            .OrderBy(f => f.DaysUntilReady)
            .FirstOrDefault();
        if (config.EnableHudNextHarvestHint && upcoming != null)
        {
            string when = upcoming.DaysUntilReady <= 0 ? "today" : $"{upcoming.DaysUntilReady}d";
            detail = $"{detail} | Next: {upcoming.Name} ({when})";
        }
        bool warning = waterTask is { Completed: false };
        Color detailColor = warning ? Color.OrangeRed : Color.LightGray;

        return new HudCardView(
            "Crop Pulse",
            primary,
            secondary,
            detail,
            warning ? Color.OrangeRed : new Color(126, 214, 223),
            detailColor,
            warning);
    }

    private static HudCardView BuildAnimalCard(FarmSnapshot snapshot)
    {
        string primary = snapshot.TotalAnimals == 0
            ? "--"
            : $"Happy {(int)Math.Round(snapshot.AverageAnimalHappiness)}%";

        string secondary = snapshot.TotalAnimals == 0
            ? "No animals"
            : $"Adult {snapshot.AdultAnimals}, Products {snapshot.ProductsReady}";

        var petTask = snapshot.CareTasks.FirstOrDefault(t => string.Equals(t.Label, "Pet animals", StringComparison.OrdinalIgnoreCase));
        string detail = petTask?.Details ?? "Everyone is content";
        bool warning = petTask is { Completed: false };

        return new HudCardView(
            "Animal Mood",
            primary,
            secondary,
            detail,
            warning ? Color.OrangeRed : new Color(46, 204, 113),
            warning ? Color.OrangeRed : Color.LightGray,
            warning);
    }

    private static HudCardView BuildActivityCard(FarmSnapshot snapshot)
    {
        string primary = DashboardFormatting.FormatTimeSpan(snapshot.TodayPlayTime);
        string secondary = snapshot.GoldPerHour <= 0
            ? "Gold/hr --"
            : $"Gold/hr {snapshot.GoldPerHour:F1}";

        var topActivity = snapshot.ActivityBreakdown
            .OrderByDescending(a => a.TimeSpent)
            .FirstOrDefault();

        string detail = topActivity == null
            ? (snapshot.Weather.Today ?? "--")
            : $"Top: {DashboardFormatting.FormatActivityName(topActivity.Activity)} ({topActivity.Percentage:F1}%)";

        if (!string.IsNullOrWhiteSpace(snapshot.Exploration?.TomorrowPlan))
            detail = snapshot.Exploration!.TomorrowPlan;

        return new HudCardView(
            "Activity Clock",
            primary,
            secondary,
            detail,
            new Color(155, 89, 182),
            Color.LightGray,
            false);
    }
}

internal sealed record HudCardView(
    string Title,
    string PrimaryValue,
    string SecondaryValue,
    string Detail,
    Color AccentColor,
    Color DetailColor,
    bool IsWarning);
