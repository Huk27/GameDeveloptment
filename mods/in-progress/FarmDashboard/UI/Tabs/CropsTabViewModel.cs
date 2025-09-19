using System;
using System.Collections.Generic;
using System.Linq;
using FarmDashboard.Data;
using FarmDashboard.UI;

namespace FarmDashboard.UI.Tabs;

internal sealed class CropsTabViewModel : ObservableObject
{
    private IReadOnlyList<CropSummaryView> _summaries = Array.Empty<CropSummaryView>();
    private IReadOnlyList<CropTaskView> _careTasks = Array.Empty<CropTaskView>();
    private IReadOnlyList<HarvestForecastView> _forecasts = Array.Empty<HarvestForecastView>();

    public IReadOnlyList<CropSummaryView> Summaries
    {
        get => _summaries;
        private set => SetProperty(ref _summaries, value);
    }

    public IReadOnlyList<CropTaskView> CareTasks
    {
        get => _careTasks;
        private set => SetProperty(ref _careTasks, value);
    }

    public IReadOnlyList<HarvestForecastView> Forecasts
    {
        get => _forecasts;
        private set => SetProperty(ref _forecasts, value);
    }

    public void Update(FarmSnapshot snapshot)
    {
        var insights = snapshot.CropInsights ?? new Dictionary<string, CropInsight>();

        Summaries = snapshot.CropStatuses
            .OrderByDescending(c => c.ReadyCount)
            .ThenBy(c => c.DaysUntilNextHarvest)
            .Select(status => CreateSummaryView(status, TryGetInsight(insights, status)))
            .ToList();

        Forecasts = snapshot.HarvestForecasts
            .OrderBy(f => f.DaysUntilReady)
            .Take(6)
            .Select(f => new HarvestForecastView(
                label: $"{f.Name} ({f.Quantity})",
                CountdownText: f.DaysUntilReady <= 0 ? "Ready today" : $"{f.DaysUntilReady} day(s) left",
                EmphasisColor: f.DaysUntilReady <= 0 ? "#2ECC40" : "#FFD700",
                ValueText: f.ExpectedValueEach.HasValue && f.ExpectedValueEach.Value > 0
                    ? $"≈{DashboardFormatting.FormatMoney(f.ExpectedValueEach.Value)} each"
                    : null))
            .ToList();

        CareTasks = snapshot.CareTasks
            .Where(t => !t.Completed)
            .Select(t => new CropTaskView(t.Label, t.Details, "#FF851B", t.Completed))
            .ToList();
    }

    private static CropSummaryView CreateSummaryView(FarmSnapshot.CropStatusEntry entry, CropInsight? insight)
    {
        string readyText = entry.ReadyCount > 0 ? $"Ready: {entry.ReadyCount}" : "No harvest ready";
        string growthText = entry.DaysUntilNextHarvest <= 0
            ? "Harvest window"
            : $"Next in {entry.DaysUntilNextHarvest} day(s)";

        string statusColor = entry.WiltedCount > 0
            ? "#FF4136"
            : entry.ReadyCount > 0 ? "#2ECC40" : "#FFFFFF";

        string? progressText = null;
        if (insight != null && insight.DaysUntilFirstHarvest >= 0 && entry.ReadyCount == 0)
            progressText = $"First harvest in {insight.DaysUntilFirstHarvest} day(s)";
        else if (entry.DaysSincePlanted.HasValue && entry.TotalGrowthDays.HasValue && entry.TotalGrowthDays.Value > 0)
        {
            double ratio = Math.Clamp(entry.DaysSincePlanted.Value / (double)entry.TotalGrowthDays.Value, 0d, 1d);
            progressText = $"Growth {ratio * 100:0}%";
        }

        string? valueText = null;
        string? alertsText = null;
        if (insight != null)
        {
            if (insight.EstimatedValue > 0)
                valueText = $"Est. {DashboardFormatting.FormatMoney(insight.EstimatedValue)}";

            if (insight.Alerts?.Count > 0)
                alertsText = string.Join(", ", insight.Alerts);
        }

        return new CropSummaryView(
            entry.Name,
            string.IsNullOrWhiteSpace(entry.Location) ? "Farm" : entry.Location,
            readyText,
            growthText,
            statusColor,
            progressText,
            alertsText,
            valueText);
    }

    private static CropInsight? TryGetInsight(IReadOnlyDictionary<string, CropInsight> map, FarmSnapshot.CropStatusEntry status)
    {
        string key = $"{status.Name}::{status.Location}";
        return map.TryGetValue(key, out var insight) ? insight : null;
    }
}
