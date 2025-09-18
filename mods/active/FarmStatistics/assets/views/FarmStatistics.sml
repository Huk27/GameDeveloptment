<lane orientation="vertical" horizontal-content-alignment="middle">
    <!-- ???ㅻ뜑 -->
    <lane margin="32, 0, 0, -16" z-index="1">
        <tab *repeat={Tabs}
             layout="64px"
             active={<>Active}
             tooltip={DisplayName}
             activate=|^OnTabActivated(Name)|>
            <image layout="32px" sprite={Sprite} vertical-alignment="middle" />
        </tab>
    </lane>
    
    <!-- 硫붿씤 肄섑뀗痢??곸뿭 -->
    <frame layout="800px 600px"
           background={@Mods/StardewUI/Sprites/MenuBackground}
           border={@Mods/StardewUI/Sprites/MenuBorder}
           border-thickness="36, 36, 40, 36"
           margin="0,16,0,0"
           padding="16">
        
        <!-- 媛쒖슂 ??肄섑뀗痢?-->
        <lane *if={ShowOverviewTab} orientation="vertical" horizontal-content-alignment="middle">
            <banner background={@Mods/StardewUI/Sprites/BannerBackground} 
                    background-border-thickness="48,0" 
                    padding="12" 
                    text="?띿옣 ?듦퀎 媛쒖슂" />
            
            <!-- 二쇱슂 吏??移대뱶??-->
            <grid layout="stretch content" item-layout="length: 180" item-spacing="16,16" margin="0,16,0,0">
                <frame layout="180px 120px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="12">
                    <lane orientation="vertical" horizontal-content-alignment="middle">
                        <label text="?뮥 珥??섏씡" color="#FFD700" margin="0,0,0,8" />
                        <label text={:TotalEarnings} color="#00FF00" />
                    </lane>
                </frame>
                
                <frame layout="180px 120px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="12">
                    <lane orientation="vertical" horizontal-content-alignment="middle">
                        <label text="?뙮 ?묐Ъ ?섑솗" color="#00FF00" margin="0,0,0,8" />
                        <label text={:TotalCropsHarvested} color="#E0E0E0" />
                    </lane>
                </frame>
                
                <frame layout="180px 120px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="12">
                    <lane orientation="vertical" horizontal-content-alignment="middle">
                        <label text="?릢 ?숇Ъ ?쒗뭹" color="#FF8C00" margin="0,0,0,8" />
                        <label text={:TotalAnimalProducts} color="#E0E0E0" />
                    </lane>
                </frame>
                
                <frame layout="180px 120px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="12">
                    <lane orientation="vertical" horizontal-content-alignment="middle">
                        <label text="???뚮젅???쒓컙" color="#0080FF" margin="0,0,0,8" />
                        <label text={:TotalPlayTime} color="#E0E0E0" />
                    </lane>
                </frame>
            </grid>
            
            <!-- 怨꾩젅 鍮꾧탳 -->
            <frame layout="600px 80px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="16" margin="0,16,0,0">
                <lane orientation="vertical" horizontal-content-alignment="middle">
                    <label text="?뱤 怨꾩젅蹂??깃낵" color="#FFD700" margin="0,0,0,8" />
                    <label text={:SeasonComparison} color="#E0E0E0" />
                </lane>
            </frame>
        </lane>
        
        <!-- ?묐Ъ ??肄섑뀗痢?-->
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
                                    <label text="?섑솗??" color="#B0B0B0" />
                                    <label text={:Harvested} color="#E0E0E0" />
                                </lane>
                                <lane orientation="horizontal">
                                    <label text="?섏씡:" color="#B0B0B0" />
                                    <label text={:Revenue} color="#00FF00" />
                                </lane>
                                <lane orientation="horizontal">
                                    <label text="?깆옣?쒓컙:" color="#B0B0B0" />
                                    <label text={:GrowthTime} color="#0080FF" />
                                </lane>
                            </lane>
                        </frame>
                    </grid>
                </scrollable>
            </frame>
        </lane>
        
        <!-- ?숇Ъ ??肄섑뀗痢?-->
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
                                    <label text="?쒗뭹 ??" color="#B0B0B0" />
                                    <label text={:Products} color="#E0E0E0" />
                                </lane>
                                <lane orientation="horizontal">
                                    <label text="?섏씡:" color="#B0B0B0" />
                                    <label text={:Revenue} color="#00FF00" />
                                </lane>
                                <lane orientation="horizontal">
                                    <label text="?됰났??" color="#B0B0B0" />
                                    <label text={:Happiness} color="#FFD700" />
                                </lane>
                            </lane>
                        </frame>
                    </grid>
                </scrollable>
            </frame>
        </lane>
        
        <!-- ?쒓컙 ??肄섑뀗痢?-->
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
        
        <!-- 紐⑺몴 ??肄섑뀗痢?-->
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
        
        <!-- Phase 3.3: 醫낇빀 遺꾩꽍 ??肄섑뀗痢?-->
        <lane *if={ShowAnalysisTab} orientation="vertical" horizontal-content-alignment="middle">
            <banner background={@Mods/StardewUI/Sprites/BannerBackground} 
                    background-border-thickness="48,0" 
                    padding="12" 
                    text={:AnalysisHeaderText} />
            
            <!-- 醫낇빀 ?먯닔 移대뱶 -->
            <frame layout="600px 120px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="16" margin="0,16,0,8">
                <lane orientation="vertical" horizontal-content-alignment="middle">
                    <label text="?룇 ?띿옣 醫낇빀 ?먯닔" color="#FFD700" margin="0,0,0,8" />
                    <lane orientation="horizontal" horizontal-content-alignment="middle">
                        <label text={:OverallScore} color="#00FF00" font-size="24" />
                        <label text="?? color="#E0E0E0" font-size="18" margin="4,0,0,0" />
                        <label text={:OverallRating} color="#FFD700" margin="16,0,0,0" />
                    </lane>
                </lane>
            </frame>
            
            <!-- ?듭떖 ?몄궗?댄듃 -->
            <frame layout="700px 200px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="16" margin="0,8,0,8">
                <scrollable peeking="32">
                    <lane orientation="vertical">
                        <label text="?뮕 ?듭떖 ?몄궗?댄듃" color="#FFD700" margin="0,0,0,12" />
                        <lane *repeat={KeyInsights} orientation="vertical" margin="0,0,0,8">
                            <label text={<>} color="#E0E0E0" />
                        </lane>
                    </lane>
                </scrollable>
            </frame>
            
            <!-- ?ㅽ뻾 媛?ν븳 沅뚯옣?ы빆 -->
            <frame layout="700px 200px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="16" margin="0,8,0,0">
                <scrollable peeking="32">
                    <lane orientation="vertical">
                        <label text="?렞 ?ㅽ뻾 媛?ν븳 沅뚯옣?ы빆" color="#0080FF" margin="0,0,0,12" />
                        <lane *repeat={ActionableRecommendations} orientation="vertical" margin="0,0,0,8">
                            <label text={<>} color="#E0E0E0" />
                        </lane>
                    </lane>
                </scrollable>
            </frame>
        </lane>
        
        <!-- Phase 3.3: ?몃젋??遺꾩꽍 ??肄섑뀗痢?-->
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
                        <!-- ?섏씡 ?몃젋??-->
                        <frame layout="650px 120px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="12" margin="0,0,0,16">
                            <lane orientation="vertical">
                                <label text="?뮥 ?섏씡 ?몃젋??遺꾩꽍" color="#FFD700" margin="0,0,0,8" />
                                <label text={:ProfitTrendSummary} color="#E0E0E0" />
                            </lane>
                        </frame>
                        
                        <!-- ?앹궛???몃젋??-->
                        <frame layout="650px 120px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="12" margin="0,0,0,16">
                            <lane orientation="vertical">
                                <label text="?뱢 ?앹궛???몃젋??遺꾩꽍" color="#00FF00" margin="0,0,0,8" />
                                <label text={:ProductionTrendSummary} color="#E0E0E0" />
                            </lane>
                        </frame>
                        
                        <!-- ?⑥쑉???몃젋??-->
                        <frame layout="650px 120px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="12" margin="0,0,0,16">
                            <lane orientation="vertical">
                                <label text="???⑥쑉???몃젋??遺꾩꽍" color="#0080FF" margin="0,0,0,8" />
                                <label text={:EfficiencyTrendSummary} color="#E0E0E0" />
                            </lane>
                        </frame>
                    </lane>
                </scrollable>
            </frame>
        </lane>
        
        <!-- Phase 3.3: 鍮꾧탳 遺꾩꽍 ??肄섑뀗痢?-->
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
                        <!-- 鍮꾧탳 遺꾩꽍 ?먯닔 諛?-->
                        <frame layout="650px 200px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="16" margin="0,0,0,16">
                            <lane orientation="vertical">
                                <label text="?룇 踰ㅼ튂留덊겕 鍮꾧탳 ?먯닔" color="#FFD700" margin="0,0,0,12" />
                                
                                <lane orientation="vertical" margin="0,0,0,8">
                                    <lane orientation="horizontal">
                                        <label text="?섏씡?? color="#00FF00" />
                                        <label text={:ProfitabilityScore} color="#E0E0E0" />
                                    </lane>
                                    <slider min="0" max="100" value={:ProfitabilityScore} />
                                </lane>
                                
                                <lane orientation="vertical" margin="0,0,0,8">
                                    <lane orientation="horizontal">
                                        <label text="?⑥쑉?? color="#0080FF" />
                                        <label text={:EfficiencyScore} color="#E0E0E0" />
                                    </lane>
                                    <slider min="0" max="100" value={:EfficiencyScore} />
                                </lane>
                                
                                <lane orientation="vertical" margin="0,0,0,8">
                                    <lane orientation="horizontal">
                                        <label text="?ㅼ뼇?? color="#FF8C00" />
                                        <label text={:DiversityScore} color="#E0E0E0" />
                                    </lane>
                                    <slider min="0" max="100" value={:DiversityScore} />
                                </lane>
                                
                                <lane orientation="vertical">
                                    <lane orientation="horizontal">
                                        <label text="?깆옣?? color="#9E4AFF" />
                                        <label text={:GrowthScore} color="#E0E0E0" />
                                    </lane>
                                    <slider min="0" max="100" value={:GrowthScore} />
                                </lane>
                            </lane>
                        </frame>
                        
                        <!-- ?곸꽭 鍮꾧탳 遺꾩꽍 -->
                        <frame layout="650px 180px" background={@Mods/StardewUI/Sprites/ControlBorder} padding="12" margin="0,0,0,8">
                            <lane orientation="vertical">
                                <label text="?뱤 ?곸꽭 鍮꾧탳 遺꾩꽍" color="#FFD700" margin="0,0,0,8" />
                                <label text={:ProfitabilityComparison} color="#E0E0E0" margin="0,0,0,4" />
                                <label text={:EfficiencyComparison} color="#E0E0E0" margin="0,0,0,4" />
                                <label text={:DiversityComparison} color="#E0E0E0" />
                                <label text={:GrowthComparison} color=\
                            </lane>
                        </frame>
                    </lane>
                </scrollable>
            </frame>
        </lane>
        
    </frame>
</lane>

