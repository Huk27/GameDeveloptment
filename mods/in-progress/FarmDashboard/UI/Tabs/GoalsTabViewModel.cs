using System;
using System.Collections.Generic;
using System.Linq;
using FarmDashboard.Data;
using FarmDashboard.UI;

namespace FarmDashboard.UI.Tabs;

internal sealed class GoalsTabViewModel : ObservableObject
{
    private IReadOnlyList<GoalEntry> _goals = Array.Empty<GoalEntry>();
    private IReadOnlyList<CustomGoalView> _customGoals = Array.Empty<CustomGoalView>();

    public IReadOnlyList<GoalEntry> Goals
    {
        get => _goals;
        private set => SetProperty(ref _goals, value);
    }

    public IReadOnlyList<CustomGoalView> CustomGoals
    {
        get => _customGoals;
        private set => SetProperty(ref _customGoals, value);
    }

    public void Update(FarmSnapshot snapshot)
    {
        Goals = snapshot.Goals
            .OrderBy(g => g.Status == GoalStatus.Completed ? 1 : 0)
            .ThenByDescending(g => g.Percentage)
            .Select(CreateGoalEntry)
            .Take(6)
            .ToList();

        CustomGoals = snapshot.CustomGoals
            .Select(CreateCustomGoalView)
            .ToList();
    }

    private static GoalEntry CreateGoalEntry(FarmSnapshot.GoalProgress goal)
    {
        string statusColor = goal.Status switch
        {
            GoalStatus.Completed => "#2ECC40",
            GoalStatus.Expired => "#FF4136",
            _ => "#FFD700"
        };

        string statusText = goal.Status switch
        {
            GoalStatus.Completed => "Completed",
            GoalStatus.Expired => "Expired",
            GoalStatus.Paused => "Paused",
            _ => "Active"
        };

        string progressText = goal.TargetValue <= 0
            ? string.Empty
            : $"{goal.CurrentValue:N0} / {goal.TargetValue:N0} ({goal.Percentage:F0}%)";

        return new GoalEntry(goal.Name, goal.Description, statusText, statusColor, progressText);
    }

    private static CustomGoalView CreateCustomGoalView(FarmSnapshot.CustomGoalEntry goal)
    {
        float clamped = Math.Clamp(goal.Progress, 0f, 1f);
        string statusText = clamped <= 0f
            ? "Not started"
            : clamped >= 1f ? "Completed" : $"{clamped:P0}";

        string accent = clamped >= 1f ? "#2ECC40" : "#FFFFFF";

        return new CustomGoalView(goal.Name, goal.Description, statusText, accent);
    }
}
