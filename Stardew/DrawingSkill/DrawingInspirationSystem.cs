using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DrawingActivityMod
{
    public class DrawingInspirationSystem
    {
        private IModHelper helper;
        private IMonitor monitor;
        private DrawingInspirationEncyclopedia encyclopedia;
        
        public DrawingInspirationSystem(IModHelper helper, IMonitor monitor)
        {
            this.helper = helper;
            this.monitor = monitor;
            this.encyclopedia = new DrawingInspirationEncyclopedia(monitor);
        }
        
        public void Initialize()
        {
            // 이벤트 등록
            this.helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            this.helper.Events.GameLoop.TimeChanged += this.OnTimeChanged;
            this.helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
            this.helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            this.helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
            this.helper.Events.GameLoop.DayEnding += this.OnDayEnding;
        }
        
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // 모든 영감 해금 조건 체크
            CheckAllUnlockConditions();
        }
        
        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            // 시간 기반 영감 체크
            CheckTimeBasedInspiration(e.NewTime);
        }
        
        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            // 아이템 획득 시 영감 체크
            foreach (var item in e.Added)
            {
                CheckItemInspiration(item);
            }
        }
        
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // 저장 파일 로드 시 영감 상태 복원
            CheckAllUnlockConditions();
        }
        
        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            // 특별한 이벤트 체크
            CheckSpecialEvents();
        }
        
        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            // 하루 마무리 시 특별한 영감 체크
            CheckEndOfDayInspiration();
        }
        
        private void CheckAllUnlockConditions()
        {
            var allInspirations = encyclopedia.GetAllInspirations();
            
            foreach (var inspiration in allInspirations)
            {
                if (!inspiration.IsUnlocked)
                {
                    if (CheckUnlockCondition(inspiration))
                    {
                        UnlockInspiration(inspiration.Key);
                    }
                }
            }
        }
        
        private bool CheckUnlockCondition(DrawingInspirationEncyclopedia.InspirationEntry inspiration)
        {
            switch (inspiration.Key)
            {
                case "first_seed":
                    return HasPlantedFirstSeed();
                case "perfect_harvest":
                    return HasHarvestedPerfectCrop();
                case "all_crops_harvested":
                    return HasHarvestedAllCropsToday();
                case "animal_product":
                    return HasGottenFirstAnimalProduct();
                case "animal_bond":
                    return HasAnimalBond();
                case "first_heart":
                    return HasGottenFirstHeart();
                case "marriage":
                    return HasMarried();
                case "child_birth":
                    return HasChild();
                case "special_item":
                    return HasFoundSpecialItem();
                case "perfect_foraging":
                    return HasPerfectForaging();
                case "first_fish":
                    return HasCaughtFirstFish();
                case "rare_fish":
                    return HasCaughtRareFish();
                case "deep_floor":
                    return HasReachedDeepFloor();
                case "rare_mineral":
                    return HasFoundRareMineral();
                case "rainbow":
                    return HasSeenRainbow();
                case "shooting_star":
                    return HasSeenShootingStar();
                case "birthday":
                    return IsBirthday();
                case "festival":
                    return IsFestivalDay();
                default:
                    return false;
            }
        }
        
        public void UnlockInspiration(string inspirationKey)
        {
            encyclopedia.UnlockInspiration(inspirationKey);
            var inspiration = encyclopedia.GetInspiration(inspirationKey);
            
            if (inspiration != null)
            {
                // 해금 메시지
                string message = $"✨ 새로운 영감 '{inspiration.Name}'이 해금되었습니다!";
                Game1.addHUDMessage(new HUDMessage(message, HUDMessage.achievement_type));
                
                monitor.Log($"영감 '{inspiration.Name}'이 해금되었습니다!", LogLevel.Info);
            }
        }
        
        private void CheckTimeBasedInspiration(int time)
        {
            // 새벽 영감 (6시)
            if (time == 600)
            {
                TryGainInspiration("dawn_beauty", "새벽의 아름다움", 30);
            }
            
            // 일몰 영감 (18시)
            if (time == 1800)
            {
                TryGainInspiration("sunset_gold", "일몰의 황금", 35);
            }
        }
        
        private void CheckItemInspiration(Item item)
        {
            if (item == null) return;
            
            // 작물 수확 영감
            if (item.Category == -75) // 작물 카테고리
            {
                if (item.Quality >= 2) // 고품질
                {
                    TryGainInspiration("perfect_harvest", "완벽한 수확", 40);
                }
            }
            
            // 동물 제품 영감
            if (item.Category == -6) // 동물 제품 카테고리
            {
                TryGainInspiration("animal_product", "자연의 선물", 25);
            }
            
            // 채집 아이템 영감
            if (item.Category == -81) // 채집 아이템
            {
                if (item.Quality >= 2)
                {
                    TryGainInspiration("perfect_foraging", "자연의 수확", 35);
                }
            }
            
            // 낚시 아이템 영감
            if (item.Category == -4) // 낚시 아이템
            {
                if (item.Quality >= 2)
                {
                    TryGainInspiration("perfect_fishing", "물과의 조화", 50);
                }
            }
        }
        
        private void CheckSpecialEvents()
        {
            // 무지개 체크
            if (Game1.isRaining && Game1.random.NextDouble() < 0.001) // 0.1% 확률
            {
                TryGainInspiration("rainbow", "자연의 기적", 70);
            }
            
            // 별똥별 체크
            if (Game1.random.NextDouble() < 0.0005) // 0.05% 확률
            {
                TryGainInspiration("shooting_star", "소원의 순간", 90);
            }
        }
        
        private void CheckEndOfDayInspiration()
        {
            // 하루 마무리 영감
            if (HasCompletedAllFarmWork())
            {
                TryGainInspiration("perfect_day", "완벽한 하루", 60);
            }
        }
        
        private bool TryGainInspiration(string inspirationKey, string inspirationName, int expAmount)
        {
            var inspiration = encyclopedia.GetInspiration(inspirationKey);
            if (inspiration == null || !inspiration.IsUnlocked) return false;
            
            // 영감 획득
            // 경험치 부여
            DrawingSkill.AddDrawingExperience(Game1.player, expAmount);
            
            // 메시지 표시
            string message = $"✨ {inspirationName} 영감을 얻었습니다!";
            Game1.addHUDMessage(new HUDMessage(message, HUDMessage.achievement_type));
            
            monitor.Log($"영감 '{inspirationName}'을 얻었습니다! (+{expAmount} 경험치)", LogLevel.Info);
            
            return true;
        }
        
        
        // 영감 해금 조건 체크 메서드들
        private bool HasPlantedFirstSeed()
        {
            // 첫 씨앗 심기 체크 (간단한 구현)
            return Game1.player.stats.SeedsSown > 0;
        }
        
        private bool HasHarvestedPerfectCrop()
        {
            // 고품질 작물 수확 체크
            return Game1.player.stats.CropsShipped > 0; // 간단한 구현
        }
        
        private bool HasHarvestedAllCropsToday()
        {
            // 하루에 모든 작물 수확 체크 (간단한 구현)
            return Game1.player.stats.CropsShipped > 10;
        }
        
        private bool HasGottenFirstAnimalProduct()
        {
            // 첫 동물 제품 획득 체크
            return Game1.player.stats.MilkPailUsed > 0 || Game1.player.stats.ShearsUsed > 0;
        }
        
        private bool HasAnimalBond()
        {
            // 동물 하트 5개 달성 체크 (간단한 구현)
            return Game1.player.stats.AnimalsPet > 50;
        }
        
        private bool HasGottenFirstHeart()
        {
            // 첫 NPC 하트 달성 체크
            foreach (var npc in Game1.getAllFarmers())
            {
                foreach (var friendship in npc.friendshipData.Pairs)
                {
                    if (friendship.Value.Points >= 250) // 1하트
                        return true;
                }
            }
            return false;
        }
        
        private bool HasMarried()
        {
            // 결혼 체크
            return Game1.player.isMarried();
        }
        
        private bool HasChild()
        {
            // 아이 출생 체크
            return Game1.player.getChildren().Count > 0;
        }
        
        private bool HasFoundSpecialItem()
        {
            // 특별한 아이템 발견 체크 (간단한 구현)
            return Game1.player.stats.ItemsForaged > 100;
        }
        
        private bool HasPerfectForaging()
        {
            // 완벽한 채집 체크
            return Game1.player.stats.ItemsForaged > 50;
        }
        
        private bool HasCaughtFirstFish()
        {
            // 첫 물고기 낚기 체크
            return Game1.player.stats.FishCaught > 0;
        }
        
        private bool HasCaughtRareFish()
        {
            // 희귀한 물고기 낚기 체크
            return Game1.player.stats.FishCaught > 20;
        }
        
        private bool HasReachedDeepFloor()
        {
            // 광산 50층 도달 체크
            return Game1.player.stats.DaysPlayed > 30; // 간단한 구현
        }
        
        private bool HasFoundRareMineral()
        {
            // 희귀한 광물 발견 체크
            return Game1.player.stats.RocksCrushed > 100;
        }
        
        private bool HasSeenRainbow()
        {
            // 무지개 목격 체크 (간단한 구현)
            return Game1.isRaining && Game1.random.NextDouble() < 0.01;
        }
        
        private bool HasSeenShootingStar()
        {
            // 별똥별 목격 체크 (간단한 구현)
            return Game1.random.NextDouble() < 0.005;
        }
        
        private bool IsBirthday()
        {
            // 생일 체크
            return Game1.player.birthdayDay == Game1.dayOfMonth && Game1.player.birthdaySeason == Game1.currentSeason;
        }
        
        private bool IsFestivalDay()
        {
            // 축제일 체크
            return Game1.isFestival();
        }
        
        private bool HasCompletedAllFarmWork()
        {
            // 모든 농장 일 완료 체크 (간단한 구현)
            return Game1.player.stats.CropsShipped > 0 && Game1.player.stats.AnimalsPet > 0;
        }
        
        // 공개 메서드들
        public bool CanCreateArtwork(string itemId)
        {
            var inspiration = encyclopedia.GetInspirationByItemId(itemId);
            if (inspiration == null) return false;
            
            return inspiration.IsUnlocked;
        }
        
        public void CreateArtwork(string itemId)
        {
            var inspiration = encyclopedia.GetInspirationByItemId(itemId);
            if (inspiration == null) return;
            
            if (CanCreateArtwork(itemId))
            {
                // 아이템 제작
                var player = Game1.player;
                var item = new StardewValley.Object(int.Parse(itemId), 1);
                player.addItemToInventory(item);
                
                // 경험치 부여
                DrawingSkill.AddDrawingExperience(player, inspiration.ExpAmount);
                
                // 성공 메시지
                string itemName = GetItemName(itemId);
                Game1.addHUDMessage(new HUDMessage($"{itemName} 제작 완료!", HUDMessage.achievement_type));
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
        
        public DrawingInspirationEncyclopedia GetEncyclopedia()
        {
            return encyclopedia;
        }
        
        public bool HasInspiration()
        {
            // 해금된 영감이 하나라도 있으면 true
            var unlockedInspirations = encyclopedia.GetUnlockedInspirations();
            return unlockedInspirations.Count > 0;
        }
        
        public void ConsumeInspiration()
        {
            // 영감은 소모되지 않음 (영구 보존)
        }
        
        public float GetInspirationBonus()
        {
            // 해금된 영감 수에 따라 보너스 증가
            var unlockedInspirations = encyclopedia.GetUnlockedInspirations();
            return 1.0f + (unlockedInspirations.Count * 0.1f); // 영감 1개당 10% 보너스
        }
        
        // UI에서 사용할 메서드들
        public List<DrawingInspirationEncyclopedia.InspirationEntry> GetUnlockedInspirations()
        {
            return encyclopedia.GetUnlockedInspirations();
        }
        
        public List<DrawingInspirationEncyclopedia.InspirationEntry> GetAllInspirations()
        {
            return encyclopedia.GetAllInspirations();
        }
    }
}

