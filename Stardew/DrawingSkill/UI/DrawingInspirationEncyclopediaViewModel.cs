using DrawingActivityMod;
using StardewValley;
using StardewModdingAPI;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DrawingActivityMod.UI
{
    public record DrawingInspirationEncyclopediaViewModel(
        string StatusText,
        string PageInfo,
        bool HasPreviousPage,
        bool HasNextPage,
        List<InspirationItemViewModel> InspirationItems
    )
    {
        public static DrawingInspirationEncyclopediaViewModel LoadFromEncyclopedia(DrawingInspirationEncyclopedia encyclopedia)
        {
            var allInspirations = encyclopedia.GetAllInspirations();
            var currentPage = 0;
            const int itemsPerPage = 8;
            
            // 상태 텍스트 계산
            var unlockedCount = encyclopedia.GetUnlockedInspirations().Count;
            var totalCount = allInspirations.Count;
            var statusText = ModEntry.Instance.Helper.Translation.Get("ui.workbench.inspiration_status", 
                new { unlocked = unlockedCount, total = totalCount });

            // 페이지 정보 계산
            var totalPages = (int)Math.Ceiling((double)totalCount / itemsPerPage);
            var pageInfo = ModEntry.Instance.Helper.Translation.Get("ui.encyclopedia.page_info", 
                new { current = currentPage + 1, total = totalPages });

            // 페이지네이션 버튼 상태
            var hasPreviousPage = currentPage > 0;
            var hasNextPage = currentPage < totalPages - 1;

            // 현재 페이지의 영감 아이템들
            var startIndex = currentPage * itemsPerPage;
            var endIndex = Math.Min(startIndex + itemsPerPage, allInspirations.Count);
            var inspirationItems = new List<InspirationItemViewModel>();
            
            for (int i = startIndex; i < endIndex; i++)
            {
                var inspiration = allInspirations[i];
                inspirationItems.Add(new InspirationItemViewModel(
                    inspiration.Name,
                    inspiration.Description,
                    inspiration.IsUnlocked,
                    inspiration.UnlockCondition
                ));
            }
            
            return new DrawingInspirationEncyclopediaViewModel(
                statusText,
                pageInfo,
                hasPreviousPage,
                hasNextPage,
                inspirationItems
            );
        }
    }

    public record InspirationItemViewModel(
        string Name,
        string Description,
        bool IsUnlocked,
        string UnlockCondition
    )
    {
        public string Status => IsUnlocked ? 
            ModEntry.Instance.Helper.Translation.Get("ui.encyclopedia.unlocked") : 
            ModEntry.Instance.Helper.Translation.Get("ui.encyclopedia.locked");

        public bool CanCreate => IsUnlocked;

        public void CreateArtwork()
        {
            if (CanCreate)
            {
                // 작품 제작 로직
                ModEntry.Instance.Monitor.Log($"작품 제작: {Name}", LogLevel.Info);
            }
        }
    }
}