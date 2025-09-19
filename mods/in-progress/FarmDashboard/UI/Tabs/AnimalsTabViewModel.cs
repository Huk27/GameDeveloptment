using System.Collections.Generic;
using System.Linq;
using FarmDashboard.Data;
using FarmDashboard.UI;

namespace FarmDashboard.UI.Tabs;

internal sealed class AnimalsTabViewModel : ObservableObject
{
    private IReadOnlyList<AnimalStatusView> _animals = Array.Empty<AnimalStatusView>();
    private IReadOnlyList<AnimalCareTaskView> _careTasks = Array.Empty<AnimalCareTaskView>();

    public IReadOnlyList<AnimalStatusView> Animals
    {
        get => _animals;
        private set => SetProperty(ref _animals, value);
    }

    public IReadOnlyList<AnimalCareTaskView> CareTasks
    {
        get => _careTasks;
        private set => SetProperty(ref _careTasks, value);
    }

    public void Update(FarmSnapshot snapshot)
    {
        Animals = snapshot.AnimalStatuses
            .OrderByDescending(a => a.ProduceReady)
            .ThenByDescending(a => a.Happiness)
            .Select(CreateAnimalView)
            .ToList();

        CareTasks = snapshot.CareTasks
            .Where(t => !t.Completed)
            .Select(t => new AnimalCareTaskView(t.Label, t.Details, "#FF851B"))
            .ToList();
    }

    private static AnimalStatusView CreateAnimalView(FarmSnapshot.AnimalStatusEntry entry)
    {
        string moodColor = entry.ProduceReady ? "#2ECC40" : entry.Happiness < 40 ? "#FF4136" : "#FFFFFF";
        string produceText = entry.ProduceReady ? "Produce ready" : "No produce";
        string happiness = $"{entry.Happiness:F0}%";

        return new AnimalStatusView(
            entry.Name,
            entry.Type,
            entry.Building,
            string.IsNullOrWhiteSpace(entry.Mood) ? "Content" : entry.Mood,
            happiness,
            produceText,
            moodColor);
    }
}
