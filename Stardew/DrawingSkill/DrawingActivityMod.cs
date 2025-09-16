using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.UI;
using StardewValley;
using StardewValley.Menus;
using SpaceCore.Skills;
using System;
using System.Collections.Generic;

namespace DrawingActivityMod
{
    public class DrawingActivityMod : Mod
    {
        public static DrawingActivityMod Instance { get; private set; }
        
        private bool isDrawingCrafting = false;
        private string lastCraftedItem = "";
        private DrawingInspirationSystem inspirationSystem;
        private DrawingDailyActivities dailyActivities;
        private DrawingToolManager toolManager;
        
        public override void Entry(IModHelper helper)
        {
            Instance = this; // Instance 설정
            
            // 도구 관리 시스템 초기화
            toolManager = new DrawingToolManager(this.Helper, this.Monitor);
            
            // 영감 시스템 초기화
            inspirationSystem = new DrawingInspirationSystem(this.Helper, this.Monitor);
            inspirationSystem.Initialize();
            
            // 일상생활 활동 시스템 초기화
            dailyActivities = new DrawingDailyActivities(this.Helper, this.Monitor, inspirationSystem);
            
            // 이벤트 등록
            this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            this.Helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            this.Helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            try
            {
                // SpaceCore 스킬 등록
                Skills.RegisterSkill(new DrawingSkill());
                this.Monitor.Log(this.Helper.Translation.Get("messages.skill_registered"), LogLevel.Info);
                
                // 도구 관리 시스템 초기화
                toolManager.Initialize();
                this.Monitor.Log(this.Helper.Translation.Get("messages.tool_manager_initialized"), LogLevel.Info);
                
                // 영감 시스템 초기화
                inspirationSystem.Initialize();
                this.Monitor.Log(this.Helper.Translation.Get("messages.inspiration_system_initialized"), LogLevel.Info);
                
                // 일상생활 활동 시스템 초기화
                dailyActivities.Initialize();
                this.Monitor.Log(this.Helper.Translation.Get("messages.daily_activities_initialized"), LogLevel.Info);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Error registering drawing skill: {ex.Message}", LogLevel.Error);
            }
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // 도구 획득 조건 확인
            toolManager.CheckToolAcquisition();
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // 제작 메뉴에서 그림 작품 제작 시 경험치 부여
            if (e.Button == SButton.MouseLeft && Game1.activeClickableMenu is CraftingPage)
            {
                CheckDrawingCrafting();
            }
            
            // 그림작업대 사용
            if (e.Button == SButton.MouseLeft && Game1.player.CurrentItem?.Name == "그림작업대")
            {
                OpenDrawingWorkbenchMenu();
            }

            // 영감 도감 열기 (I 키)
            if (e.Button == SButton.I && Game1.activeClickableMenu == null)
            {
                OpenInspirationEncyclopediaMenu();
            }
        }

        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            // 제작 완료 감지
            if (Game1.activeClickableMenu is CraftingPage craftingPage)
            {
                CheckDrawingCrafting();
            }
        }

        private void CheckDrawingCrafting()
        {
            if (isDrawingCrafting) return;

            var player = Game1.player;
            var currentItem = player.CurrentItem;

            if (currentItem != null && IsDrawingItem(currentItem.Name))
            {
                isDrawingCrafting = true;
                lastCraftedItem = currentItem.Name;
                
                // 그림 스킬 경험치 부여
                int baseExp = 15;
                int currentLevel = DrawingSkill.GetDrawingLevel(player);
                int bonusExp = currentLevel * 2;
                int totalExp = baseExp + bonusExp;

                DrawingSkill.AddDrawingExperience(player, totalExp);
                
                // 경험치 메시지 표시
                Game1.addHUDMessage(new HUDMessage($"그림 경험치 +{totalExp} 획득!", HUDMessage.achievement_type));
                
                this.Monitor.Log($"그림 작품 '{currentItem.Name}' 제작 완료! (+{totalExp} 경험치)", LogLevel.Info);
            }
        }

        private bool IsDrawingItem(string itemName)
        {
            // 그림 작품인지 확인
            string[] drawingItems = {
                "풍경화", "인물화", "정물화", "마법 풍경화", "감정 표현화", "계절의 정수",
                "스타듀 밸리의 영혼", "동물 조각", "식물 조각", "도구 조각", "움직이는 조각",
                "감정 조각", "원소 조각", "농장의 수호신", "계절의 멜로디", "작업의 리듬",
                "휴식의 선율", "마법의 교향곡", "감정의 소나타", "자연의 합창", "스타듀 밸리 교향곡",
                "봄의 씨앗", "황금 밀밭", "수확의 기쁨", "우유 한 방울", "동물과의 유대",
                "우정의 증명", "결혼식", "아이의 첫 걸음", "숲의 비밀", "계절의 선물",
                "물고기의 춤", "깊은 바다", "광산의 깊이", "땅속의 보물", "무지개의 아름다움",
                "별똥별의 소원", "생일의 기쁨", "축제의 기쁨"
            };

            foreach (string item in drawingItems)
            {
                if (itemName.Contains(item))
                    return true;
            }
            return false;
        }

        private void OnDrawingCraftingComplete()
        {
            isDrawingCrafting = false;
            lastCraftedItem = "";
        }

        private void ApplyDrawingEffects()
        {
            var player = Game1.player;
            int drawingLevel = DrawingSkill.GetDrawingLevel(player);

            // 그림 스킬 레벨에 따른 효과
            switch (drawingLevel)
            {
                case 1:
                    this.Monitor.Log("그림 스킬 레벨 1: 기본적인 그림 그리기 가능!", LogLevel.Info);
                    break;
                case 2:
                    this.Monitor.Log("그림 스킬 레벨 2: 더 정교한 그림 그리기 가능!", LogLevel.Info);
                    break;
                case 3:
                    this.Monitor.Log("그림 스킬 레벨 3: 색상 사용 기술 향상!", LogLevel.Info);
                    break;
                case 4:
                    this.Monitor.Log("그림 스킬 레벨 4: 그림자와 명암 표현 가능!", LogLevel.Info);
                    break;
                case 5:
                    this.Monitor.Log("그림 스킬 레벨 5: 전문가 수준의 그림 그리기 가능!", LogLevel.Info);
                    break;
                case 6:
                    this.Monitor.Log("그림 스킬 레벨 6: 마법적인 효과가 있는 그림 그리기 가능!", LogLevel.Info);
                    break;
                case 7:
                    this.Monitor.Log("그림 스킬 레벨 7: 생동감 있는 그림 그리기 가능!", LogLevel.Info);
                    break;
                case 8:
                    this.Monitor.Log("그림 스킬 레벨 8: 감정을 담은 그림 그리기 가능!", LogLevel.Info);
                    break;
                case 9:
                    this.Monitor.Log("그림 스킬 레벨 9: 전설적인 그림 그리기 가능!", LogLevel.Info);
                    break;
                case 10:
                    this.Monitor.Log("그림 스킬 레벨 10: 마스터 수준의 그림 그리기 가능!", LogLevel.Info);
                    break;
            }

            // 특별한 효과들
            if (DrawingSkill.HasProfession(player, "DrawingMaster"))
            {
                this.Monitor.Log("그림의 달인 효과: 그림 제작 시 25% 추가 경험치!", LogLevel.Info);
            }

            if (DrawingSkill.HasProfession(player, "DrawingInspiration"))
            {
                this.Monitor.Log("예술적 영감 효과: 영감을 더 자주 얻을 수 있습니다!", LogLevel.Info);
            }

            // 특별한 작품 효과
            if (lastCraftedItem == "작물 성장의 멜로디")
            {
                this.Monitor.Log("작물 성장의 멜로디 효과: 작물 성장이 가속됩니다!", LogLevel.Info);
                break;
            }
        }
        
        private void OpenDrawingWorkbenchMenu()
        {
            var viewModel = new DrawingActivityMod.UI.DrawingWorkbenchViewModel(toolManager, inspirationSystem);
            var ui = new StardewUI.Menus.SomeMenu("jinhyy.DrawingActivity/UI/DrawingWorkbench", viewModel);
            Game1.activeClickableMenu = ui;
        }

        private void OpenInspirationEncyclopediaMenu()
        {
            var encyclopedia = inspirationSystem.GetEncyclopedia();
            var viewModel = new DrawingActivityMod.UI.DrawingInspirationEncyclopediaViewModel(encyclopedia);
            var ui = new StardewUI.Menus.SomeMenu("jinhyy.DrawingActivity/UI/DrawingInspirationEncyclopedia", viewModel);
            Game1.activeClickableMenu = ui;
        }
    }
}
