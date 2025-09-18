using DrawingActivityMod;
using StardewValley;
using StardewModdingAPI;
using System.Linq;

namespace DrawingActivityMod.UI
{
    public record DrawingWorkbenchViewModel(
        string InspirationStatus,
        string ToolStatus,
        bool CanCreateArtwork,
        string RecentActivities
    )
    {
        public static DrawingWorkbenchViewModel LoadFromSystems(
            DrawingToolManager toolManager, 
            DrawingInspirationSystem inspirationSystem,
            DrawingDailyActivities dailyActivities)
        {
            // 영감 상태 계산
            var unlockedCount = inspirationSystem.GetUnlockedInspirations().Count;
            var totalCount = inspirationSystem.GetAllInspirations().Count;
            var inspirationStatus = ModEntry.Instance.Helper.Translation.Get(
                "ui.workbench.inspiration_status", 
                new { unlocked = unlockedCount, total = totalCount });

            // 도구 상태 계산
            var brushAcquired = toolManager.HasBrush();
            var toolStatus = ModEntry.Instance.Helper.Translation.Get(
                "ui.workbench.tool_status", 
                new { has_brush = brushAcquired });

            // 작품 제작 가능 여부
            var canCreateArtwork = brushAcquired && unlockedCount > 0;

            // 최근 활동 계산
            var activities = dailyActivities.GetRecentActivities(5);
            var recentActivities = string.Join("\n", activities.Select(a => 
                ModEntry.Instance.Helper.Translation.Get("ui.workbench.activity_format", 
                    new { activity = a.Name, time = a.Time })));

            return new DrawingWorkbenchViewModel(
                inspirationStatus,
                toolStatus,
                canCreateArtwork,
                recentActivities
            );
        }

        public void OpenEncyclopedia()
        {
            // 영감 백과사전 열기 로직
            ModEntry.Instance.Monitor.Log("영감 백과사전 열기", LogLevel.Info);
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