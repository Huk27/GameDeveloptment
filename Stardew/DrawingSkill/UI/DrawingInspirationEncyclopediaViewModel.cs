using StardewUI.ViewModels;
using DrawingActivityMod.Systems;
using System.Collections.Generic;
using System.Linq;

namespace DrawingActivityMod.UI
{
    public class DrawingInspirationEncyclopediaViewModel : ViewModel
    {
        private readonly DrawingInspirationEncyclopedia encyclopedia;
        private List<DrawingInspirationEncyclopedia.InspirationEntry> allInspirations;
        private int currentPage = 0;
        private const int itemsPerPage = 8;

        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        private string _pageInfo;
        public string PageInfo
        {
            get => _pageInfo;
            set => SetProperty(ref _pageInfo, value);
        }

        private bool _hasPreviousPage;
        public bool HasPreviousPage
        {
            get => _hasPreviousPage;
            set => SetProperty(ref _hasPreviousPage, value);
        }

        private bool _hasNextPage;
        public bool HasNextPage
        {
            get => _hasNextPage;
            set => SetProperty(ref _hasNextPage, value);
        }

        private List<InspirationItemViewModel> _inspirationItems;
        public List<InspirationItemViewModel> InspirationItems
        {
            get => _inspirationItems;
            set => SetProperty(ref _inspirationItems, value);
        }

        public DrawingInspirationEncyclopediaViewModel(DrawingInspirationEncyclopedia encyclopedia)
        {
            this.encyclopedia = encyclopedia;
            this.allInspirations = encyclopedia.GetAllInspirations();
            
            UpdateData();
        }

        public void UpdateData()
        {
            // 상태 텍스트 업데이트
            var unlockedCount = encyclopedia.GetUnlockedInspirations().Count;
            var totalCount = allInspirations.Count;
            StatusText = ModEntry.Instance.Helper.Translation.Get("ui.workbench.inspiration_status", 
                new { unlocked = unlockedCount, total = totalCount });

            // 페이지 정보 업데이트
            var totalPages = GetTotalPages();
            PageInfo = ModEntry.Instance.Helper.Translation.Get("ui.encyclopedia.page_info", 
                new { current = currentPage + 1, total = totalPages });

            // 페이지네이션 버튼 상태 업데이트
            HasPreviousPage = currentPage > 0;
            HasNextPage = currentPage < totalPages - 1;

            // 현재 페이지의 영감 아이템들 업데이트
            var startIndex = currentPage * itemsPerPage;
            var endIndex = Math.Min(startIndex + itemsPerPage, allInspirations.Count);
            var currentPageItems = allInspirations.Skip(startIndex).Take(itemsPerPage);

            InspirationItems = currentPageItems.Select(inspiration => 
                new InspirationItemViewModel(inspiration, encyclopedia.IsUnlocked(inspiration.Id))).ToList();
        }

        private int GetTotalPages()
        {
            return (int)Math.Ceiling((double)allInspirations.Count / itemsPerPage);
        }

        public void PreviousPage()
        {
            if (HasPreviousPage)
            {
                currentPage--;
                UpdateData();
            }
        }

        public void NextPage()
        {
            if (HasNextPage)
            {
                currentPage++;
                UpdateData();
            }
        }

        public void Close()
        {
            Game1.activeClickableMenu = null;
        }
    }

    public class InspirationItemViewModel : ViewModel
    {
        private readonly DrawingInspirationEncyclopedia.InspirationEntry inspiration;
        private readonly bool isUnlocked;

        public string Name => inspiration.Name;
        public string Description => inspiration.Description;
        public string Status { get; private set; }
        public bool CanCreate => isUnlocked;

        public InspirationItemViewModel(DrawingInspirationEncyclopedia.InspirationEntry inspiration, bool isUnlocked)
        {
            this.inspiration = inspiration;
            this.isUnlocked = isUnlocked;
            
            Status = isUnlocked ? 
                ModEntry.Instance.Helper.Translation.Get("ui.encyclopedia.unlocked") : 
                ModEntry.Instance.Helper.Translation.Get("ui.encyclopedia.locked");
        }

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
