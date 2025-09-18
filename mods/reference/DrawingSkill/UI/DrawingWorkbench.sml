<lane orientation="vertical" horizontal-content-alignment="middle">
    <banner background={@Mods/StardewUI/Sprites/BannerBackground} text={@Mods/jinhyy.DrawingActivity/i18n/ui.workbench.title} />
    <frame layout="800px 600px" padding="32,24" background={@Mods/StardewUI/Sprites/ControlBorder}>
        <lane orientation="vertical" spacing="16">
            <!-- 영감 상태 표시 -->
            <text text={InspirationStatus} font-size="18" color="Gold" />
            
            <!-- 도구 상태 표시 -->
            <text text={ToolStatus} font-size="16" color="White" />
            
            <!-- 버튼들 -->
            <lane orientation="vertical" spacing="12">
                <button text={@Mods/jinhyy.DrawingActivity/i18n/ui.workbench.encyclopedia} 
                        tooltip={@Mods/jinhyy.DrawingActivity/i18n/ui.workbench.encyclopedia_tooltip}
                        on-click={OpenEncyclopedia} />
                
                <button text={@Mods/jinhyy.DrawingActivity/i18n/ui.workbench.create_artwork} 
                        tooltip={@Mods/jinhyy.DrawingActivity/i18n/ui.workbench.create_artwork_tooltip}
                        on-click={CreateArtwork} 
                        enabled={CanCreateArtwork} />
                
                <button text={@Mods/jinhyy.DrawingActivity/i18n/ui.workbench.daily_activities} 
                        tooltip={@Mods/jinhyy.DrawingActivity/i18n/ui.workbench.daily_activities_tooltip}
                        on-click={OpenDailyActivities} />
            </lane>
            
            <!-- 최근 활동 -->
            <frame layout="fill" padding="16" background={@Mods/StardewUI/Sprites/ControlBorder}>
                <lane orientation="vertical" spacing="8">
                    <text text={@Mods/jinhyy.DrawingActivity/i18n/ui.workbench.recent_activities} 
                          font-size="16" color="Gold" />
                    <text text={RecentActivities} font-size="14" />
                </lane>
            </frame>
        </lane>
    </frame>
</lane>
