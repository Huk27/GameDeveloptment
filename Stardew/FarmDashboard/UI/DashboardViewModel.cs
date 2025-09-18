using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using FarmDashboard.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace FarmDashboard.UI;

internal sealed class DashboardViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private const string OverviewTab = "overview";
    private const string ProductionTab = "production";
    private const string LivestockTab = "livestock";
    private const string EconomyTab = "economy";
    private const string ExplorationTab = "exploration";
    private const string GoalsTab = "goals";

    private readonly ObservableCollection<DashboardTab> _tabs;
    private string _activeTab;

    private IReadOnlyList<DashboardCard> _overviewCards = Array.Empty<DashboardCard>();
    private IReadOnlyList<DashboardMetric> _overviewDetails = Array.Empty<DashboardMetric>();
    private IReadOnlyList<FarmHealthMetricView> _farmHealthMetrics = Array.Empty<FarmHealthMetricView>();
    private IReadOnlyList<DailyFlowView> _dailyFlow = Array.Empty<DailyFlowView>();
    private IReadOnlyList<string> _highlights = Array.Empty<string>();

    private IReadOnlyList<DashboardMetric> _cropMetrics = Array.Empty<DashboardMetric>();
    private IReadOnlyList<DashboardMetric> _animalMetrics = Array.Empty<DashboardMetric>();
    private IReadOnlyList<ActivityEntry> _activityEntries = Array.Empty<ActivityEntry>();
    private IReadOnlyList<GoalEntry> _goalEntries = Array.Empty<GoalEntry>();
    private string _lastUpdatedText = string.Empty;

    public DashboardViewModel()
    {
        _tabs = new ObservableCollection<DashboardTab>
        {
            CreateTab(OverviewTab, "Overview", new Rectangle(20, 388, 8, 8), active: true),
            CreateTab(ProductionTab, "Production", new Rectangle(4, 372, 8, 8)),
            CreateTab(LivestockTab, "Livestock", new Rectangle(4, 388, 8, 8)),
            CreateTab(EconomyTab, "Economy", new Rectangle(294, 390, 10, 10)),
            CreateTab(ExplorationTab, "Exploration", new Rectangle(36, 374, 7, 8)),
            CreateTab(GoalsTab, "Goals", new Rectangle(420, 1204, 8, 8)),
        };

        _activeTab = OverviewTab;
    }

    public IReadOnlyList<DashboardTab> Tabs => _tabs;

    public IReadOnlyList<DashboardCard> OverviewCards
    {
        get => _overviewCards;
        private set => SetProperty(ref _overviewCards, value);
    }

    public IReadOnlyList<DashboardMetric> OverviewDetails
    {
        get => _overviewDetails;
        private set => SetProperty(ref _overviewDetails, value);
    }

    public IReadOnlyList<FarmHealthMetricView> FarmHealthMetrics
    {
        get => _farmHealthMetrics;
        private set => SetProperty(ref _farmHealthMetrics, value);
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

    public IReadOnlyList<DashboardMetric> CropMetrics
    {
        get => _cropMetrics;
        private set => SetProperty(ref _cropMetrics, value);
    }

    public IReadOnlyList<DashboardMetric> AnimalMetrics
    {
        get => _animalMetrics;
        private set => SetProperty(ref _animalMetrics, value);
    }

    public IReadOnlyList<ActivityEntry> ActivityEntries
    {
        get => _activityEntries;
        private set => SetProperty(ref _activityEntries, value);
    }

    public IReadOnlyList<GoalEntry> GoalEntries
    {
        get => _goalEntries;
        private set => SetProperty(ref _goalEntries, value);
    }

    public string LastUpdatedText
    {
        get => _lastUpdatedText;
        private set => SetProperty(ref _lastUpdatedText, value);
    }

    public bool ShowOverviewTab => string.Equals(_activeTab, OverviewTab, StringComparison.OrdinalIgnoreCase);
    public bool ShowActivityTab => string.Equals(_activeTab, ProductionTab, StringComparison.OrdinalIgnoreCase);
    public bool ShowCropsTab => string.Equals(_activeTab, ProductionTab, StringComparison.OrdinalIgnoreCase);
    public bool ShowAnimalsTab => string.Equals(_activeTab, LivestockTab, StringComparison.OrdinalIgnoreCase);
    public bool ShowGoalsTab => string.Equals(_activeTab, GoalsTab, StringComparison.OrdinalIgnoreCase);

    public void OnTabActivated(string name)
    {
        SetActiveTab(name);
    }

    public void ResetToDefaultTab()
    {
        SetActiveTab(OverviewTab);
    }

    public void UpdateFromSnapshot(FarmSnapshot snapshot)
    {
        OverviewCards = BuildOverviewCards(snapshot);

        OverviewDetails = new List<DashboardMetric>
        {
            new("Ready Crops", $"{snapshot.ReadyCrops:N0} / {snapshot.TotalCrops:N0}"),
            new("Products Ready", snapshot.ProductsReady.ToString("N0", CultureInfo.CurrentCulture)),
            new("Avg Happiness", $"{snapshot.AverageAnimalHappiness:F0}%"),
            new("Luck", snapshot.Luck.Level),
            new("Forecast", snapshot.Weather.Forecast)
        };

        FarmHealthMetrics = snapshot.FarmHealthMetrics
            .Select(m => new FarmHealthMetricView(m.Label, m.Value, BuildBar(m.Percentage), GetSeverityColor(m.Severity)))
            .ToList();

        DailyFlow = snapshot.DailyEarnings
            .Select(entry => CreateDailyFlowView(entry, snapshot.DailyEarnings))
            .ToList();

        Highlights = snapshot.Highlights.Count > 0
            ? snapshot.Highlights
            : new List<string> { "No major updates — farm is running smoothly!" };

        ActivityEntries = snapshot.ActivityBreakdown
            .Select(entry => new ActivityEntry(FormatActivityName(entry.Activity), FormatTime(entry.TimeSpent),
                entry.Percentage <= 0 ? string.Empty : $"{entry.Percentage:F1}%"))
            .ToList();

        LastUpdatedText = $"Last updated: {snapshot.LastUpdated.ToLocalTime():t}";
    }

    private IReadOnlyList<DashboardCard> BuildOverviewCards(FarmSnapshot snapshot)
    {
        var cards = new List<DashboardCard>
        {
            new("Today", FormatMoney(snapshot.TodayEarnings), "#FFD700", "#FFFFFF", "Income since morning"),
            new("This Season", FormatMoney(snapshot.SeasonEarnings), "#7FDBFF", "#FFFFFF", "Season total"),
            new("Lifetime", FormatMoney(snapshot.LifetimeEarnings), "#2ECC40", "#FFFFFF", "Career earnings"),
            new("Wallet", FormatMoney(snapshot.WalletBalance), "#39CCCC", "#FFFFFF", "On-hand gold"),
            new("Gold / Hour", snapshot.GoldPerHour <= 0 ? "--" : $"{snapshot.GoldPerHour:F1}", "#FF851B", "#FFFFFF", snapshot.Luck.Description),
            new("Weather", snapshot.Weather.Today, "#B10DC9", "#FFFFFF", $"Tomorrow: {snapshot.Weather.Tomorrow}")
        };

        return cards;
    }

    private DailyFlowView CreateDailyFlowView(DailyFlowEntry entry, IList<DailyFlowEntry> history)
    {
        int maxValue = Math.Max(1, history.Select(h => Math.Max(h.Earnings, h.Expenses)).DefaultIfEmpty(1).Max());
        float positiveRatio = entry.Earnings <= 0 ? 0f : Math.Clamp(entry.Earnings / (float)maxValue, 0f, 1f);
        float negativeRatio = entry.Expenses <= 0 ? 0f : Math.Clamp(entry.Expenses / (float)maxValue, 0f, 1f);

        string netColor = entry.Net >= 0 ? "#2ECC40" : "#FF4136";

        return new DailyFlowView(
            entry.Label,
            entry.Earnings > 0 ? FormatMoney(entry.Earnings) : "--",
            entry.Expenses > 0 ? FormatMoney(entry.Expenses) : "--",
            entry.Net >= 0 ? $"+{FormatMoney(entry.Net)}" : $"-{FormatMoney(Math.Abs(entry.Net))}",
            netColor,
            BuildBar(positiveRatio),
            BuildBar(negativeRatio)
        );
    }

    private string BuildBar(float ratio)
    {
        const int totalBlocks = 12;
        int filled = (int)Math.Round(Math.Clamp(ratio, 0f, 1f) * totalBlocks);
        if (filled <= 0)
            return string.Empty;
        return new string('█', filled);
    }

    private string GetSeverityColor(string severity)
    {
        return severity switch
        {
            "warning" => "#FF851B",
            "critical" => "#FF4136",
            "good" => "#2ECC40",
            _ => "#FFFFFF"
        };
    }

    private static DashboardTab CreateTab(string name, string displayName, Rectangle sourceRect, bool active = false)
    {
        var texture = Game1.mouseCursors;
        return new DashboardTab(name, displayName, Tuple.Create((Texture2D)texture, sourceRect))
        {
            Active = active
        };
    }

    private static string FormatMoney(int value)
    {
        return string.Format(CultureInfo.CurrentCulture, "{0:N0}g", value);
    }

    private static string FormatTime(TimeSpan span)
    {
        if (span.TotalMinutes < 1)
            return "0m";

        if (span.TotalHours < 1)
            return string.Format(CultureInfo.CurrentCulture, "{0}m", span.Minutes);

        return string.Format(CultureInfo.CurrentCulture, "{0}h {1}m", (int)span.TotalHours, span.Minutes);
    }

    private static string FormatActivityName(ActivityType activity)
    {
        return activity switch
        {
            ActivityType.Farming => "Farming",
            ActivityType.Mining => "Mining",
            ActivityType.Fishing => "Fishing",
            ActivityType.Combat => "Combat",
            ActivityType.Foraging => "Foraging",
            ActivityType.Social => "Social",
            _ => "Other"
        };
    }

    private void SetActiveTab(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        var normalized = name.ToLowerInvariant();
        if (normalized == _activeTab)
            return;

        _activeTab = normalized;

        foreach (var tab in _tabs)
        {
            tab.Active = string.Equals(tab.Name, normalized, StringComparison.OrdinalIgnoreCase);
        }

        RaisePropertyChanged(nameof(ShowOverviewTab));
        RaisePropertyChanged(nameof(ShowActivityTab));
        RaisePropertyChanged(nameof(ShowCropsTab));
        RaisePropertyChanged(nameof(ShowAnimalsTab));
        RaisePropertyChanged(nameof(ShowGoalsTab));
    }

    private void SetProperty<T>(ref IReadOnlyList<T> field, IReadOnlyList<T> value, string? propertyName = null)
    {
        if (ReferenceEquals(field, value))
            return;

        field = value;
        RaisePropertyChanged(propertyName);
    }

    private void SetProperty(ref string field, string value, string? propertyName = null)
    {
        if (string.Equals(field, value, StringComparison.Ordinal))
            return;

        field = value;
        RaisePropertyChanged(propertyName);
    }

    private void RaisePropertyChanged(string? propertyName)
    {
        if (propertyName == null)
            return;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

internal sealed class DashboardTab : INotifyPropertyChanged
{
    private bool _active;

    public DashboardTab(string name, string displayName, Tuple<Texture2D, Rectangle> sprite)
    {
        Name = name;
        DisplayName = displayName;
        Sprite = sprite;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name { get; }

    public string DisplayName { get; }

    public Tuple<Texture2D, Rectangle> Sprite { get; }

    public bool Active
    {
        get => _active;
        set
        {
            if (_active == value)
                return;

            _active = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Active)));
        }
    }
}

internal sealed record class DashboardCard(string Title, string Value, string TitleColor, string ValueColor, string Hint)
{
    public bool HasHint => !string.IsNullOrWhiteSpace(Hint);
}

internal sealed record DashboardMetric(string Label, string Value);

internal sealed record class FarmHealthMetricView(string Label, string Value, string Bar, string SeverityColor)
{
    public bool HasBar => !string.IsNullOrEmpty(Bar);
}


internal sealed record class DailyFlowView(string Label, string Earnings, string Expenses, string Net, string NetColor, string PositiveBar, string NegativeBar)
{
    public bool HasPositiveBar => !string.IsNullOrEmpty(PositiveBar);
    public bool HasNegativeBar => !string.IsNullOrEmpty(NegativeBar);
}


internal sealed record class ActivityEntry(string Label, string Duration, string PercentageText)
{
    public bool HasPercentage => !string.IsNullOrWhiteSpace(PercentageText);
}

internal sealed record GoalEntry(string Name, string Description, string StatusText, string StatusColor, string ProgressText);
