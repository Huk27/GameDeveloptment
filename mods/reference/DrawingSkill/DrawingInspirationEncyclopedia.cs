using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DrawingActivityMod
{
    public class DrawingInspirationEncyclopedia
    {
        private Dictionary<string, InspirationEntry> inspirationEntries = new Dictionary<string, InspirationEntry>();
        private List<string> unlockedInspirations = new List<string>();
        private IMonitor monitor;

        public class InspirationEntry
        {
            public string Key { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string ItemId { get; set; } // 1:1 대응 아이템
            public string UnlockCondition { get; set; }
            public bool IsUnlocked { get; set; }
            public int ExpAmount { get; set; }
            public Color DisplayColor { get; set; }
        }

        public DrawingInspirationEncyclopedia(IMonitor monitor)
        {
            this.monitor = monitor;
            InitializeInspirationEntries();
        }

        private void InitializeInspirationEntries()
        {
            // 농업 관련 영감
            inspirationEntries["first_seed"] = new InspirationEntry
            {
                Key = "first_seed",
                Name = "새로운 시작",
                Description = "첫 씨앗을 심을 때의 설렘",
                ItemId = "1004", // 봄의 씨앗
                UnlockCondition = "첫 씨앗 심기",
                IsUnlocked = false,
                ExpAmount = 30,
                DisplayColor = Color.LightGreen
            };

            inspirationEntries["perfect_harvest"] = new InspirationEntry
            {
                Key = "perfect_harvest",
                Name = "완벽한 수확",
                Description = "완벽한 품질의 작물을 수확할 때의 기쁨",
                ItemId = "1005", // 황금 밀밭
                UnlockCondition = "고품질 작물 수확",
                IsUnlocked = false,
                ExpAmount = 40,
                DisplayColor = Color.Gold
            };

            inspirationEntries["all_crops_harvested"] = new InspirationEntry
            {
                Key = "all_crops_harvested",
                Name = "풍요로운 하루",
                Description = "모든 작물을 수확했을 때의 만족감",
                ItemId = "1006", // 수확의 기쁨
                UnlockCondition = "하루에 모든 작물 수확",
                IsUnlocked = false,
                ExpAmount = 50,
                DisplayColor = Color.Orange
            };

            // 동물 관련 영감
            inspirationEntries["animal_product"] = new InspirationEntry
            {
                Key = "animal_product",
                Name = "자연의 선물",
                Description = "동물이 주는 첫 선물의 감동",
                ItemId = "1007", // 우유 한 방울
                UnlockCondition = "첫 동물 제품 획득",
                IsUnlocked = false,
                ExpAmount = 25,
                DisplayColor = Color.LightBlue
            };

            inspirationEntries["animal_bond"] = new InspirationEntry
            {
                Key = "animal_bond",
                Name = "깊은 사랑",
                Description = "동물과의 깊은 유대감",
                ItemId = "1008", // 동물과의 유대
                UnlockCondition = "동물 하트 5개 달성",
                IsUnlocked = false,
                ExpAmount = 55,
                DisplayColor = Color.Pink
            };

            // NPC 관계 영감
            inspirationEntries["first_heart"] = new InspirationEntry
            {
                Key = "first_heart",
                Name = "진정한 우정",
                Description = "누군가와의 첫 하트를 얻을 때의 기쁨",
                ItemId = "1009", // 우정의 증명
                UnlockCondition = "첫 NPC 하트 달성",
                IsUnlocked = false,
                ExpAmount = 100,
                DisplayColor = Color.Pink
            };

            inspirationEntries["marriage"] = new InspirationEntry
            {
                Key = "marriage",
                Name = "사랑의 완성",
                Description = "결혼식의 감동적인 순간",
                ItemId = "1010", // 결혼식
                UnlockCondition = "결혼하기",
                IsUnlocked = false,
                ExpAmount = 200,
                DisplayColor = Color.Magenta
            };

            inspirationEntries["child_birth"] = new InspirationEntry
            {
                Key = "child_birth",
                Name = "새로운 생명",
                Description = "아이의 탄생",
                ItemId = "1011", // 아이의 첫 걸음
                UnlockCondition = "아이 출생",
                IsUnlocked = false,
                ExpAmount = 150,
                DisplayColor = Color.Cyan
            };

            // 채집 활동 영감
            inspirationEntries["special_item"] = new InspirationEntry
            {
                Key = "special_item",
                Name = "숨겨진 보물",
                Description = "특별한 아이템을 발견했을 때의 기쁨",
                ItemId = "1012", // 숲의 비밀
                UnlockCondition = "특별한 채집 아이템 발견",
                IsUnlocked = false,
                ExpAmount = 45,
                DisplayColor = Color.Green
            };

            inspirationEntries["perfect_foraging"] = new InspirationEntry
            {
                Key = "perfect_foraging",
                Name = "자연의 수확",
                Description = "완벽한 채집의 만족감",
                ItemId = "1013", // 계절의 선물
                UnlockCondition = "고품질 채집 아이템 수집",
                IsUnlocked = false,
                ExpAmount = 35,
                DisplayColor = Color.LightGreen
            };

            // 낚시 활동 영감
            inspirationEntries["first_fish"] = new InspirationEntry
            {
                Key = "first_fish",
                Name = "물의 선물",
                Description = "첫 물고기를 잡았을 때의 기쁨",
                ItemId = "1014", // 물고기의 춤
                UnlockCondition = "첫 물고기 낚기",
                IsUnlocked = false,
                ExpAmount = 30,
                DisplayColor = Color.Blue
            };

            inspirationEntries["rare_fish"] = new InspirationEntry
            {
                Key = "rare_fish",
                Name = "바다의 신비",
                Description = "희귀한 물고기를 잡았을 때의 경이로움",
                ItemId = "1015", // 깊은 바다
                UnlockCondition = "희귀한 물고기 낚기",
                IsUnlocked = false,
                ExpAmount = 60,
                DisplayColor = Color.DarkBlue
            };

            // 광산 활동 영감
            inspirationEntries["deep_floor"] = new InspirationEntry
            {
                Key = "deep_floor",
                Name = "땅의 깊이",
                Description = "깊은 광산 층에 도달했을 때의 성취감",
                ItemId = "1016", // 광산의 깊이
                UnlockCondition = "광산 50층 도달",
                IsUnlocked = false,
                ExpAmount = 80,
                DisplayColor = Color.Brown
            };

            inspirationEntries["rare_mineral"] = new InspirationEntry
            {
                Key = "rare_mineral",
                Name = "땅속의 보물",
                Description = "희귀한 광물을 발견했을 때의 기쁨",
                ItemId = "1017", // 땅속의 보물
                UnlockCondition = "희귀한 광물 발견",
                IsUnlocked = false,
                ExpAmount = 70,
                DisplayColor = Color.Gold
            };

            // 특별한 이벤트 영감
            inspirationEntries["rainbow"] = new InspirationEntry
            {
                Key = "rainbow",
                Name = "자연의 기적",
                Description = "무지개를 목격할 때의 경이로움",
                ItemId = "1018", // 무지개의 아름다움
                UnlockCondition = "무지개 목격",
                IsUnlocked = false,
                ExpAmount = 70,
                DisplayColor = Color.Magenta
            };

            inspirationEntries["shooting_star"] = new InspirationEntry
            {
                Key = "shooting_star",
                Name = "소원의 순간",
                Description = "별똥별을 볼 때의 소원",
                ItemId = "1019", // 별똥별의 소원
                UnlockCondition = "별똥별 목격",
                IsUnlocked = false,
                ExpAmount = 90,
                DisplayColor = Color.Cyan
            };

            inspirationEntries["birthday"] = new InspirationEntry
            {
                Key = "birthday",
                Name = "특별한 날",
                Description = "생일의 특별함",
                ItemId = "1020", // 생일의 기쁨
                UnlockCondition = "생일 이벤트",
                IsUnlocked = false,
                ExpAmount = 100,
                DisplayColor = Color.Yellow
            };

            inspirationEntries["festival"] = new InspirationEntry
            {
                Key = "festival",
                Name = "축제의 기쁨",
                Description = "마을 축제의 즐거움",
                ItemId = "1021", // 축제의 기쁨
                UnlockCondition = "마을 축제 참가",
                IsUnlocked = false,
                ExpAmount = 80,
                DisplayColor = Color.Orange
            };
        }

        public List<InspirationEntry> GetAllInspirations()
        {
            return inspirationEntries.Values.ToList();
        }

        public List<InspirationEntry> GetUnlockedInspirations()
        {
            return unlockedInspirations.Select(key => inspirationEntries[key]).ToList();
        }

        public InspirationEntry GetInspiration(string key)
        {
            return inspirationEntries.ContainsKey(key) ? inspirationEntries[key] : null;
        }

        public InspirationEntry GetInspirationByItemId(string itemId)
        {
            return inspirationEntries.Values.FirstOrDefault(i => i.ItemId == itemId);
        }

        public void UnlockInspiration(string key)
        {
            if (inspirationEntries.ContainsKey(key) && !unlockedInspirations.Contains(key))
            {
                inspirationEntries[key].IsUnlocked = true;
                unlockedInspirations.Add(key);
                monitor.Log($"영감 '{inspirationEntries[key].Name}'이 해금되었습니다!", LogLevel.Info);
            }
        }

        public bool IsInspirationUnlocked(string key)
        {
            return unlockedInspirations.Contains(key);
        }

        public List<string> GetAvailableItemIds()
        {
            return unlockedInspirations.Select(key => inspirationEntries[key].ItemId).ToList();
        }
    }
}
