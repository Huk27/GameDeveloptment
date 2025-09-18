<lane orientation="vertical" horizontal-content-alignment="middle">
    <!-- 탭 헤더 -->
    <lane margin="32, 0, 0, -16" z-index="1">
        <tab *repeat={Tabs}
             layout="64px"
             active={<>Active}
             tooltip={DisplayName}
             activate=|^OnTabActivated(Name)|>
            <image layout="32px" sprite={Sprite} vertical-alignment="middle" />
        </tab>
    </lane>
    
    <!-- 메인 콘텐츠 영역 -->
    <frame layout="600px 500px"
           background={@Mods/StardewUI/Sprites/MenuBackground}
           border={@Mods/StardewUI/Sprites/MenuBorder}
           border-thickness="36, 36, 40, 36"
           margin="0,16,0,0"
           padding="16">
        
        <!-- 개요 탭 콘텐츠 -->
        <lane *if={ShowOverviewTab} orientation="vertical" horizontal-content-alignment="middle">
            <banner background={@Mods/StardewUI/Sprites/BannerBackground} 
                    background-border-thickness="48,0" 
                    padding="12" 
                    text="플레이어 개요" />
            
            <lane orientation="vertical" margin="0,16,0,0">
                <label text={:TestProperty} color="#FF00FF" margin="0,0,0,8" />
                <label text={:PlayerName} color="#4A9EFF" margin="0,0,0,8" />
                <label text={:HealthText} color="#4AFF4A" margin="0,0,0,8" />
                <label text={:EnergyText} color="#FFFF4A" margin="0,0,0,8" />
            </lane>
        </lane>
        
        <!-- 인벤토리 탭 콘텐츠 -->
        <lane *if={ShowInventoryTab} orientation="vertical" horizontal-content-alignment="middle">
            <banner background={@Mods/StardewUI/Sprites/BannerBackground} 
                    background-border-thickness="48,0" 
                    padding="12" 
                    text={:InventoryHeaderText} />
            
            <frame layout="600px 400px"
                   margin="0,16,0,0"
                   padding="16"
                   background={@Mods/StardewUI/Sprites/ControlBorder}>
                <scrollable peeking="64">
                    <grid layout="stretch content"
                          item-layout="length: 48"
                          item-spacing="8,8"
                          horizontal-item-alignment="middle">
                        <image *repeat={InventoryItems}
                               layout="stretch content"
                               sprite={this}
                               tooltip={DisplayName}
                               focusable="true" />
                    </grid>
                </scrollable>
            </frame>
        </lane>
        
        <!-- 스킬 탭 콘텐츠 -->
        <lane *if={ShowSkillsTab} orientation="vertical" horizontal-content-alignment="middle">
            <banner background={@Mods/StardewUI/Sprites/BannerBackground} 
                    background-border-thickness="48,0" 
                    padding="12" 
                    text="스킬 정보" />
            
            <lane orientation="vertical" margin="0,16,0,0">
                <label text="스킬 정보가 여기에 표시됩니다" color="#FFFFFF" margin="0,0,0,8" />
                <label text="농업, 채광, 낚시, 전투, 채집" color="#CCCCCC" margin="0,0,0,8" />
            </lane>
        </lane>
        
        <!-- 설정 탭 콘텐츠 -->
        <lane *if={ShowSettingsTab} orientation="vertical" horizontal-content-alignment="middle">
            <banner background={@Mods/StardewUI/Sprites/BannerBackground} 
                    background-border-thickness="48,0" 
                    padding="12" 
                    text="설정" />
            
            <lane orientation="vertical" margin="0,16,0,0">
                <label text="설정 옵션이 여기에 표시됩니다" color="#FFFFFF" margin="0,0,0,8" />
                <label text="음향, 그래픽, 게임플레이 설정" color="#CCCCCC" margin="0,0,0,8" />
            </lane>
        </lane>
        
    </frame>
</lane>
