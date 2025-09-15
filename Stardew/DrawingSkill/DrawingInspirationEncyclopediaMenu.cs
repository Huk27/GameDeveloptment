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
    public class DrawingInspirationEncyclopediaMenu : FrameworkMenu
    {
        private DrawingInspirationEncyclopedia encyclopedia;
        private LocalizationManager localization;
        private List<DrawingInspirationEncyclopedia.InspirationEntry> allInspirations;
        private int currentPage = 0;
        private const int ITEMS_PER_PAGE = 8;

        public DrawingInspirationEncyclopediaMenu(DrawingInspirationEncyclopedia encyclopedia, LocalizationManager localization)
        {
            this.encyclopedia = encyclopedia;
            this.localization = localization;
            this.allInspirations = encyclopedia.GetAllInspirations();

            // StardewUI 메뉴 초기화
            InitializeMenu();
        }

        private void InitializeMenu()
        {
            // 메뉴 제목
            Title = "영감 도감";
            
            // 서브타이틀 업데이트
            UpdateSubtitle();

            // 버튼들 추가
            AddButton("이전 페이지", "이전 페이지로 이동합니다.", () => PreviousPage());
            AddButton("다음 페이지", "다음 페이지로 이동합니다.", () => NextPage());
            AddButton("닫기", "메뉴를 닫습니다.", () => CloseMenu());

            // 영감 목록 표시
            ShowInspirationList();
        }

        private void UpdateSubtitle()
        {
            var unlockedCount = encyclopedia.GetUnlockedInspirations().Count;
            var totalCount = allInspirations.Count;
            Subtitle = $"해금된 영감: {unlockedCount}/{totalCount}";
        }

        private void ShowInspirationList()
        {
            // 현재 페이지의 영감들 가져오기
            var startIndex = currentPage * ITEMS_PER_PAGE;
            var endIndex = Math.Min(startIndex + ITEMS_PER_PAGE, allInspirations.Count);
            var pageInspirations = allInspirations.GetRange(startIndex, endIndex - startIndex);

            // 기존 버튼들 제거 (페이지네이션 버튼 제외)
            var buttonsToRemove = new List<string>();
            foreach (var button in Buttons)
            {
                if (button.Text.StartsWith("영감:") || button.Text.StartsWith("상태:"))
                {
                    buttonsToRemove.Add(button.Text);
                }
            }
            foreach (var buttonText in buttonsToRemove)
            {
                RemoveButton(buttonText);
            }

            // 영감 버튼들 추가
            foreach (var inspiration in pageInspirations)
            {
                string status = inspiration.IsUnlocked ? "✅ 해금됨" : "❌ 미해금";
                string buttonText = $"영감: {inspiration.Name}";
                string description = $"{inspiration.Description}\n상태: {status}\n경험치: {inspiration.ExpAmount}";
                
                AddButton(buttonText, description, () => ShowInspirationDetails(inspiration));
            }
        }

        private void ShowInspirationDetails(DrawingInspirationEncyclopedia.InspirationEntry inspiration)
        {
            var details = new List<string>();
            details.Add($"=== {inspiration.Name} ===");
            details.Add($"설명: {inspiration.Description}");
            details.Add($"해금 조건: {inspiration.UnlockCondition}");
            details.Add($"경험치: {inspiration.ExpAmount}");
            details.Add($"상태: {(inspiration.IsUnlocked ? "✅ 해금됨" : "❌ 미해금")}");
            
            if (inspiration.IsUnlocked)
            {
                details.Add($"");
                details.Add("이 영감을 사용하여 작품을 제작할 수 있습니다!");
            }
            else
            {
                details.Add($"");
                details.Add("해금 조건을 만족하면 이 영감을 사용할 수 있습니다.");
            }

            ShowMessage(string.Join("\n", details), MessageType.Info);
        }

        private void PreviousPage()
        {
            if (currentPage > 0)
            {
                currentPage--;
                ShowInspirationList();
                UpdateSubtitle();
            }
        }

        private void NextPage()
        {
            int maxPages = (int)Math.Ceiling((double)allInspirations.Count / ITEMS_PER_PAGE);
            if (currentPage < maxPages - 1)
            {
                currentPage++;
                ShowInspirationList();
                UpdateSubtitle();
            }
        }

        private void CloseMenu()
        {
            exitThisMenu();
        }

        public override void draw(SpriteBatch b)
        {
            // StardewUI의 기본 그리기 호출
            base.draw(b);
            
            // 페이지 정보 표시
            int maxPages = (int)Math.Ceiling((double)allInspirations.Count / ITEMS_PER_PAGE);
            if (maxPages > 1)
            {
                string pageInfo = $"페이지 {currentPage + 1}/{maxPages}";
                Vector2 textSize = Game1.smallFont.MeasureString(pageInfo);
                Vector2 textPosition = new Vector2(
                    xPositionOnScreen + width - textSize.X - 20,
                    yPositionOnScreen + height - textSize.Y - 20
                );
                b.DrawString(Game1.smallFont, pageInfo, textPosition, Color.Black);
            }
        }
    }
}