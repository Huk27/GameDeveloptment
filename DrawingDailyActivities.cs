using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace DrawingActivityMod
{
    public class DrawingDailyActivities
    {
        private IModHelper helper;
        private IMonitor monitor;
        private DrawingInspirationSystem inspirationSystem;
        private LocalizationManager localization;
        
        public DrawingDailyActivities(IModHelper helper, IMonitor monitor, DrawingInspirationSystem inspirationSystem, LocalizationManager localization)
        {
            this.helper = helper;
            this.monitor = monitor;
            this.inspirationSystem = inspirationSystem;
            this.localization = localization;
        }
        
        public void Initialize()
        {
            // 이벤트 등록
            this.helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            this.helper.Events.GameLoop.TimeChanged += this.OnTimeChanged;
            this.helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
            this.helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            this.helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
        }
        
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // 하루 시작 시 예술적 활동 체크
            CheckDailyDrawingActivities();
        }
        
        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            // 특정 시간에 예술적 활동 체크
            CheckTimeBasedActivities(e.NewTime);
        }
        
        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            // 인벤토리 변화 시 예술적 활동 체크
            CheckInventoryBasedActivities(e.Added);
        }
        
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // 버튼 입력 시 예술적 활동 체크
            CheckButtonBasedActivities(e);
        }
        
        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            // 지속적인 예술적 활동 체크
            CheckContinuousActivities();
        }
        
        private void CheckDailyDrawingActivities()
        {
            // 하루 시작 시 예술적 활동
            GiveDrawingExperience(5, "새로운 하루의 시작");
        }
        
        private void CheckTimeBasedActivities(int time)
        {
            // 새벽 (6시) - 자연 관찰
            if (time == 600)
            {
                GiveDrawingExperience(3, "새벽의 자연 관찰");
            }
            
            // 오후 (14시) - 창작 시간
            if (time == 1400)
            {
                GiveDrawingExperience(2, "오후의 창작 시간");
            }
            
            // 저녁 (18시) - 일몰 감상
            if (time == 1800)
            {
                GiveDrawingExperience(3, "일몰의 아름다움 감상");
            }
            
            // 밤 (22시) - 밤하늘 관찰
            if (time == 2200)
            {
                GiveDrawingExperience(2, "밤하늘의 별 관찰");
            }
        }
        
        private void CheckInventoryBasedActivities(List<Item> addedItems)
        {
            foreach (var item in addedItems)
            {
                if (item == null) continue;
                
                // 작물 수확 - 자연의 아름다움 관찰
                if (item.Category == -75) // 작물
                {
                    GiveDrawingExperience(1, "자연의 수확물 관찰");
                    
                    // 고품질 작물 - 더 많은 경험치
                    if (item.Quality >= 2)
                    {
                        GiveDrawingExperience(2, "완벽한 작물의 아름다움");
                    }
                }
                
                // 동물 제품 - 생명의 아름다움 관찰
                if (item.Category == -6) // 동물 제품
                {
                    GiveDrawingExperience(1, "생명의 선물 관찰");
                }
                
                // 채집 아이템 - 자연의 보물 발견
                if (item.Category == -81) // 채집 아이템
                {
                    GiveDrawingExperience(1, "자연의 보물 발견");
                    
                    // 고품질 채집 - 더 많은 경험치
                    if (item.Quality >= 2)
                    {
                        GiveDrawingExperience(2, "완벽한 자연의 선물");
                    }
                }
                
                // 낚시 아이템 - 물의 아름다움 관찰
                if (item.Category == -4) // 낚시 아이템
                {
                    GiveDrawingExperience(1, "물의 아름다움 관찰");
                    
                    // 고품질 물고기 - 더 많은 경험치
                    if (item.Quality >= 2)
                    {
                        GiveDrawingExperience(2, "완벽한 물고기의 아름다움");
                    }
                }
                
                // 광물 - 땅의 아름다움 발견
                if (item.Category == -15) // 광물
                {
                    GiveDrawingExperience(1, "땅의 보물 발견");
                }
                
                // 예술 작품 제작 - 직접적인 예술 활동
                if (item.Category == -26) // 예술품 (가정)
                {
                    GiveDrawingExperience(5, "예술 작품 제작");
                }
            }
        }
        
        private void CheckButtonBasedActivities(ButtonPressedEventArgs e)
        {
            // 특정 버튼 입력 시 예술적 활동
            
            // 스페이스바 - 점프 (자유로운 움직임)
            if (e.Button == SButton.Space)
            {
                GiveDrawingExperience(0.5f, "자유로운 움직임");
            }
            
            // 마우스 클릭 - 관찰 활동
            if (e.Button == SButton.MouseLeft)
            {
                GiveDrawingExperience(0.2f, "세부사항 관찰");
            }
        }
        
        private void CheckContinuousActivities()
        {
            // 지속적인 예술적 활동
            
            // 걷기 - 주변 환경 관찰
            if (Game1.player.isMoving())
            {
                GiveDrawingExperience(0.1f, "주변 환경 관찰");
            }
            
            // 달리기 - 빠른 움직임의 아름다움
            if (Game1.player.isRunning())
            {
                GiveDrawingExperience(0.2f, "움직임의 아름다움");
            }
            
            // 농장 작업 - 자연과의 교감
            if (IsDoingFarmWork())
            {
                GiveDrawingExperience(0.3f, "자연과의 교감");
            }
            
            // NPC와 대화 - 인간관계의 아름다움
            if (IsTalkingToNPC())
            {
                GiveDrawingExperience(0.5f, "인간관계의 아름다움");
            }
        }
        
        private bool IsDoingFarmWork()
        {
            // 농장 작업 중인지 체크
            return Game1.player.UsingTool || Game1.player.isMoving();
        }
        
        private bool IsTalkingToNPC()
        {
            // NPC와 대화 중인지 체크
            return Game1.activeClickableMenu is DialogueBox;
        }
        
        private void GiveDrawingExperience(float amount, string reason)
        {
            if (amount <= 0) return;
            
            // 그림 스킬 경험치 부여
            DrawingSkill.AddDrawingExperience(Game1.player, (int)amount);
            
            // 로그 기록 (자주 발생하는 활동은 로그 생략)
            if (amount >= 1.0f)
            {
                monitor.Log($"그림 활동: {reason} (+{amount} 경험치)", LogLevel.Debug);
            }
        }
        
        // 특별한 그림 활동들
        public void OnArtworkCreated(string artworkName)
        {
            // 그림 작품 제작 시 특별한 경험치
            GiveDrawingExperience(10, $"그림 작품 '{artworkName}' 제작");
        }
        
        public void OnInspirationGained(string inspirationName)
        {
            // 영감 획득 시 특별한 경험치
            GiveDrawingExperience(5, $"영감 '{inspirationName}' 획득");
        }
        
        public void OnDrawingSkillLevelUp(int newLevel)
        {
            // 그림 스킬 레벨업 시 특별한 경험치
            GiveDrawingExperience(20, $"그림 스킬 레벨 {newLevel} 달성");
        }
        
        // 계절별 특별한 그림 활동
        public void OnSeasonChange(string newSeason)
        {
            GiveDrawingExperience(15, $"{newSeason} 계절의 아름다움 감상");
        }
        
        public void OnWeatherChange(string weather)
        {
            GiveDrawingExperience(3, $"{weather} 날씨의 아름다움 감상");
        }
        
        // 특별한 이벤트
        public void OnFestival()
        {
            GiveDrawingExperience(10, "축제의 즐거움과 아름다움");
        }
        
        public void OnBirthday()
        {
            GiveDrawingExperience(8, "생일의 특별함과 기쁨");
        }
        
        public void OnMarriage()
        {
            GiveDrawingExperience(25, "사랑의 아름다움과 완성");
        }
        
        public void OnChildBirth()
        {
            GiveDrawingExperience(20, "새로운 생명의 아름다움");
        }
    }
}