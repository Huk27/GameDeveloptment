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
    <frame layout="800px 600px"
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
                    text="농장 통계 개요" />
            
            <!-- 주요 지표 카드들 -->
            <grid layout="stretch content" item-layout="length: 180" item-spacing="16,16" margin="0,16,0,0">
                <frame layout="180px 120px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="12">
                    <lane orientation="vertical" horizontal-content-alignment="middle">
                        <label text="💰 총 수익" color="#FFD700" margin="0,0,0,8" />
                        <label text={:TotalEarnings} color="#00FF00" />
                    </lane>
                </frame>
                
                <frame layout="180px 120px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="12">
                    <lane orientation="vertical" horizontal-content-alignment="middle">
                        <label text="🌱 작물 수확" color="#00FF00" margin="0,0,0,8" />
                        <label text={:TotalCropsHarvested} color="#E0E0E0" />
                    </lane>
                </frame>
                
                <frame layout="180px 120px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="12">
                    <lane orientation="vertical" horizontal-content-alignment="middle">
                        <label text="🐄 동물 제품" color="#FF8C00" margin="0,0,0,8" />
                        <label text={:TotalAnimalProducts} color="#E0E0E0" />
                    </lane>
                </frame>
                
                <frame layout="180px 120px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="12">
                    <lane orientation="vertical" horizontal-content-alignment="middle">
                        <label text="⏰ 플레이 시간" color="#0080FF" margin="0,0,0,8" />
                        <label text={:TotalPlayTime} color="#E0E0E0" />
                    </lane>
                </frame>
            </grid>
            
            <!-- 계절 비교 -->
            <frame layout="600px 80px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="16" margin="0,16,0,0">
                <lane orientation="vertical" horizontal-content-alignment="middle">
                    <label text="📊 계절별 성과" color="#FFD700" margin="0,0,0,8" />
                    <label text={:SeasonComparison} color="#E0E0E0" />
                </lane>
            </frame>
        </lane>
        
        <!-- 작물 탭 콘텐츠 -->
        <lane *if={ShowCropsTab} orientation="vertical" horizontal-content-alignment="middle">
            <banner background={@Mods/StardewUI/Sprites/BannerBackground} 
                    background-border-thickness="48,0" 
                    padding="12" 
                    text={:CropsHeaderText} />
            
            <frame layout="700px 450px"
                   margin="0,16,0,0"
                   padding="16"
                   background={@Mods/StardewUI/Sprites/ControlBorder}>
                <scrollable peeking="64">
                    <grid layout="stretch content"
                          item-layout="length: 320"
                          item-spacing="16,16"
                          horizontal-item-alignment="middle">
                        <frame *repeat={CropStatistics}
                               layout="320px 140px"
                               background={@Mods/StardewUI/Sprites/ControlBorder}
                               padding="12">
                            <lane orientation="vertical">
                                <lane orientation="horizontal">
                                    <label text={:CropName} color="#00FF00" />
                                    <label text={:Quality} color="#FFD700" />
                                </lane>
                                <lane orientation="horizontal" margin="0,8,0,0">
                                    <label text="수확량:" color="#B0B0B0" />
                                    <label text={:Harvested} color="#E0E0E0" />
                                </lane>
                                <lane orientation="horizontal">
                                    <label text="수익:" color="#B0B0B0" />
                                    <label text={:Revenue} color="#00FF00" />
                                </lane>
                                <lane orientation="horizontal">
                                    <label text="성장시간:" color="#B0B0B0" />
                                    <label text={:GrowthTime} color="#0080FF" />
                                </lane>
                            </lane>
                        </frame>
                    </grid>
                </scrollable>
            </frame>
        </lane>
        
        <!-- 동물 탭 콘텐츠 -->
        <lane *if={ShowAnimalsTab} orientation="vertical" horizontal-content-alignment="middle">
            <banner background={@Mods/StardewUI/Sprites/BannerBackground} 
                    background-border-thickness="48,0" 
                    padding="12" 
                    text={:AnimalsHeaderText} />
            
            <frame layout="700px 450px"
                   margin="0,16,0,0"
                   padding="16"
                   background={@Mods/StardewUI/Sprites/ControlBorder}>
                <scrollable peeking="64">
                    <grid layout="stretch content"
                          item-layout="length: 320"
                          item-spacing="16,16"
                          horizontal-item-alignment="middle">
                        <frame *repeat={AnimalStatistics}
                               layout="320px 140px"
                               background={@Mods/StardewUI/Sprites/ControlBorder}
                               padding="12">
                            <lane orientation="vertical">
                                <label text={:AnimalName} color="#FF8C00" />
                                <lane orientation="horizontal" margin="0,8,0,0">
                                    <label text="제품 수:" color="#B0B0B0" />
                                    <label text={:Products} color="#E0E0E0" />
                                </lane>
                                <lane orientation="horizontal">
                                    <label text="수익:" color="#B0B0B0" />
                                    <label text={:Revenue} color="#00FF00" />
                                </lane>
                                <lane orientation="horizontal">
                                    <label text="행복도:" color="#B0B0B0" />
                                    <label text={:Happiness} color="#FFD700" />
                                </lane>
                            </lane>
                        </frame>
                    </grid>
                </scrollable>
            </frame>
        </lane>
        
        <!-- 시간 탭 콘텐츠 -->
        <lane *if={ShowTimeTab} orientation="vertical" horizontal-content-alignment="middle">
            <banner background={@Mods/StardewUI/Sprites/BannerBackground} 
                    background-border-thickness="48,0" 
                    padding="12" 
                    text={:TimeHeaderText} />
            
            <frame layout="700px 450px"
                   margin="0,16,0,0"
                   padding="16"
                   background={@Mods/StardewUI/Sprites/ControlBorder}>
                <scrollable peeking="64">
                    <lane orientation="vertical">
                        <lane *repeat={TimeStatistics} orientation="vertical" margin="0,0,0,16">
                            <lane orientation="horizontal">
                                <label text={:Activity} color={:Color} />
                                <label text={:Hours} color="#E0E0E0" />
                                <label text={:Percentage} color="#B0B0B0" />
                            </lane>
                            <slider min="0" max="100" value={:Percentage} />
                        </lane>
                    </lane>
                </scrollable>
            </frame>
        </lane>
        
        <!-- 목표 탭 콘텐츠 -->
        <lane *if={ShowGoalsTab} orientation="vertical" horizontal-content-alignment="middle">
            <banner background={@Mods/StardewUI/Sprites/BannerBackground} 
                    background-border-thickness="48,0" 
                    padding="12" 
                    text={:GoalsHeaderText} />
            
            <frame layout="700px 450px"
                   margin="0,16,0,0"
                   padding="16"
                   background={@Mods/StardewUI/Sprites/ControlBorder}>
                <scrollable peeking="64">
                    <lane orientation="vertical">
                        <lane *repeat={GoalStatistics} orientation="vertical" margin="0,0,0,16">
                            <lane orientation="horizontal">
                                <label text={:GoalName} color="#0080FF" />
                                <label text={:ProgressText} color="#E0E0E0" />
                            </lane>
                            <slider min="0" max="100" value={:Progress} />
                        </lane>
                    </lane>
                </scrollable>
            </frame>
        </lane>
        
    </frame>
</lane>