using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace DrawingActivityMod
{
    public class DrawingInspirationState
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public int Intensity { get; set; } // 1-3단계
        public int Duration { get; set; } // 게임 내 시간 (분)
        public DateTime AcquiredTime { get; set; }
        
        public DrawingInspirationState(string type, string name, int intensity = 1, int duration = 180) // 3시간
        {
            Type = type;
            Name = name;
            Intensity = intensity;
            Duration = duration;
            AcquiredTime = DateTime.Now;
        }
        
        public bool IsExpired()
        {
            return (DateTime.Now - AcquiredTime).TotalMinutes >= Duration;
        }
        
        public int GetRemainingDuration()
        {
            int elapsed = (int)(DateTime.Now - AcquiredTime).TotalMinutes;
            return Math.Max(0, Duration - elapsed);
        }
        
        public float GetIntensityMultiplier()
        {
            return 1.0f + (Intensity - 1) * 0.5f; // 1.0x, 1.5x, 2.0x
        }
    }
    
    public class DrawingInspirationManager
    {
        private IModHelper helper;
        private IMonitor monitor;
        private List<DrawingInspirationState> activeInspirations;
        private Dictionary<string, int> inspirationCooldowns; // 같은 영감의 쿨다운 (분 단위)
        private Dictionary<string, int> inspirationCooldownDays; // 영감별 쿨다운 일수
        
        public DrawingInspirationManager(IModHelper helper, IMonitor monitor)
        {
            this.helper = helper;
            this.monitor = monitor;
            this.activeInspirations = new List<DrawingInspirationState>();
            this.inspirationCooldowns = new Dictionary<string, int>();
            this.inspirationCooldownDays = new Dictionary<string, int>();
            
            // 영감별 쿨다운 일수 설정
            InitializeCooldownSettings();
        }
        
        private void InitializeCooldownSettings()
        {
            // 기본 영감 (7-10일 쿨타임)
            inspirationCooldownDays["dawn_beauty"] = 7;
            inspirationCooldownDays["sunset_gold"] = 7;
            inspirationCooldownDays["rain_melody"] = 5;
            inspirationCooldownDays["snow_silence"] = 5;
            
            // 특별한 영감 (14-21일 쿨타임)
            inspirationCooldownDays["rainbow_mystery"] = 14;
            inspirationCooldownDays["harvest_joy"] = 10;
            inspirationCooldownDays["perfect_crop"] = 21;
            inspirationCooldownDays["nature_gift"] = 14;
            
            // 전설의 영감 (30-90일 쿨타임)
            inspirationCooldownDays["farmer_joy"] = 30;
            inspirationCooldownDays["true_friendship"] = 60;
            inspirationCooldownDays["love_completion"] = 90;
        }
        
        public void Initialize()
        {
            // 이벤트 등록
            this.helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
            this.helper.Events.Display.RenderedHud += this.OnRenderedHud;
        }
        
        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            // 영감 상태 업데이트
            UpdateInspirationStates();
            
            // 쿨다운 업데이트
            UpdateCooldowns();
        }
        
        private void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            // 영감 상태 HUD 표시
            DrawInspirationHUD(e.SpriteBatch);
        }
        
        private void UpdateInspirationStates()
        {
            // 만료된 영감 제거
            for (int i = activeInspirations.Count - 1; i >= 0; i--)
            {
                if (activeInspirations[i].IsExpired())
                {
                    string expiredName = activeInspirations[i].Name;
                    activeInspirations.RemoveAt(i);
                    this.monitor.Log($"영감 '{expiredName}'이 만료되었습니다.", LogLevel.Info);
                }
            }
        }
        
        private void UpdateCooldowns()
        {
            // 쿨다운 감소
            var keysToRemove = new List<string>();
            foreach (var kvp in inspirationCooldowns)
            {
                inspirationCooldowns[kvp.Key] = Math.Max(0, kvp.Value - 1);
                if (inspirationCooldowns[kvp.Key] == 0)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
            
            foreach (string key in keysToRemove)
            {
                inspirationCooldowns.Remove(key);
            }
        }
        
        public bool TryAddInspiration(string type, string name, int intensity = 1, int duration = 180)
        {
            // 쿨다운 체크 (일 단위)
            if (IsOnCooldown(type))
            {
                int remainingDays = GetRemainingCooldownDays(type);
                this.monitor.Log($"'{name}' 영감은 아직 쿨다운 중입니다. ({remainingDays}일 후 사용 가능)", LogLevel.Debug);
                return false; // 쿨다운 중
            }
            
            // 같은 타입의 영감이 이미 있는지 체크
            var existingInspiration = activeInspirations.Find(i => i.Type == type);
            if (existingInspiration != null)
            {
                // 기존 영감 강화
                existingInspiration.Intensity = Math.Min(3, existingInspiration.Intensity + intensity);
                existingInspiration.Duration = Math.Max(existingInspiration.Duration, duration);
                this.monitor.Log($"영감 '{name}'이 강화되었습니다! 강도: {existingInspiration.Intensity}", LogLevel.Info);
            }
            else
            {
                // 새로운 영감 추가
                var newInspiration = new DrawingInspirationState(type, name, intensity, duration);
                activeInspirations.Add(newInspiration);
                this.monitor.Log($"새로운 영감 '{name}'을 얻었습니다! 강도: {intensity}", LogLevel.Info);
            }
            
            // 쿨다운 설정 (일 단위)
            int cooldownDays = inspirationCooldownDays.GetValueOrDefault(type, 7);
            inspirationCooldowns[type] = cooldownDays * 24 * 60; // 일을 분으로 변환
            
            return true;
        }
        
        public bool HasInspiration(string type = null)
        {
            if (type == null)
            {
                return activeInspirations.Count > 0;
            }
            
            return activeInspirations.Exists(i => i.Type == type);
        }
        
        public List<DrawingInspirationState> GetActiveInspirations()
        {
            return new List<DrawingInspirationState>(activeInspirations);
        }
        
        public float GetInspirationBonus()
        {
            if (activeInspirations.Count == 0)
            {
                return 1.0f;
            }
            
            float totalBonus = 0f;
            foreach (var inspiration in activeInspirations)
            {
                totalBonus += inspiration.GetIntensityMultiplier();
            }
            
            return 1.0f + totalBonus; // 기본 1.0 + 영감 보너스
        }
        
        public bool ConsumeInspiration(string type = null)
        {
            if (type == null)
            {
                // 가장 강한 영감 소모
                if (activeInspirations.Count > 0)
                {
                    var strongestInspiration = activeInspirations[0];
                    foreach (var inspiration in activeInspirations)
                    {
                        if (inspiration.Intensity > strongestInspiration.Intensity)
                        {
                            strongestInspiration = inspiration;
                        }
                    }
                    
                    activeInspirations.Remove(strongestInspiration);
                    this.monitor.Log($"영감 '{strongestInspiration.Name}'을 사용했습니다.", LogLevel.Info);
                    return true;
                }
            }
            else
            {
                // 특정 타입의 영감 소모
                var inspiration = activeInspirations.Find(i => i.Type == type);
                if (inspiration != null)
                {
                    activeInspirations.Remove(inspiration);
                    this.monitor.Log($"영감 '{inspiration.Name}'을 사용했습니다.", LogLevel.Info);
                    return true;
                }
            }
            
            return false;
        }
        
        private void DrawInspirationHUD(SpriteBatch spriteBatch)
        {
            if (activeInspirations.Count == 0)
            {
                return;
            }
            
            // HUD 위치 설정
            int x = 20;
            int y = 20;
            
            // 영감 아이콘과 텍스트 그리기
            for (int i = 0; i < activeInspirations.Count; i++)
            {
                var inspiration = activeInspirations[i];
                
                // 영감 아이콘 (간단한 원으로 표현)
                Color iconColor = GetInspirationColor(inspiration.Intensity);
                spriteBatch.DrawCircle(x, y + i * 30, 10, iconColor, 2);
                
                // 영감 이름과 지속시간
                string text = $"{inspiration.Name} ({inspiration.GetRemainingDuration()}분)";
                spriteBatch.DrawString(Game1.smallFont, text, new Vector2(x + 25, y + i * 30 - 5), Color.White);
            }
        }
        
        private Color GetInspirationColor(int intensity)
        {
            switch (intensity)
            {
                case 1: return Color.LightBlue;
                case 2: return Color.Gold;
                case 3: return Color.Magenta;
                default: return Color.White;
            }
        }
        
        // 편의 메서드들
        public void AddDawnInspiration()
        {
            TryAddInspiration("dawn", localization.GetInspirationName("dawn_beauty"), 1, 120);
        }
        
        public void AddSunsetInspiration()
        {
            TryAddInspiration("sunset", localization.GetInspirationName("sunset_gold"), 1, 120);
        }
        
        public void AddRainInspiration()
        {
            TryAddInspiration("rain", localization.GetInspirationName("rain_melody"), 2, 180);
        }
        
        public void AddSnowInspiration()
        {
            TryAddInspiration("snow", localization.GetInspirationName("snow_silence"), 2, 180);
        }
        
        public void AddRainbowInspiration()
        {
            TryAddInspiration("rainbow", localization.GetInspirationName("rainbow_mystery"), 3, 240);
        }
        
        // 쿨다운 관련 메서드들
        public bool IsOnCooldown(string type)
        {
            if (!inspirationCooldowns.ContainsKey(type))
                return false;
                
            return inspirationCooldowns[type] > 0;
        }
        
        public int GetRemainingCooldownDays(string type)
        {
            if (!inspirationCooldowns.ContainsKey(type))
                return 0;
                
            int remainingMinutes = inspirationCooldowns[type];
            return (remainingMinutes + 1439) / 1440; // 분을 일로 변환 (올림)
        }
        
        public int GetCooldownDays(string type)
        {
            return inspirationCooldownDays.GetValueOrDefault(type, 7);
        }
    }
}
