using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.Skills;
using System.Collections.Generic;

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
                Name = "화가",
                Description = "그림 그리기에 특화된 직업입니다. 그림 제작 시 25% 추가 경험치를 획득합니다.",
                Level = 5
            };
            this.Professions.Add(artist);
            
            var sculptor = new Profession(this, "sculptor")
            {
                Name = "조각가",
                Description = "조각 작품 제작에 특화된 직업입니다. 조각 제작 시 25% 추가 경험치를 획득합니다.",
                Level = 5
            };
            this.Professions.Add(sculptor);
            
            // 레벨 10 직업들 (분기)
            var masterArtist = new Profession(this, "master_artist")
            {
                Name = "거장 화가",
                Description = "화가의 최고 단계입니다. 그림 작품의 가치가 50% 증가합니다.",
                Level = 10,
                ParentProfessionId = "artist"
            };
            this.Professions.Add(masterArtist);
            
            var artCritic = new Profession(this, "art_critic")
            {
                Name = "예술 평론가",
                Description = "예술에 대한 깊은 이해를 가진 직업입니다. 영감을 더 자주 얻을 수 있습니다.",
                Level = 10,
                ParentProfessionId = "artist"
            };
            this.Professions.Add(artCritic);
            
            var masterSculptor = new Profession(this, "master_sculptor")
            {
                Name = "거장 조각가",
                Description = "조각가의 최고 단계입니다. 조각 작품의 가치가 50% 증가합니다.",
                Level = 10,
                ParentProfessionId = "sculptor"
            };
            this.Professions.Add(masterSculptor);
            
            var artDealer = new Profession(this, "art_dealer")
            {
                Name = "예술 딜러",
                Description = "예술 작품 거래에 특화된 직업입니다. 작품 판매 시 추가 수익을 얻을 수 있습니다.",
                Level = 10,
                ParentProfessionId = "sculptor"
            };
            this.Professions.Add(artDealer);
        }

        public override string GetName()
        {
            return ModEntry.Instance.Helper.Translation.Get("skill.name");
        }

        public override string GetDescription()
        {
            return ModEntry.Instance.Helper.Translation.Get("skill.description");
        }

        public override Texture2D SkillsPageIcon => null; // Content Patcher에서 처리
        public override Texture2D Icon => null; // Content Patcher에서 처리
    }
}
