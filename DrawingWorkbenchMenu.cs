using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DrawingActivityMod
{
    public class DrawingWorkbenchMenu : IClickableMenu
    {
        private DrawingToolManager toolManager;
        private DrawingInspirationSystem inspirationSystem;
        private LocalizationManager localization;
        private DrawingInspirationEncyclopedia encyclopedia;
        private List<ClickableComponent> menuButtons = new List<ClickableComponent>();
        private List<ClickableComponent> itemButtons = new List<ClickableComponent>();
        private string hoverText = "";
        private int scrollOffset = 0;
        private const int ITEMS_PER_PAGE = 8;

        // Stardew Valley 스타일 색상
        private readonly Color BACKGROUND_COLOR = new Color(0, 0, 0, 0.8f);
        private readonly Color BUTTON_COLOR = new Color(255, 255, 255, 0.9f);
        private readonly Color BUTTON_HOVER_COLOR = new Color(255, 255, 255, 1.0f);
        private readonly Color BUTTON_DISABLED_COLOR = new Color(150, 150, 150, 0.6f);
        private readonly Color TEXT_COLOR = Color.Black;
        private readonly Color TEXT_DISABLED_COLOR = Color.Gray;

        public DrawingWorkbenchMenu(DrawingToolManager toolManager, DrawingInspirationSystem inspirationSystem, LocalizationManager localization)
        {
            this.toolManager = toolManager;
            this.inspirationSystem = inspirationSystem;
            this.localization = localization;
            this.encyclopedia = inspirationSystem.GetEncyclopedia();

            // 메뉴 크기 설정 (더 큰 크기)
            width = 1000;
            height = 700;

            // 화면 중앙에 배치
            xPositionOnScreen = Game1.viewport.Width / 2 - width / 2;
            yPositionOnScreen = Game1.viewport.Height / 2 - height / 2;

            // 메뉴 버튼들 생성
            CreateMenuButtons();
        }

        private void CreateMenuButtons()
        {
            int buttonWidth = 200;
            int buttonHeight = 60;
            int spacing = 20;
            int startX = xPositionOnScreen + 50;
            int startY = yPositionOnScreen + 100;

            // 영감 도감 버튼
            menuButtons.Add(new ClickableComponent(
                new Rectangle(startX, startY, buttonWidth, buttonHeight),
                "encyclopedia",
                "영감 도감"
            ));

            // 그림 작품 제작 버튼
            menuButtons.Add(new ClickableComponent(
                new Rectangle(startX + buttonWidth + spacing, startY, buttonWidth, buttonHeight),
                "create",
                "그림 작품 제작"
            ));
        }

        public override void draw(SpriteBatch b)
        {
            // 배경 그리기 (Stardew Valley 스타일)
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), BACKGROUND_COLOR);

            // 메뉴 배경 그리기 (Stardew Valley 스타일 패널)
            DrawMenuBackground(b);

            // 제목 그리기
            DrawTitle(b);

            // 메뉴 버튼들 그리기
            DrawMenuButtons(b);

            // 아이템들 그리기
            DrawItemList(b);

            // 영감 상태 표시
            DrawInspirationStatus(b);

            // 호버 텍스트 그리기
            if (!string.IsNullOrEmpty(hoverText))
            {
                drawHoverText(b, hoverText, Game1.smallFont);
            }

            // 마우스 커서 그리기
            drawMouse(b);
        }

        private void DrawMenuBackground(SpriteBatch b)
        {
            // Stardew Valley 스타일 패널
            b.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height), 
                   new Rectangle(0, 0, 64, 64), Color.White);
        }

        private void DrawTitle(SpriteBatch b)
        {
            string title = "그림작업대";
            Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
            Vector2 titlePosition = new Vector2(
                xPositionOnScreen + width / 2 - titleSize.X / 2,
                yPositionOnScreen + 30
            );

            b.DrawString(Game1.dialogueFont, title, titlePosition, Color.Black);
        }

        private void DrawMenuButtons(SpriteBatch b)
        {
            foreach (var button in menuButtons)
            {
                // 버튼 배경 색상 결정
                Color buttonColor;
                if (button.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                {
                    buttonColor = BUTTON_HOVER_COLOR;
                }
                else
                {
                    buttonColor = BUTTON_COLOR;
                }

                // 버튼 배경 그리기
                b.Draw(Game1.menuTexture, button.bounds, buttonColor);

                // 버튼 텍스트 그리기
                Vector2 textSize = Game1.smallFont.MeasureString(button.name);
                Vector2 textPosition = new Vector2(
                    button.bounds.X + button.bounds.Width / 2 - textSize.X / 2,
                    button.bounds.Y + button.bounds.Height / 2 - textSize.Y / 2
                );

                b.DrawString(Game1.smallFont, button.name, textPosition, TEXT_COLOR);
            }
        }

        private void DrawItemList(SpriteBatch b)
        {
            var availableItems = GetAvailableItems();
            if (availableItems.Count == 0) return;

            int itemWidth = 180;
            int itemHeight = 80;
            int spacing = 15;
            int startX = xPositionOnScreen + 50;
            int startY = yPositionOnScreen + 250;

            // 아이템 버튼들 생성 (필요시)
            if (itemButtons.Count != availableItems.Count)
            {
                CreateItemButtons(availableItems, startX, startY, itemWidth, itemHeight, spacing);
            }

            // 아이템들 그리기
            for (int i = 0; i < availableItems.Count && i < itemButtons.Count; i++)
            {
                var item = availableItems[i];
                var button = itemButtons[i];

                // 아이템 배경 색상 결정
                Color itemColor;
                if (button.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                {
                    itemColor = BUTTON_HOVER_COLOR;
                }
                else
                {
                    itemColor = BUTTON_COLOR;
                }

                // 아이템 배경 그리기
                b.Draw(Game1.menuTexture, button.bounds, itemColor);

                // 아이템 이름 그리기
                string itemName = GetItemName(item.ItemId);
                b.DrawString(Game1.smallFont, itemName,
                            new Vector2(button.bounds.X + 10, button.bounds.Y + 10),
                            TEXT_COLOR);

                // 영감 정보 그리기
                b.DrawString(Game1.tinyFont, $"영감: {item.Name}",
                            new Vector2(button.bounds.X + 10, button.bounds.Y + 30),
                            Color.Gray);

                // 제작 가능 여부 표시
                bool canCreate = inspirationSystem.CanCreateArtwork(item.ItemId);
                string statusText = canCreate ? "제작 가능" : "해금 필요";
                Color statusColor = canCreate ? Color.Green : Color.Red;

                b.DrawString(Game1.tinyFont, statusText,
                            new Vector2(button.bounds.X + 10, button.bounds.Y + 50),
                            statusColor);

                // 영감 색상 표시
                Rectangle colorRect = new Rectangle(button.bounds.Right - 20, button.bounds.Y + 10, 15, 15);
                b.Draw(Game1.staminaRect, colorRect, item.DisplayColor);
            }
        }

        private void CreateItemButtons(List<DrawingInspirationEncyclopedia.InspirationEntry> items, int startX, int startY, int itemWidth, int itemHeight, int spacing)
        {
            itemButtons.Clear();

            for (int i = 0; i < items.Count; i++)
            {
                int row = i / 4;
                int col = i % 4;

                int x = startX + col * (itemWidth + spacing);
                int y = startY + row * (itemHeight + spacing);

                itemButtons.Add(new ClickableComponent(
                    new Rectangle(x, y, itemWidth, itemHeight),
                    items[i].ItemId,
                    items[i].Name
                ));
            }
        }

        private List<DrawingInspirationEncyclopedia.InspirationEntry> GetAvailableItems()
        {
            var allInspirations = encyclopedia.GetAllInspirations();
            return allInspirations.Where(i => i.IsUnlocked).ToList();
        }

        private void DrawInspirationStatus(SpriteBatch b)
        {
            int statusX = xPositionOnScreen + 50;
            int statusY = yPositionOnScreen + height - 100;

            // 해금된 영감 수 표시
            var unlockedCount = encyclopedia.GetUnlockedInspirations().Count;
            var totalCount = encyclopedia.GetAllInspirations().Count;
            
            if (unlockedCount > 0)
            {
                b.DrawString(Game1.smallFont, $"✨ 해금된 영감: {unlockedCount}/{totalCount}",
                            new Vector2(statusX, statusY), Color.Green);
            }
            else
            {
                b.DrawString(Game1.smallFont, "해금된 영감: 0/{totalCount}",
                            new Vector2(statusX, statusY), Color.Gray);
            }

            // 영감 보너스 표시
            float bonus = inspirationSystem.GetInspirationBonus();
            if (bonus > 1.0f)
            {
                b.DrawString(Game1.smallFont, $"영감 보너스: +{((bonus - 1.0f) * 100):F0}%",
                            new Vector2(statusX, statusY + 25), Color.Gold);
            }
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

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            // 메뉴 버튼 클릭 처리
            foreach (var button in menuButtons)
            {
                if (button.containsPoint(x, y))
                {
                    if (button.name == "encyclopedia")
                    {
                        // 영감 도감 열기
                        var encyclopediaMenu = new DrawingInspirationEncyclopediaMenu(encyclopedia, localization);
                        Game1.activeClickableMenu = encyclopediaMenu;
                        if (playSound)
                        {
                            Game1.playSound("smallSelect");
                        }
                        return;
                    }
                    else if (button.name == "create")
                    {
                        // 그림 작품 제작 (랜덤 작품 제작)
                        CreateRandomDrawing();
                        if (playSound)
                        {
                            Game1.playSound("crafting");
                        }
                        return;
                    }
                }
            }

            // 아이템 버튼 클릭 처리
            foreach (var button in itemButtons)
            {
                if (button.containsPoint(x, y))
                {
                    CreateDrawing(item.name);
                    if (playSound)
                    {
                        Game1.playSound("crafting");
                    }
                    return;
                }
            }

            // ESC 키로 닫기
            if (x < xPositionOnScreen || x > xPositionOnScreen + width ||
                y < yPositionOnScreen || y > yPositionOnScreen + height)
            {
                exitThisMenu(playSound);
            }
        }

        private void CreateDrawing(string itemId)
        {
            // 작품 제작 (영감 소모 없음)
            inspirationSystem.CreateArtwork(itemId);
        }

        private void CreateRandomDrawing()
        {
            // 해금된 영감 중에서 랜덤으로 선택
            var unlockedInspirations = encyclopedia.GetUnlockedInspirations();
            if (unlockedInspirations.Count == 0)
            {
                Game1.addHUDMessage(new HUDMessage("해금된 영감이 없습니다!", HUDMessage.error_type));
                return;
            }

            // 랜덤 선택
            var random = new Random();
            var randomInspiration = unlockedInspirations[random.Next(unlockedInspirations.Count)];
            
            // 작품 제작
            inspirationSystem.CreateArtwork(randomInspiration.ItemId);
        }

        public override void performHoverAction(int x, int y)
        {
            hoverText = "";

            // 메뉴 버튼 호버
            foreach (var button in menuButtons)
            {
                if (button.containsPoint(x, y))
                {
                    if (button.name == "encyclopedia")
                    {
                        hoverText = "영감 도감을 열어서 해금된 영감을 확인할 수 있습니다.";
                    }
                    else if (button.name == "create")
                    {
                        hoverText = "해금된 영감 중에서 랜덤으로 그림 작품을 제작합니다.";
                    }
                    return;
                }
            }

            // 아이템 버튼 호버
            foreach (var button in itemButtons)
            {
                if (button.containsPoint(x, y))
                {
                    var inspiration = encyclopedia.GetInspirationByItemId(button.name);
                    if (inspiration != null)
                    {
                        hoverText = $"{GetItemName(button.name)} - {inspiration.Description}";
                    }
                    return;
                }
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Escape)
            {
                exitThisMenu(true);
            }
        }
    }
}
