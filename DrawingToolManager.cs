using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;

namespace DrawingActivityMod
{
    public class DrawingToolManager
    {
        private IModHelper helper;
        private IMonitor monitor;
        private LocalizationManager localization;
        private Dictionary<string, bool> acquiredTools = new Dictionary<string, bool>();
        
        // 도구 획득 조건
        private Dictionary<string, int> toolRequirements = new Dictionary<string, int>
        {
            { "brush", 2 },      // Abigail 2하트
            { "pencil", 2 },     // Elliott 2하트
            { "paint", 2 },      // Leah 2하트
            { "advanced", 3 }    // Robin 3하트
        };
        
        public DrawingToolManager(IModHelper helper, IMonitor monitor, LocalizationManager localization)
        {
            this.helper = helper;
            this.monitor = monitor;
            this.localization = localization;
        }
        
        public void Initialize()
        {
            // 도구 획득 상태 초기화
            acquiredTools["brush"] = false;
            acquiredTools["pencil"] = false;
            acquiredTools["paint"] = false;
            acquiredTools["advanced"] = false;
            
            // 이벤트 등록
            helper.Events.GameLoop.DayStarted += OnDayStarted;
        }
        
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            CheckToolAcquisition();
        }
        
        public void CheckToolAcquisition()
        {
            var player = Game1.player;
            
            // Abigail과의 관계 확인 (붓)
            if (!acquiredTools["brush"] && player.friendshipData.ContainsKey("Abigail"))
            {
                int hearts = player.friendshipData["Abigail"].Points / 250;
                if (hearts >= toolRequirements["brush"])
                {
                    AcquireTool("brush", "Abigail");
                }
            }
            
            // Elliott과의 관계 확인 (연필)
            if (!acquiredTools["pencil"] && player.friendshipData.ContainsKey("Elliott"))
            {
                int hearts = player.friendshipData["Elliott"].Points / 250;
                if (hearts >= toolRequirements["pencil"])
                {
                    AcquireTool("pencil", "Elliott");
                }
            }
            
            // Leah과의 관계 확인 (물감)
            if (!acquiredTools["paint"] && player.friendshipData.ContainsKey("Leah"))
            {
                int hearts = player.friendshipData["Leah"].Points / 250;
                if (hearts >= toolRequirements["paint"])
                {
                    AcquireTool("paint", "Leah");
                }
            }
            
            // Robin과의 관계 확인 (고급 도구)
            if (!acquiredTools["advanced"] && player.friendshipData.ContainsKey("Robin"))
            {
                int hearts = player.friendshipData["Robin"].Points / 250;
                if (hearts >= toolRequirements["advanced"])
                {
                    AcquireTool("advanced", "Robin");
                }
            }
        }
        
        private void AcquireTool(string toolType, string npcName)
        {
            acquiredTools[toolType] = true;
            
            string toolName = GetToolName(toolType);
            string message = GetAcquisitionMessage(toolType, npcName);
            
            // HUD 메시지 표시
            Game1.addHUDMessage(new HUDMessage(message, HUDMessage.achievement_type));
            
            // 로그 기록
            monitor.Log($"도구 획득: {toolName} ({npcName}과의 관계로 인해)", LogLevel.Info);
            
            // 도구 획득 시 특별한 효과
            ApplyToolAcquisitionEffect(toolType);
        }
        
        private string GetToolName(string toolType)
        {
            switch (toolType)
            {
                case "brush": return "붓";
                case "pencil": return "연필";
                case "paint": return "물감";
                case "advanced": return "고급 그림 도구";
                default: return "알 수 없는 도구";
            }
        }
        
        private string GetAcquisitionMessage(string toolType, string npcName)
        {
            switch (toolType)
            {
                case "brush": return localization.GetString("tool.brush_acquired");
                case "pencil": return localization.GetString("tool.pencil_acquired");
                case "paint": return localization.GetString("tool.paint_acquired");
                case "advanced": return localization.GetString("tool.advanced_acquired");
                default: return "알 수 없는 도구를 획득했습니다.";
            }
        }
        
        private void ApplyToolAcquisitionEffect(string toolType)
        {
            switch (toolType)
            {
                case "brush":
                    // 그림 제작 가능
                    monitor.Log("그림 제작이 가능해졌습니다!", LogLevel.Info);
                    break;
                case "pencil":
                    // 연필 그림 제작 가능
                    monitor.Log("연필 그림 제작이 가능해졌습니다!", LogLevel.Info);
                    break;
                case "paint":
                    // 물감 그림 제작 가능
                    monitor.Log("물감 그림 제작이 가능해졌습니다!", LogLevel.Info);
                    break;
                case "advanced":
                    // 고급 그림 제작 가능
                    monitor.Log("고급 그림 제작이 가능해졌습니다!", LogLevel.Info);
                    break;
            }
        }
        
        public bool HasTool(string toolType)
        {
            return acquiredTools.ContainsKey(toolType) && acquiredTools[toolType];
        }
        
        public bool CanCraftDrawing(string drawingType)
        {
            switch (drawingType)
            {
                case "painting":
                    return HasTool("brush");
                case "sketch":
                    return HasTool("pencil");
                case "watercolor":
                    return HasTool("paint");
                case "advanced":
                    return HasTool("advanced");
                default:
                    return false;
            }
        }
        
        public string GetRequiredTool(string drawingType)
        {
            switch (drawingType)
            {
                case "painting": return "붓";
                case "sketch": return "연필";
                case "watercolor": return "물감";
                case "advanced": return "고급 그림 도구";
                default: return "알 수 없는 도구";
            }
        }
    }
}
