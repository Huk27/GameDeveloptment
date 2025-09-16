using StardewUI.ViewModels;
using DrawingActivityMod.Systems;

namespace DrawingActivityMod.UI
{
    public class DrawingWorkbenchViewModel : ViewModel
    {
        private readonly DrawingToolManager toolManager;
        private readonly DrawingInspirationSystem inspirationSystem;
        private readonly DrawingDailyActivities dailyActivities;

        private string _inspirationStatus;
        public string InspirationStatus
        {
            get => _inspirationStatus;
            set => SetProperty(ref _inspirationStatus, value);
        }

        private string _toolStatus;
        public string ToolStatus
        {
            get => _toolStatus;
            set => SetProperty(ref _toolStatus, value);
        }

        private bool _canCreateArtwork;
        public bool CanCreateArtwork
        {
            get => _canCreateArtwork;
            set => SetProperty(ref _canCreateArtwork, value);
        }

        private string _recentActivities;
        public string RecentActivities
        {
            get => _recentActivities;
            set => SetProperty(ref _recentActivities, value);
        }

        public DrawingWorkbenchViewModel(DrawingToolManager toolManager, 
                                       DrawingInspirationSystem inspirationSystem,
                                       DrawingDailyActivities dailyActivities)
        {
            this.toolManager = toolManager;
            this.inspirationSystem = inspirationSystem;
            this.dailyActivities = dailyActivities;
            
            UpdateData();
        }

        public void UpdateData()
        {
            // 영감 상태 업데이트
            var unlockedCount = inspirationSystem.GetUnlockedInspirations().Count;
            var totalCount = inspirationSystem.GetAllInspirations().Count;
            InspirationStatus = ModEntry.Instance.Helper.Translation.Get(
                "ui.workbench.inspiration_status", 
                new { unlocked = unlockedCount, total = totalCount });

            // 도구 상태 업데이트
            var brushAcquired = toolManager.HasBrush();
            ToolStatus = ModEntry.Instance.Helper.Translation.Get(
                "ui.workbench.tool_status", 
                new { has_brush = brushAcquired });

            // 작품 제작 가능 여부
            CanCreateArtwork = brushAcquired && unlockedCount > 0;

            // 최근 활동 업데이트
            var activities = dailyActivities.GetRecentActivities(5);
            RecentActivities = string.Join("\n", activities.Select(a => 
                ModEntry.Instance.Helper.Translation.Get("ui.workbench.activity_format", 
                    new { activity = a.Name, time = a.Time })));
        }

        public void OpenEncyclopedia()
        {
            var encyclopedia = inspirationSystem.GetEncyclopedia();
            var viewModel = new DrawingInspirationEncyclopediaViewModel(encyclopedia);
            var ui = new StardewUI.Menus.SomeMenu("jinhyy.DrawingActivity/UI/DrawingInspirationEncyclopedia", viewModel);
            Game1.activeClickableMenu = ui;
        }

        public void CreateArtwork()
        {
            if (CanCreateArtwork)
            {
                // 작품 제작 로직
                ModEntry.Instance.Monitor.Log("작품 제작 시작", LogLevel.Info);
            }
        }

        public void OpenDailyActivities()
        {
            // 일상 활동 메뉴 열기
            ModEntry.Instance.Monitor.Log("일상 활동 메뉴 열기", LogLevel.Info);
        }
    }
}
