using System;
using System.Collections.Generic;
using System.Linq;
using FarmDashboard.Data;
using FarmDashboard.UI;

namespace FarmDashboard.UI.Tabs;

internal sealed class TimeActivityTabViewModel : ObservableObject
{
    private ActivitySummaryView _summary = new("--", "--", string.Empty);
    private IReadOnlyList<ActivityEntry> _activities = Array.Empty<ActivityEntry>();
    private IReadOnlyList<DailyFlowView> _recentGoldFlow = Array.Empty<DailyFlowView>();
    private IReadOnlyList<ActivitySuggestionView> _recommendations = Array.Empty<ActivitySuggestionView>();

    public ActivitySummaryView Summary
    {
        get => _summary;
        private set => SetProperty(ref _summary, value);
    }

    public IReadOnlyList<ActivityEntry> Activities
    {
        get => _activities;
        private set => SetProperty(ref _activities, value);
    }

    public IReadOnlyList<DailyFlowView> RecentGoldFlow
    {
        get => _recentGoldFlow;
        private set => SetProperty(ref _recentGoldFlow, value);
    }

    public IReadOnlyList<ActivitySuggestionView> Recommendations
    {
        get => _recommendations;
        private set => SetProperty(ref _recommendations, value);
    }

    public void Update(FarmSnapshot snapshot)
    {
        var summary = snapshot.ActivitySummary ?? new ActivitySummarySnapshot();
        var topSuggestion = summary.Suggestions.FirstOrDefault();

        Summary = new ActivitySummaryView(
            DashboardFormatting.FormatTimeSpan(snapshot.TodayPlayTime),
            snapshot.GoldPerHour <= 0 ? "--" : $"{snapshot.GoldPerHour:F1}",
            topSuggestion?.Message ?? snapshot.Exploration?.TomorrowPlan ?? string.Empty);

        var activityEntries = summary.Entries.Any() ? summary.Entries : snapshot.ActivityBreakdown;
        Activities = activityEntries
            .Select(entry => new ActivityEntry(
                DashboardFormatting.FormatActivityName(entry.Activity),
                DashboardFormatting.FormatTimeSpan(entry.TimeSpent),
                entry.Percentage <= 0 ? string.Empty : $"{entry.Percentage:F1}%"))
            .ToList();

        Recommendations = summary.Suggestions
            .Select(s => new ActivitySuggestionView(s.Title, s.Message, s.Category))
            .ToList();

        var earningsHistory = snapshot.DailyEarnings ?? new List<FarmSnapshot.DailyFlowEntry>();
        int maxValue = Math.Max(1, earningsHistory.Select(h => Math.Max(h.Earnings, h.Expenses)).DefaultIfEmpty(1).Max());
        int skip = Math.Max(0, earningsHistory.Count - 5);

        RecentGoldFlow = earningsHistory
            .Skip(skip)
            .Select(entry =>
            {
                float positiveRatio = entry.Earnings <= 0 ? 0f : Math.Clamp(entry.Earnings / (float)maxValue, 0f, 1f);
                float negativeRatio = entry.Expenses <= 0 ? 0f : Math.Clamp(entry.Expenses / (float)maxValue, 0f, 1f);

                return new DailyFlowView(
                    entry.Label,
                    entry.Earnings > 0 ? DashboardFormatting.FormatMoney(entry.Earnings) : "--",
                    entry.Expenses > 0 ? DashboardFormatting.FormatMoney(entry.Expenses) : "--",
                    entry.Net >= 0 ? $"+{DashboardFormatting.FormatMoney(entry.Net)}" : $"-{DashboardFormatting.FormatMoney(Math.Abs(entry.Net))}",
                    entry.Net >= 0 ? "#2ECC40" : "#FF4136",
                    DashboardFormatting.BuildBar(positiveRatio),
                    DashboardFormatting.BuildBar(negativeRatio));
            })
            .ToList();
    }
}
