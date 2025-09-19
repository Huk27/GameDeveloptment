using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FarmDashboard.Data;
using FarmDashboard.UI.Tabs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace FarmDashboard.UI;

internal sealed class DashboardViewModel : ObservableObject
{
    private const string OverviewTab = "overview";
    private const string CropsTab = "crops";
    private const string AnimalsTab = "animals";
    private const string TimeTab = "time";
    private const string GoalsTab = "goals";

    private readonly ObservableCollection<DashboardTab> _tabs;
    private readonly ReadOnlyObservableCollection<DashboardTab> _readonlyTabs;

    private string _activeTab;
    private string _lastUpdatedText = string.Empty;

    public DashboardViewModel()
    {
        _tabs = new ObservableCollection<DashboardTab>
        {
            CreateTab(OverviewTab, "Overview", new Rectangle(20, 388, 8, 8), active: true),
            CreateTab(CropsTab, "Crops", new Rectangle(4, 372, 8, 8)),
            CreateTab(AnimalsTab, "Animals", new Rectangle(4, 388, 8, 8)),
            CreateTab(TimeTab, "Time & Activity", new Rectangle(36, 374, 7, 8)),
            CreateTab(GoalsTab, "Goals", new Rectangle(420, 1204, 8, 8))
        };
        _readonlyTabs = new ReadOnlyObservableCollection<DashboardTab>(_tabs);

        _activeTab = OverviewTab;

        Overview = new OverviewTabViewModel();
        Crops = new CropsTabViewModel();
        Animals = new AnimalsTabViewModel();
        TimeActivity = new TimeActivityTabViewModel();
        Goals = new GoalsTabViewModel();
    }

    public IReadOnlyList<DashboardTab> Tabs => _readonlyTabs;

    public OverviewTabViewModel Overview { get; }

    public CropsTabViewModel Crops { get; }

    public AnimalsTabViewModel Animals { get; }

    public TimeActivityTabViewModel TimeActivity { get; }

    public GoalsTabViewModel Goals { get; }

    public string LastUpdatedText
    {
        get => _lastUpdatedText;
        private set => SetProperty(ref _lastUpdatedText, value);
    }

    public bool ShowOverviewTab => IsActive(OverviewTab);

    public bool ShowCropsTab => IsActive(CropsTab);

    public bool ShowAnimalsTab => IsActive(AnimalsTab);

    public bool ShowTimeActivityTab => IsActive(TimeTab);

    public bool ShowGoalsTab => IsActive(GoalsTab);

    // Backwards compatibility alias for docs referencing older naming.
    public bool ShowActivityTab => ShowTimeActivityTab;

    public void OnTabActivated(string name)
    {
        SetActiveTab(name);
    }

    public void ResetToDefaultTab()
    {
        SetActiveTab(OverviewTab);
    }

    public bool TryActivateTab(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        SetActiveTab(name);
        return true;
    }

    public void UpdateFromSnapshot(FarmSnapshot snapshot, bool activeTabOnly = false)
    {
        if (!activeTabOnly || ShowOverviewTab)
            Overview.Update(snapshot);

        if (!activeTabOnly || ShowCropsTab)
            Crops.Update(snapshot);

        if (!activeTabOnly || ShowAnimalsTab)
            Animals.Update(snapshot);

        if (!activeTabOnly || ShowTimeActivityTab)
            TimeActivity.Update(snapshot);

        if (!activeTabOnly || ShowGoalsTab)
            Goals.Update(snapshot);

        LastUpdatedText = $"Last updated: {snapshot.LastUpdated.ToLocalTime():t}";
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
            tab.Active = string.Equals(tab.Name, normalized, StringComparison.OrdinalIgnoreCase);

        RaisePropertyChanged(nameof(ShowOverviewTab));
        RaisePropertyChanged(nameof(ShowCropsTab));
        RaisePropertyChanged(nameof(ShowAnimalsTab));
        RaisePropertyChanged(nameof(ShowTimeActivityTab));
        RaisePropertyChanged(nameof(ShowGoalsTab));
        RaisePropertyChanged(nameof(ShowActivityTab));
    }

    private bool IsActive(string tabName)
    {
        return string.Equals(_activeTab, tabName, StringComparison.OrdinalIgnoreCase);
    }

    private static DashboardTab CreateTab(string name, string displayName, Rectangle sourceRect, bool active = false)
    {
        var texture = Game1.mouseCursors;
        return new DashboardTab(name, displayName, Tuple.Create((Texture2D)texture, sourceRect))
        {
            Active = active
        };
    }
}

internal sealed class DashboardTab : ObservableObject
{
    private bool _active;

    public DashboardTab(string name, string displayName, Tuple<Texture2D, Rectangle> sprite)
    {
        Name = name;
        DisplayName = displayName;
        Sprite = sprite;
    }

    public string Name { get; }

    public string DisplayName { get; }

    public Tuple<Texture2D, Rectangle> Sprite { get; }

    public bool Active
    {
        get => _active;
        set => SetProperty(ref _active, value);
    }
}
