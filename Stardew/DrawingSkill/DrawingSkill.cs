using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace DrawingActivityMod
{
    public class DrawingSkill
    {
        public static string SkillId = "spacechase0.DrawingActivity";
        
        public static void RegisterSkill(IModHelper helper, LocalizationManager localization)
        {
            var spaceCore = helper.ModRegistry.GetApi("spacechase0.SpaceCore");
            if (spaceCore != null)
            {
                // 스킬 정의
                var skillData = new
                {
                    Id = SkillId,
                    Name = "그림",
                    Description = "그림 그리기와 예술 작품 제작에 관한 스킬입니다.",
                    Icon = "LooseSprites\\Cursors",
                    SourceRect = new Rectangle(0, 0, 16, 16),
                    Color = Color.Purple,
                    LevelUpPerk = "그림 스킬 레벨업! 새로운 작품을 제작할 수 있습니다.",
                    Professions = new[]
                    {
                        new
                        {
                            Id = "DrawingMaster",
                            Name = "그림의 달인",
                            Description = "그림 제작 시 25% 추가 경험치를 획득합니다.",
                            Level = 5
                        },
                        new
                        {
                            Id = "DrawingInspiration",
                            Name = "예술적 영감",
                            Description = "영감을 더 자주 얻을 수 있습니다.",
                            Level = 10
                        }
                    }
                };
                
                // SpaceCore에 스킬 등록
                spaceCore.RegisterSkill(skillData);
            }
        }
        
        public static int GetDrawingLevel(Farmer farmer)
        {
            var spaceCore = ModEntry.Instance.Helper.ModRegistry.GetApi("spacechase0.SpaceCore");
            if (spaceCore != null)
            {
                return spaceCore.GetLevel(farmer, SkillId);
            }
            return 0;
        }
        
        public static void AddDrawingExperience(Farmer farmer, int amount)
        {
            var spaceCore = ModEntry.Instance.Helper.ModRegistry.GetApi("spacechase0.SpaceCore");
            if (spaceCore != null)
            {
                spaceCore.AddExperience(farmer, SkillId, amount);
            }
        }
        
        public static int GetDrawingExperience(Farmer farmer)
        {
            var spaceCore = ModEntry.Instance.Helper.ModRegistry.GetApi("spacechase0.SpaceCore");
            if (spaceCore != null)
            {
                return spaceCore.GetExperience(farmer, SkillId);
            }
            return 0;
        }
        
        public static bool HasProfession(Farmer farmer, string professionId)
        {
            var spaceCore = ModEntry.Instance.Helper.ModRegistry.GetApi("spacechase0.SpaceCore");
            if (spaceCore != null)
            {
                return spaceCore.HasProfession(farmer, professionId);
            }
            return false;
        }
    }
}
