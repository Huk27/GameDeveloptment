namespace FarmDashboard.UI;

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

internal sealed record GoalEntry(string Name, string Description, string StatusText, string StatusColor, string ProgressText)
{
    public bool HasProgress => !string.IsNullOrWhiteSpace(ProgressText);
}

internal sealed record class CropSummaryView(string Name, string Location, string ReadyText, string GrowthText, string StatusColor, string? ProgressText, string? AlertsText, string? ValueText)
{
    public bool HasProgress => !string.IsNullOrWhiteSpace(ProgressText);
    public bool HasAlerts => !string.IsNullOrWhiteSpace(AlertsText);
    public bool HasValue => !string.IsNullOrWhiteSpace(ValueText);
}

internal sealed record class CropTaskView(string Label, string Details, string StatusColor, bool Completed)
{
    public bool IsCompleted => Completed;
}

internal sealed record class HarvestForecastView(string Label, string CountdownText, string EmphasisColor, string? ValueText)
{
    public bool HasValue => !string.IsNullOrWhiteSpace(ValueText);
}

internal sealed record class AnimalStatusView(string Name, string Type, string Building, string Mood, string HappinessText, string ProduceText, string MoodColor);

internal sealed record class AnimalCareTaskView(string Label, string Details, string StatusColor);

internal sealed record class ActivitySummaryView(string TodayPlaytime, string GoldPerHour, string Recommendation)
{
    public bool HasRecommendation => !string.IsNullOrWhiteSpace(Recommendation);
}

internal sealed record class ActivitySuggestionView(string Title, string Message, string Category);

internal sealed record class CustomGoalView(string Name, string Description, string StatusText, string AccentColor);
