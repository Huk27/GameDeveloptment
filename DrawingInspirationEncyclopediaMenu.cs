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
    public class DrawingInspirationEncyclopediaMenu : IClickableMenu
    {
        private DrawingInspirationEncyclopedia encyclopedia;
        private LocalizationManager localization;
        private List<ClickableComponent> inspirationButtons = new List<ClickableComponent>();
        private string selectedInspiration = "";
        private int scrollOffset = 0;
        private const int ITEMS_PER_PAGE = 8;
        private string hoverText = "";

        // Stardew Valley 스타일 색상
        private readonly Color BACKGROUND_COLOR = new Color(0, 0, 0, 0.8f);
        private readonly Color BUTTON_COLOR = new Color(255, 255, 255, 0.9f);
        private readonly Color BUTTON_HOVER_COLOR = new Color(255, 255, 255, 1.0f);
        private readonly Color BUTTON_DISABLED_COLOR = new Color(150, 150, 150, 0.6f);
        private readonly Color TEXT_COLOR = Color.Black;
        private readonly Color TEXT_DISABLED_COLOR = Color.Gray;

        public DrawingInspirationEncyclopediaMenu(DrawingInspirationEncyclopedia encyclopedia, LocalizationManager localization)
        {
            this.encyclopedia = encyclopedia;
            this.localization = localization;

            // 메뉴 크기 설정
            width = 1000;
            height = 700;

            // 화면 중앙에 배치
            xPositionOnScreen = Game1.viewport.Width / 2 - width / 2;
            yPositionOnScreen = Game1.viewport.Height / 2 - height / 2;

            CreateInspirationButtons();
        }

        private void CreateInspirationButtons()
        {
            inspirationButtons.Clear();

            var allInspirations = encyclopedia.GetAllInspirations();
            int buttonWidth = 220;
            int buttonHeight = 70;
            int spacing = 15;
            int startX = xPositionOnScreen + 30;
            int startY = yPositionOnScreen + 100;

            for (int i = 0; i < allInspirations.Count; i++)
            {
                var inspiration = allInspirations[i];
                int row = i / 4;
                int col = i % 4;

                int x = startX + col * (buttonWidth + spacing);
                int y = startY + row * (buttonHeight + spacing);

                inspirationButtons.Add(new ClickableComponent(
                    new Rectangle(x, y, buttonWidth, buttonHeight),
                    inspiration.Key,
                    inspiration.Name
                ));
            }
        }

        public override void draw(SpriteBatch b)
        {
            // 배경
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), BACKGROUND_COLOR);

            // 메뉴 배경
            DrawMenuBackground(b);

            // 제목
            DrawTitle(b);

            // 영감 목록
            DrawInspirationList(b);

            // 선택된 영감 상세 정보
            if (!string.IsNullOrEmpty(selectedInspiration))
            {
                DrawInspirationDetails(b);
            }

            // 호버 텍스트
            if (!string.IsNullOrEmpty(hoverText))
            {
                drawHoverText(b, hoverText, Game1.smallFont);
            }

            // 마우스 커서
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
            string title = "영감 도감";
            Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
            Vector2 titlePosition = new Vector2(
                xPositionOnScreen + width / 2 - titleSize.X / 2,
                yPositionOnScreen + 30
            );

            b.DrawString(Game1.dialogueFont, title, titlePosition, Color.Black);
        }

        private void DrawInspirationList(SpriteBatch b)
        {
            var allInspirations = encyclopedia.GetAllInspirations();

            for (int i = 0; i < inspirationButtons.Count; i++)
            {
                var button = inspirationButtons[i];
                var inspiration = allInspirations[i];

                // 버튼 배경 색상 결정
                Color buttonColor;
                if (!inspiration.IsUnlocked)
                {
                    buttonColor = BUTTON_DISABLED_COLOR;
                }
                else if (button.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                {
                    buttonColor = BUTTON_HOVER_COLOR;
                }
                else
                {
                    buttonColor = BUTTON_COLOR;
                }

                // 버튼 배경
                b.Draw(Game1.menuTexture, button.bounds, buttonColor);

                // 영감 이름
                Color textColor = inspiration.IsUnlocked ? TEXT_COLOR : TEXT_DISABLED_COLOR;
                b.DrawString(Game1.smallFont, inspiration.Name,
                            new Vector2(button.bounds.X + 10, button.bounds.Y + 10),
                            textColor);

                // 영감 설명 (간단히)
                string shortDesc = inspiration.Description.Length > 25 ? 
                    inspiration.Description.Substring(0, 25) + "..." : inspiration.Description;
                b.DrawString(Game1.tinyFont, shortDesc,
                            new Vector2(button.bounds.X + 10, button.bounds.Y + 30),
                            textColor);

                // 잠금 상태
                if (!inspiration.IsUnlocked)
                {
                    b.DrawString(Game1.tinyFont, "잠금 해제 필요",
                                new Vector2(button.bounds.X + 10, button.bounds.Y + 50),
                                Color.Red);
                }
                else
                {
                    b.DrawString(Game1.tinyFont, "해금됨",
                                new Vector2(button.bounds.X + 10, button.bounds.Y + 50),
                                Color.Green);
                }

                // 영감 색상 표시
                Rectangle colorRect = new Rectangle(button.bounds.Right - 20, button.bounds.Y + 10, 15, 15);
                b.Draw(Game1.staminaRect, colorRect, inspiration.DisplayColor);
            }
        }

        private void DrawInspirationDetails(SpriteBatch b)
        {
            var inspiration = encyclopedia.GetInspiration(selectedInspiration);
            if (inspiration == null) return;

            int detailX = xPositionOnScreen + 600;
            int detailY = yPositionOnScreen + 100;
            int detailWidth = 350;
            int detailHeight = 500;

            // 상세 정보 배경
            b.Draw(Game1.menuTexture, new Rectangle(detailX, detailY, detailWidth, detailHeight),
                   new Rectangle(0, 0, 64, 64), Color.White);

            // 영감 이름
            b.DrawString(Game1.dialogueFont, inspiration.Name,
                        new Vector2(detailX + 20, detailY + 20), Color.Black);

            // 영감 설명
            string wrappedDescription = WrapText(inspiration.Description, detailWidth - 40, Game1.smallFont);
            b.DrawString(Game1.smallFont, wrappedDescription,
                        new Vector2(detailX + 20, detailY + 60), Color.Gray);

            // 해금 조건
            b.DrawString(Game1.smallFont, "해금 조건:",
                        new Vector2(detailX + 20, detailY + 120), Color.DarkBlue);
            b.DrawString(Game1.smallFont, inspiration.UnlockCondition,
                        new Vector2(detailX + 20, detailY + 140), Color.Black);


            // 경험치
            b.DrawString(Game1.smallFont, "경험치:",
                        new Vector2(detailX + 20, detailY + 180), Color.DarkGreen);
            b.DrawString(Game1.smallFont, $"{inspiration.ExpAmount}",
                        new Vector2(detailX + 20, detailY + 200), Color.Black);

            // 대응 작품 정보
            b.DrawString(Game1.smallFont, "대응 작품:",
                        new Vector2(detailX + 20, detailY + 240), Color.Purple);
            string itemName = GetItemName(inspiration.ItemId);
            b.DrawString(Game1.smallFont, itemName,
                        new Vector2(detailX + 20, detailY + 260), Color.Black);

            // 영감 색상 표시
            Rectangle colorRect = new Rectangle(detailX + 20, detailY + 300, 50, 50);
            b.Draw(Game1.staminaRect, colorRect, inspiration.DisplayColor);
            b.DrawString(Game1.smallFont, "영감 색상",
                        new Vector2(detailX + 80, detailY + 315), Color.Black);
        }

        private string WrapText(string text, int maxWidth, SpriteFont font)
        {
            string[] words = text.Split(' ');
            string result = "";
            string currentLine = "";

            foreach (string word in words)
            {
                string testLine = currentLine + (currentLine == "" ? "" : " ") + word;
                Vector2 size = font.MeasureString(testLine);

                if (size.X > maxWidth)
                {
                    if (currentLine != "")
                    {
                        result += currentLine + "\n";
                        currentLine = word;
                    }
                    else
                    {
                        result += word + "\n";
                    }
                }
                else
                {
                    currentLine = testLine;
                }
            }

            if (currentLine != "")
            {
                result += currentLine;
            }

            return result;
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
            // 영감 버튼 클릭 처리
            foreach (var button in inspirationButtons)
            {
                if (button.containsPoint(x, y))
                {
                    selectedInspiration = button.name;
                    if (playSound)
                    {
                        Game1.playSound("smallSelect");
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

        public override void performHoverAction(int x, int y)
        {
            hoverText = "";

            foreach (var button in inspirationButtons)
            {
                if (button.containsPoint(x, y))
                {
                    var inspiration = encyclopedia.GetInspiration(button.name);
                    if (inspiration != null)
                    {
                        hoverText = inspiration.IsUnlocked ? 
                            $"{inspiration.Name} - 해금됨" : 
                            $"{inspiration.Name} - 잠금 해제 필요";
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
