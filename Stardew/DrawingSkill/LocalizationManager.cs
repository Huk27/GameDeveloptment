using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace ArtActivityMod
{
    public class LocalizationManager
    {
        private IModHelper helper;
        private IMonitor monitor;
        private Dictionary<string, Dictionary<string, string>> translations;
        private string currentLanguage;
        
        public LocalizationManager(IModHelper helper, IMonitor monitor)
        {
            this.helper = helper;
            this.monitor = monitor;
            this.translations = new Dictionary<string, Dictionary<string, string>>();
            this.currentLanguage = "en"; // 기본 언어는 영어
        }
        
        public void Initialize()
        {
            // 게임 언어 설정 확인
            string gameLanguage = GetGameLanguage();
            this.currentLanguage = gameLanguage;
            
            // 언어 파일 로드
            LoadLanguageFiles();
            
            this.monitor.Log($"Localization initialized. Current language: {this.currentLanguage}", LogLevel.Info);
        }
        
        private string GetGameLanguage()
        {
            // 게임의 언어 설정을 확인
            // 실제 구현에서는 게임의 언어 설정을 읽어와야 함
            try
            {
                // 임시로 한국어로 설정 (실제로는 게임 설정에서 읽어와야 함)
                return "ko";
            }
            catch
            {
                return "en"; // 기본값
            }
        }
        
        private void LoadLanguageFiles()
        {
            string i18nPath = Path.Combine(this.helper.DirectoryPath, "i18n");
            
            if (!Directory.Exists(i18nPath))
            {
                this.monitor.Log("i18n directory not found. Creating default language files.", LogLevel.Warn);
                return;
            }
            
            // 지원하는 언어 목록
            string[] supportedLanguages = { "ko", "en", "ja", "zh" };
            
            foreach (string lang in supportedLanguages)
            {
                string filePath = Path.Combine(i18nPath, $"{lang}.json");
                
                if (File.Exists(filePath))
                {
                    try
                    {
                        string jsonContent = File.ReadAllText(filePath);
                        var translation = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);
                        
                        if (translation != null)
                        {
                            this.translations[lang] = translation;
                            this.monitor.Log($"Loaded language file: {lang}.json", LogLevel.Info);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.monitor.Log($"Failed to load language file {lang}.json: {ex.Message}", LogLevel.Error);
                    }
                }
            }
            
            // 현재 언어가 없으면 영어로 폴백
            if (!this.translations.ContainsKey(this.currentLanguage))
            {
                this.currentLanguage = "en";
                this.monitor.Log($"Language {this.currentLanguage} not found. Falling back to English.", LogLevel.Warn);
            }
        }
        
        public string GetString(string key, params object[] args)
        {
            if (this.translations.ContainsKey(this.currentLanguage) && 
                this.translations[this.currentLanguage].ContainsKey(key))
            {
                string value = this.translations[this.currentLanguage][key];
                
                // 인수 치환
                if (args != null && args.Length > 0)
                {
                    try
                    {
                        return string.Format(value, args);
                    }
                    catch (Exception ex)
                    {
                        this.monitor.Log($"Failed to format string for key '{key}': {ex.Message}", LogLevel.Error);
                        return value;
                    }
                }
                
                return value;
            }
            
            // 현재 언어에 없으면 영어로 폴백
            if (this.currentLanguage != "en" && 
                this.translations.ContainsKey("en") && 
                this.translations["en"].ContainsKey(key))
            {
                string value = this.translations["en"][key];
                
                if (args != null && args.Length > 0)
                {
                    try
                    {
                        return string.Format(value, args);
                    }
                    catch
                    {
                        return value;
                    }
                }
                
                return value;
            }
            
            // 모든 언어에 없으면 키 자체를 반환
            this.monitor.Log($"Translation key '{key}' not found in any language.", LogLevel.Warn);
            return key;
        }
        
        public void SetLanguage(string language)
        {
            if (this.translations.ContainsKey(language))
            {
                this.currentLanguage = language;
                this.monitor.Log($"Language changed to: {language}", LogLevel.Info);
            }
            else
            {
                this.monitor.Log($"Language '{language}' not supported.", LogLevel.Warn);
            }
        }
        
        public string GetCurrentLanguage()
        {
            return this.currentLanguage;
        }
        
        public string[] GetSupportedLanguages()
        {
            return new string[] { "ko", "en", "ja", "zh" };
        }
        
        // 자주 사용되는 번역들을 위한 편의 메서드들
        public string GetModName()
        {
            return GetString("mod.name");
        }
        
        public string GetModDescription()
        {
            return GetString("mod.description");
        }
        
        public string GetSkillName()
        {
            return GetString("skill.name");
        }
        
        public string GetSkillDescription()
        {
            return GetString("skill.description");
        }
        
        public string GetItemName(string itemKey)
        {
            return GetString($"items.{itemKey}");
        }
        
        public string GetItemDescription(string itemKey)
        {
            return GetString($"items.{itemKey}.description");
        }
        
        public string GetArtworkName(string artworkKey)
        {
            return GetString($"artworks.{artworkKey}");
        }
        
        public string GetArtworkDescription(string artworkKey)
        {
            return GetString($"artworks.{artworkKey}.description");
        }
        
        public string GetSculptureName(string sculptureKey)
        {
            return GetString($"sculptures.{sculptureKey}");
        }
        
        public string GetSculptureDescription(string sculptureKey)
        {
            return GetString($"sculptures.{sculptureKey}.description");
        }
        
        public string GetMusicName(string musicKey)
        {
            return GetString($"music.{musicKey}");
        }
        
        public string GetMusicDescription(string musicKey)
        {
            return GetString($"music.{musicKey}.description");
        }
        
        public string GetSpecialEffectName(string effectKey)
        {
            return GetString($"special_effects.{effectKey}");
        }
        
        public string GetSpecialEffectDescription(string effectKey)
        {
            return GetString($"special_effects.{effectKey}.description");
        }
        
        public string GetInspirationName(string inspirationKey)
        {
            return GetString($"inspiration.{inspirationKey}");
        }
        
        public string GetInspirationMessage(string inspirationKey)
        {
            return GetString($"inspiration.{inspirationKey}.message");
        }
        
        public string GetMessage(string messageKey, params object[] args)
        {
            return GetString($"messages.{messageKey}", args);
        }
        
        public string GetEffect(string effectKey)
        {
            return GetString($"effects.{effectKey}");
        }
    }
}
