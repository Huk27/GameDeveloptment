using System;
using System.Collections.Generic;
using System.Linq;
using FarmDashboard.Data;
using FarmDashboard.UI;

namespace FarmDashboard.UI.Tabs;

internal sealed class OverviewTabViewModel : ObservableObject
{
    private IReadOnlyList<DashboardCard> _cards = Array.Empty<DashboardCard>();
    private IReadOnlyList<DashboardMetric> _keyMetrics = Array.Empty<DashboardMetric>();
    private IReadOnlyList<FarmHealthMetricView> _farmHealth = Array.Empty<FarmHealthMetricView>();
    private IReadOnlyList<DailyFlowView> _dailyFlow = Array.Empty<DailyFlowView>();
    private IReadOnlyList<string> _highlights = Array.Empty<string>();

    public IReadOnlyList<DashboardCard> Cards
    {
        get => _cards;
        private set => SetProperty(ref _cards, value);
    }

    public IReadOnlyList<DashboardMetric> KeyMetrics
    {
        get => _keyMetrics;
        private set => SetProperty(ref _keyMetrics, value);
    }

    public IReadOnlyList<FarmHealthMetricView> FarmHealth
    {
        get => _farmHealth;
        private set => SetProperty(ref _farmHealth, value);
    }

    public IReadOnlyList<DailyFlowView> DailyFlow
    {
        get => _dailyFlow;
        private set => SetProperty(ref _dailyFlow, value);
    }

    public IReadOnlyList<string> Highlights
    {
        get => _highlights;
        private set => SetProperty(ref _highlights, value);
    }

    public void Update(FarmSnapshot snapshot)
    {
        Cards = BuildCards(snapshot);

        KeyMetrics = new List<DashboardMetric>
        {
            new("Ready Crops", $"{snapshot.ReadyCrops:N0} / {snapshot.TotalCrops:N0}"),
            new("Products Ready", snapshot.ProductsReady.ToString("N0")),
            new("Avg Happiness", $"{snapshot.AverageAnimalHappiness:F0}%"),
            new("Luck", snapshot.Luck.Level),
            new("Forecast", snapshot.Weather.Forecast)
        };

        FarmHealth = snapshot.FarmHealthMetrics
            .Select(m => new FarmHealthMetricView(
                m.Label,
                m.Value,
                DashboardFormatting.BuildBar(m.Percentage),
                DashboardFormatting.GetSeverityColor(m.Severity)))
            .ToList();

        DailyFlow = snapshot.DailyEarnings
            .Select(entry => CreateDailyFlowView(entry, snapshot.DailyEarnings))
            .ToList();

        Highlights = snapshot.Highlights.Count > 0
            ? snapshot.Highlights
            : new List<string> { "No major updates — farm is running smoothly!" };
    }

    private static IReadOnlyList<DashboardCard> BuildCards(FarmSnapshot snapshot)
    {
        return new List<DashboardCard>
        {
            new("Today", DashboardFormatting.FormatMoney(snapshot.TodayEarnings), "#FFD700", "#FFFFFF", "Income since morning"),
            new("This Season", DashboardFormatting.FormatMoney(snapshot.SeasonEarnings), "#7FDBFF", "#FFFFFF", "Season total"),
            new("Lifetime", DashboardFormatting.FormatMoney(snapshot.LifetimeEarnings), "#2ECC40", "#FFFFFF", "Career earnings"),
            new("Wallet", DashboardFormatting.FormatMoney(snapshot.WalletBalance), "#39CCCC", "#FFFFFF", "On-hand gold"),
            new("Gold / Hour", snapshot.GoldPerHour <= 0 ? "--" : $"{snapshot.GoldPerHour:F1}", "#FF851B", "#FFFFFF", snapshot.Luck.Description),
            new("Weather", snapshot.Weather.Today, "#B10DC9", "#FFFFFF", $"Tomorrow: {snapshot.Weather.Tomorrow}")
        };
    }

    private static DailyFlowView CreateDailyFlowView(FarmSnapshot.DailyFlowEntry entry, IList<FarmSnapshot.DailyFlowEntry> history)
    {
        int maxValue = Math.Max(1, history.Select(h => Math.Max(h.Earnings, h.Expenses)).DefaultIfEmpty(1).Max());
        float positiveRatio = entry.Earnings <= 0 ? 0f : Math.Clamp(entry.Earnings / (float)maxValue, 0f, 1f);
        float negativeRatio = entry.Expenses <= 0 ? 0f : Math.Clamp(entry.Expenses / (float)maxValue, 0f, 1f);

        string netColor = entry.Net >= 0 ? "#2ECC40" : "#FF4136";

        return new DailyFlowView(
            entry.Label,
            entry.Earnings > 0 ? DashboardFormatting.FormatMoney(entry.Earnings) : "--",
            entry.Expenses > 0 ? DashboardFormatting.FormatMoney(entry.Expenses) : "--",
            entry.Net >= 0 ? $"+{DashboardFormatting.FormatMoney(entry.Net)}" : $"-{DashboardFormatting.FormatMoney(Math.Abs(entry.Net))}",
            netColor,
            DashboardFormatting.BuildBar(positiveRatio),
            DashboardFormatting.BuildBar(negativeRatio)
        );
    }
}
