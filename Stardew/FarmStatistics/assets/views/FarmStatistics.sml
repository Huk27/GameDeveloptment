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
        
        <!-- Phase 3.3: 종합 분석 탭 콘텐츠 -->
        <lane *if={ShowAnalysisTab} orientation="vertical" horizontal-content-alignment="middle">
            <banner background={@Mods/StardewUI/Sprites/BannerBackground} 
                    background-border-thickness="48,0" 
                    padding="12" 
                    text={:AnalysisHeaderText} />
            
            <!-- 종합 점수 카드 -->
            <frame layout="600px 120px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="16" margin="0,16,0,8">
                <lane orientation="vertical" horizontal-content-alignment="middle">
                    <label text="🏆 농장 종합 점수" color="#FFD700" margin="0,0,0,8" />
                    <lane orientation="horizontal" horizontal-content-alignment="middle">
                        <label text={:OverallScore} color="#00FF00" font-size="24" />
                        <label text="점" color="#E0E0E0" font-size="18" margin="4,0,0,0" />
                        <label text={:OverallRating} color="#FFD700" margin="16,0,0,0" />
                    </lane>
                </lane>
            </frame>
            
            <!-- 핵심 인사이트 -->
            <frame layout="700px 200px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="16" margin="0,8,0,8">
                <scrollable peeking="32">
                    <lane orientation="vertical">
                        <label text="💡 핵심 인사이트" color="#FFD700" margin="0,0,0,12" />
                        <lane *repeat={KeyInsights} orientation="vertical" margin="0,0,0,8">
                            <label text={<>} color="#E0E0E0" />
                        </lane>
                    </lane>
                </scrollable>
            </frame>
            
            <!-- 실행 가능한 권장사항 -->
            <frame layout="700px 200px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="16" margin="0,8,0,0">
                <scrollable peeking="32">
                    <lane orientation="vertical">
                        <label text="🎯 실행 가능한 권장사항" color="#0080FF" margin="0,0,0,12" />
                        <lane *repeat={ActionableRecommendations} orientation="vertical" margin="0,0,0,8">
                            <label text={<>} color="#E0E0E0" />
                        </lane>
                    </lane>
                </scrollable>
            </frame>
        </lane>
        
        <!-- Phase 3.3: 트렌드 분석 탭 콘텐츠 -->
        <lane *if={ShowTrendsTab} orientation="vertical" horizontal-content-alignment="middle">
            <banner background={@Mods/StardewUI/Sprites/BannerBackground} 
                    background-border-thickness="48,0" 
                    padding="12" 
                    text={:TrendsHeaderText} />
            
            <frame layout="700px 450px"
                   margin="0,16,0,0"
                   padding="16"
                   background={@Mods/StardewUI/Sprites/ControlBorder}>
                <scrollable peeking="64">
                    <lane orientation="vertical">
                        <!-- 수익 트렌드 -->
                        <frame layout="650px 120px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="12" margin="0,0,0,16">
                            <lane orientation="vertical">
                                <label text="💰 수익 트렌드 분석" color="#FFD700" margin="0,0,0,8" />
                                <label text={:ProfitTrendSummary} color="#E0E0E0" />
                            </lane>
                        </frame>
                        
                        <!-- 생산량 트렌드 -->
                        <frame layout="650px 120px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="12" margin="0,0,0,16">
                            <lane orientation="vertical">
                                <label text="📈 생산량 트렌드 분석" color="#00FF00" margin="0,0,0,8" />
                                <label text={:ProductionTrendSummary} color="#E0E0E0" />
                            </lane>
                        </frame>
                        
                        <!-- 효율성 트렌드 -->
                        <frame layout="650px 120px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="12" margin="0,0,0,16">
                            <lane orientation="vertical">
                                <label text="⚡ 효율성 트렌드 분석" color="#0080FF" margin="0,0,0,8" />
                                <label text={:EfficiencyTrendSummary} color="#E0E0E0" />
                            </lane>
                        </frame>
                    </lane>
                </scrollable>
            </frame>
        </lane>
        
        <!-- Phase 3.3: 비교 분석 탭 콘텐츠 -->
        <lane *if={ShowComparisonTab} orientation="vertical" horizontal-content-alignment="middle">
            <banner background={@Mods/StardewUI/Sprites/BannerBackground} 
                    background-border-thickness="48,0" 
                    padding="12" 
                    text={:ComparisonHeaderText} />
            
            <frame layout="700px 450px"
                   margin="0,16,0,0"
                   padding="16"
                   background={@Mods/StardewUI/Sprites/ControlBorder}>
                <scrollable peeking="64">
                    <lane orientation="vertical">
                        <!-- 비교 분석 점수 바 -->
                        <frame layout="650px 200px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="16" margin="0,0,0,16">
                            <lane orientation="vertical">
                                <label text="🏆 벤치마크 비교 점수" color="#FFD700" margin="0,0,0,12" />
                                
                                <lane orientation="vertical" margin="0,0,0,8">
                                    <lane orientation="horizontal">
                                        <label text="수익성" color="#00FF00" />
                                        <label text={:ProfitabilityScore} color="#E0E0E0" />
                                    </lane>
                                    <slider min="0" max="100" value={:ProfitabilityScore} />
                                </lane>
                                
                                <lane orientation="vertical" margin="0,0,0,8">
                                    <lane orientation="horizontal">
                                        <label text="효율성" color="#0080FF" />
                                        <label text={:EfficiencyScore} color="#E0E0E0" />
                                    </lane>
                                    <slider min="0" max="100" value={:EfficiencyScore} />
                                </lane>
                                
                                <lane orientation="vertical" margin="0,0,0,8">
                                    <lane orientation="horizontal">
                                        <label text="다양성" color="#FF8C00" />
                                        <label text={:DiversityScore} color="#E0E0E0" />
                                    </lane>
                                    <slider min="0" max="100" value={:DiversityScore} />
                                </lane>
                                
                                <lane orientation="vertical">
                                    <lane orientation="horizontal">
                                        <label text="성장성" color="#9E4AFF" />
                                        <label text={:GrowthScore} color="#E0E0E0" />
                                    </lane>
                                    <slider min="0" max="100" value={:GrowthScore} />
                                </lane>
                            </lane>
                        </frame>
                        
                        <!-- 상세 비교 분석 -->
                        <frame layout="650px 180px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="12" margin="0,0,0,8">
                            <lane orientation="vertical">
                                <label text="📊 상세 비교 분석" color="#FFD700" margin="0,0,0,8" />
                                <label text={:ProfitabilityComparison} color="#E0E0E0" margin="0,0,0,4" />
                                <label text={:EfficiencyComparison} color="#E0E0E0" margin="0,0,0,4" />
                                <label text={:DiversityComparison} color="#E0E0E0" />
                            </lane>
                        </frame>
                    </lane>
                </scrollable>
            </frame>
        </lane>
        
    </frame>
</lane>