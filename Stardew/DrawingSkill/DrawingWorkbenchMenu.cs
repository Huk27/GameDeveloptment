using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.UI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DrawingActivityMod
{
    public class DrawingWorkbenchMenu : FrameworkMenu
    {
        private DrawingToolManager toolManager;
        private DrawingInspirationSystem inspirationSystem;
        private LocalizationManager localization;
        private DrawingInspirationEncyclopedia encyclopedia;
        private string hoverText = "";

        public DrawingWorkbenchMenu(DrawingToolManager toolManager, DrawingInspirationSystem inspirationSystem, LocalizationManager localization)
        {
            this.toolManager = toolManager;
            this.inspirationSystem = inspirationSystem;
            this.localization = localization;
            this.encyclopedia = inspirationSystem.GetEncyclopedia();

            // StardewUI 메뉴 초기화
            InitializeMenu();
        }

        private void InitializeMenu()
        {
            // 메뉴 제목
            Title = "그림작업대";

            // 버튼들 추가
            AddButton("영감 도감", "해금된 영감을 확인하고 특정 작품을 제작할 수 있습니다.", () => OpenInspirationEncyclopedia());
            AddButton("그림 작품 제작", "해금된 영감 중에서 랜덤으로 작품을 제작합니다.", () => CreateRandomDrawing());
            AddButton("도구 상태", "현재 보유한 그림 도구를 확인합니다.", () => ShowToolStatus());
            AddButton("닫기", "메뉴를 닫습니다.", () => CloseMenu());

            // 영감 상태 표시
            UpdateInspirationStatus();
        }

        private void OpenInspirationEncyclopedia()
        {
            var encyclopediaMenu = new DrawingInspirationEncyclopediaMenu(encyclopedia, localization);
            Game1.activeClickableMenu = encyclopediaMenu;
        }

        private void CreateRandomDrawing()
        {
            // 해금된 영감 중에서 랜덤으로 선택
            var unlockedInspirations = encyclopedia.GetUnlockedInspirations();
            if (unlockedInspirations.Count == 0)
            {
                ShowMessage("해금된 영감이 없습니다!", MessageType.Error);
                return;
            }

            // 랜덤 선택
            var random = new Random();
            var randomInspiration = unlockedInspirations[random.Next(unlockedInspirations.Count)];
            
            // 작품 제작
            inspirationSystem.CreateArtwork(randomInspiration.ItemId);
            
            // 성공 메시지
            string itemName = GetItemName(randomInspiration.ItemId);
            ShowMessage($"{itemName} 작품을 제작했습니다!", MessageType.Success);
        }

        private void ShowToolStatus()
        {
            var toolStatus = new List<string>();
            toolStatus.Add("=== 보유한 그림 도구 ===");
            
            if (toolManager.HasTool("brush"))
                toolStatus.Add("✅ 붓 (Abigail과의 관계로 획득)");
            else
                toolStatus.Add("❌ 붓 (Abigail 2하트 필요)");
                
            if (toolManager.HasTool("pencil"))
                toolStatus.Add("✅ 연필 (Elliott과의 관계로 획득)");
            else
                toolStatus.Add("❌ 연필 (Elliott 2하트 필요)");
                
            if (toolManager.HasTool("paint"))
                toolStatus.Add("✅ 물감 (Leah과의 관계로 획득)");
            else
                toolStatus.Add("❌ 물감 (Leah 2하트 필요)");
                
            if (toolManager.HasTool("advanced"))
                toolStatus.Add("✅ 고급 그림 도구 (Robin과의 관계로 획득)");
            else
                toolStatus.Add("❌ 고급 그림 도구 (Robin 3하트 필요)");

            // 해금된 영감 수 표시
            var unlockedCount = encyclopedia.GetUnlockedInspirations().Count;
            var totalCount = encyclopedia.GetAllInspirations().Count;
            toolStatus.Add($"");
            toolStatus.Add($"=== 영감 상태 ===");
            toolStatus.Add($"해금된 영감: {unlockedCount}/{totalCount}");

            // 영감 보너스 표시
            float bonus = inspirationSystem.GetInspirationBonus();
            if (bonus > 1.0f)
            {
                toolStatus.Add($"영감 보너스: +{((bonus - 1.0f) * 100):F0}%");
            }

            ShowMessage(string.Join("\n", toolStatus), MessageType.Info);
        }

        private void CloseMenu()
        {
            exitThisMenu();
        }

        private void UpdateInspirationStatus()
        {
            // 영감 상태를 실시간으로 업데이트
            var unlockedCount = encyclopedia.GetUnlockedInspirations().Count;
            var totalCount = encyclopedia.GetAllInspirations().Count;
            
            // 상태 표시를 위한 서브타이틀 업데이트
            Subtitle = $"해금된 영감: {unlockedCount}/{totalCount}";
        }

        private string GetItemName(string itemId)
        {
            // 간단한 아이템 이름 매핑
            switch (itemId)
            {
                case "1004": return "봄의 씨앗";
                case "1005": return "황금 밀밭";
                case "1006": return "수확의 기쁨";
                case "1007": return "우유 한 방울";
                case "1008": return "동물과의 유대";
                case "1009": return "우정의 증명";
                case "1010": return "결혼식";
                case "1011": return "아이의 첫 걸음";
                case "1012": return "숲의 비밀";
                case "1013": return "계절의 선물";
                case "1014": return "물고기의 춤";
                case "1015": return "깊은 바다";
                case "1016": return "광산의 깊이";
                case "1017": return "땅속의 보물";
                case "1018": return "무지개의 아름다움";
                case "1019": return "별똥별의 소원";
                case "1020": return "생일의 기쁨";
                case "1021": return "축제의 기쁨";
                default: return "알 수 없는 작품";
            }
        }

        public override void draw(SpriteBatch b)
        {
            // StardewUI의 기본 그리기 호출
            base.draw(b);
            
            // 추가적인 커스텀 그리기 (필요시)
            UpdateInspirationStatus();
        }
    }
}