using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static SpaceCore.Skills;
using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace DrawingActivityMod
{
    public class DrawingSkill : Skill
    {
        public DrawingSkill() : base("drawing")
        {
            // 경험치 곡선 설정 (기본 스킬과 유사)
            this.ExperienceCurve = new int[] { 100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 15000 };
            
            // 스킬 바 색상 설정 (보라색)
            this.ExperienceBarColor = new Color(138, 43, 226);
            
            // 직업 정의
            this.Professions = new List<Profession>();
            
            // 레벨 5 직업들
            var artist = new Profession(this, "artist")
            {
                Name = ModEntry.Instance.Helper.Translation.Get("profession.artist.name"),
                Description = ModEntry.Instance.Helper.Translation.Get("profession.artist.description"),
                Level = 5
            };
            this.Professions.Add(artist);
            
            var sculptor = new Profession(this, "sculptor")
            {
                Name = ModEntry.Instance.Helper.Translation.Get("profession.sculptor.name"),
                Description = ModEntry.Instance.Helper.Translation.Get("profession.sculptor.description"),
                Level = 5
            };
            this.Professions.Add(sculptor);
            
            // 레벨 10 직업들 (분기)
            var masterArtist = new Profession(this, "master_artist")
            {
                Name = ModEntry.Instance.Helper.Translation.Get("profession.master_artist.name"),
                Description = ModEntry.Instance.Helper.Translation.Get("profession.master_artist.description"),
                Level = 10,
                ParentProfessionId = "artist"
            };
            this.Professions.Add(masterArtist);
            
            var artCritic = new Profession(this, "art_critic")
            {
                Name = ModEntry.Instance.Helper.Translation.Get("profession.art_critic.name"),
                Description = ModEntry.Instance.Helper.Translation.Get("profession.art_critic.description"),
                Level = 10,
                ParentProfessionId = "artist"
            };
            this.Professions.Add(artCritic);
            
            var masterSculptor = new Profession(this, "master_sculptor")
            {
                Name = ModEntry.Instance.Helper.Translation.Get("profession.master_sculptor.name"),
                Description = ModEntry.Instance.Helper.Translation.Get("profession.master_sculptor.description"),
                Level = 10,
                ParentProfessionId = "sculptor"
            };
            this.Professions.Add(masterSculptor);
            
            var artDealer = new Profession(this, "art_dealer")
            {
                Name = ModEntry.Instance.Helper.Translation.Get("profession.art_dealer.name"),
                Description = ModEntry.Instance.Helper.Translation.Get("profession.art_dealer.description"),
                Level = 10,
                ParentProfessionId = "sculptor"
            };
            this.Professions.Add(artDealer);
        }

        public override string GetName()
        {
            return ModEntry.Instance.Helper.Translation.Get("skill.name");
        }

        public string GetDescription()
        {
            return ModEntry.Instance.Helper.Translation.Get("skill.description");
        }

        public new Texture2D SkillsPageIcon => null; // Content Patcher에서 처리
        public new Texture2D Icon => null; // Content Patcher에서 처리
        
        // 편의 메서드들
        public static int GetDrawingLevel(Farmer farmer)
        {
            return farmer.GetCustomSkillLevel("drawing");
        }
        
        public static void AddDrawingExperience(Farmer farmer, int amount)
        {
            farmer.AddCustomSkillExperience("drawing", amount);
        }
        
        public static bool HasProfession(Farmer farmer, string professionId)
        {
            var skill = Skills.GetSkill("drawing");
            if (skill == null) return false;
            
            var profession = skill.Professions.FirstOrDefault(p => p.Id == professionId);
            if (profession == null) return false;
            
            return farmer.professions.Contains(profession.GetVanillaId());
        }
    }
}
