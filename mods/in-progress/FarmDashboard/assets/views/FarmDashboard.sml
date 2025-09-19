<lane orientation="vertical" horizontal-content-alignment="middle">
    <lane margin="32,0,0,-16" z-index="1">
        <tab *repeat={Tabs}
             layout="64px"
             active={<>Active}
             tooltip={:DisplayName}
             activate=|^OnTabActivated(Name)|>
            <image layout="32px" sprite={:Sprite} vertical-alignment="middle" />
        </tab>
    </lane>
    <frame layout="880px 560px"
           background={@Mods/StardewUI/Sprites/MenuBackground}
           border={@Mods/StardewUI/Sprites/MenuBorder}
           border-thickness="36,36,40,36"
           margin="0,16,0,0"
           padding="24">
        <lane *if={ShowOverviewTab} orientation="vertical" spacing="16">
            <banner background={@Mods/StardewUI/Sprites/BannerBackground}
                    background-border-thickness="48,0"
                    padding="12"
                    text="Farm Overview" />

            <grid layout="stretch content"
                  item-layout="length: 180"
                  item-spacing="16,16"
                  horizontal-item-alignment="middle">
                <frame *repeat={Overview.Cards}
                       layout="180px 110px"
                       background={@Mods/StardewUI/Sprites/ControlBorder}
                       padding="12">
                    <lane orientation="vertical" horizontal-content-alignment="middle">
                        <label text={Title} color={TitleColor} margin="0,0,0,6" />
                        <label text={Value} color={ValueColor} font="dialogue" />
                        <label *if={HasHint} text={Hint} color="#B0B0B0" margin="0,6,0,0" />
                    </lane>
                </frame>
            </grid>

            <lane orientation="horizontal" item-alignment="stretch" item-spacing="16">
                <frame layout="420px stretch"
                       background={@Mods/StardewUI/Sprites/ControlBorder}
                       padding="16">
                    <lane orientation="vertical" spacing="12">
                        <label text="Farm Health" color="#FFD700" />
                        <frame *repeat={Overview.FarmHealth}
                               layout="stretch content"
                               background={@Mods/StardewUI/Sprites/ControlBorder}
                               padding="10">
                            <lane orientation="vertical" spacing="6">
                                <label text={Label} color="#FFFFFF" />
                                <label text={Value} color="#FFFFFF" />
                                <label *if={HasBar} text={Bar} color={SeverityColor} />
                            </lane>
                        </frame>

                        <label text="Highlights" color="#FFD700" />
                        <frame layout="stretch content"
                               background={@Mods/StardewUI/Sprites/ControlBorder}
                               padding="10">
                            <lane orientation="vertical" spacing="6">
                                <label *repeat={Overview.Highlights} text={.} color="#FFFFFF" />
                            </lane>
                        </frame>
                    </lane>
                </frame>

                <frame layout="420px stretch"
                       background={@Mods/StardewUI/Sprites/ControlBorder}
                       padding="16">
                    <lane orientation="vertical" spacing="12">
                        <label text="Daily Flow" color="#FFD700" />
                        <frame *repeat={Overview.DailyFlow}
                               layout="stretch content"
                               background={@Mods/StardewUI/Sprites/ControlBorder}
                               padding="10">
                            <lane orientation="vertical" spacing="4">
                                <label text={Label} color="#FFFFFF" />
                                <lane orientation="horizontal" spacing="6">
                                    <label text="Earned" color="#2ECC40" />
                                    <label text={Earnings} color="#FFFFFF" />
                                    <label *if={HasPositiveBar} text={PositiveBar} color="#2ECC40" />
                                </lane>
                                <lane orientation="horizontal" spacing="6">
                                    <label text="Spent" color="#FF4136" />
                                    <label text={Expenses} color="#FFFFFF" />
                                    <label *if={HasNegativeBar} text={NegativeBar} color="#FF4136" />
                                </lane>
                                <label text={Net} color={NetColor} />
                            </lane>
                        </frame>
                    </lane>
                </frame>
            </lane>

            <frame layout="stretch content"
                   background={@Mods/StardewUI/Sprites/ControlBorder}
                   padding="16">
                <label text="Key Metrics" color="#FFD700" />
                <lane orientation="vertical" spacing="8" margin="0,8,0,0">
                    <lane *repeat={Overview.KeyMetrics} orientation="horizontal" spacing="10">
                        <label text={Label} color="#C0C0C0" />
                        <spacer layout="stretch" />
                        <label text={Value} color="#FFFFFF" />
                    </lane>
                </lane>
            </frame>
        </lane>

        <lane *if={ShowCropsTab} orientation="vertical" spacing="16">
            <banner background={@Mods/StardewUI/Sprites/BannerBackground}
                    background-border-thickness="48,0"
                    padding="12"
                    text="Crops" />

            <grid layout="stretch content"
                  item-layout="length: 200"
                  item-spacing="16,16"
                  horizontal-item-alignment="start">
                <frame *repeat={Crops.Summaries}
                       layout="200px 140px"
                       background={@Mods/StardewUI/Sprites/ControlBorder}
                       padding="12">
                    <lane orientation="vertical" spacing="6">
                        <label text={Name} color="#FFFFFF" />
                        <label text={Location} color="#B0B0B0" />
                        <label text={ReadyText} color={StatusColor} />
                        <label text={GrowthText} color="#FFFFFF" />
                        <label *if={HasProgress} text={ProgressText} color="#C0C0C0" />
                        <label *if={HasValue} text={ValueText} color="#7FDBFF" />
                        <label *if={HasAlerts} text={AlertsText} color="#FF851B" />
                    </lane>
                </frame>
            </grid>

            <lane orientation="horizontal" item-alignment="stretch" item-spacing="16">
                <frame layout="420px stretch"
                       background={@Mods/StardewUI/Sprites/ControlBorder}
                       padding="16">
                    <label text="Upcoming Harvests" color="#FFD700" />
                    <lane orientation="vertical" spacing="6" margin="0,8,0,0">
                        <lane *repeat={Crops.Forecasts} orientation="horizontal" spacing="8">
                            <label text={Label} color="#FFFFFF" />
                            <spacer layout="stretch" />
                            <label text={CountdownText} color={EmphasisColor} />
                            <label *if={HasValue} text={ValueText} color="#7FDBFF" margin="8,0,0,0" />
                        </lane>
                    </lane>
                </frame>

                <frame layout="420px stretch"
                       background={@Mods/StardewUI/Sprites/ControlBorder}
                       padding="16">
                    <label text="Care Tasks" color="#FFD700" />
                    <lane orientation="vertical" spacing="6" margin="0,8,0,0">
                        <frame *repeat={Crops.CareTasks}
                               layout="stretch content"
                               background={@Mods/StardewUI/Sprites/ControlBorder}
                               padding="10">
                            <lane orientation="vertical" spacing="4">
                                <label text={Label} color="#FFFFFF" />
                                <label text={Details} color={StatusColor} />
                            </lane>
                        </frame>
                    </lane>
                </frame>
            </lane>
        </lane>

        <lane *if={ShowAnimalsTab} orientation="vertical" spacing="16">
            <banner background={@Mods/StardewUI/Sprites/BannerBackground}
                    background-border-thickness="48,0"
                    padding="12"
                    text="Animals" />

            <grid layout="stretch content"
                  item-layout="length: 200"
                  item-spacing="16,16"
                  horizontal-item-alignment="start">
                <frame *repeat={Animals.Animals}
                       layout="200px 150px"
                       background={@Mods/StardewUI/Sprites/ControlBorder}
                       padding="12">
                    <lane orientation="vertical" spacing="6">
                        <label text={Name} color="#FFFFFF" />
                        <label text={Type} color="#B0B0B0" />
                        <label text={Building} color="#B0B0B0" />
                        <label text={Mood} color={MoodColor} />
                        <label text={HappinessText} color="#FFFFFF" />
                        <label text={ProduceText} color="#2ECC40" />
                    </lane>
                </frame>
            </grid>

            <frame layout="stretch content"
                   background={@Mods/StardewUI/Sprites/ControlBorder}
                   padding="16">
                <label text="Care Queue" color="#FFD700" />
                <lane orientation="vertical" spacing="6" margin="0,8,0,0">
                    <frame *repeat={Animals.CareTasks}
                           layout="stretch content"
                           background={@Mods/StardewUI/Sprites/ControlBorder}
                           padding="10">
                        <lane orientation="vertical" spacing="4">
                            <label text={Label} color="#FFFFFF" />
                            <label text={Details} color={StatusColor} />
                        </lane>
                    </frame>
                </lane>
            </frame>
        </lane>

        <lane *if={ShowTimeActivityTab} orientation="vertical" spacing="16">
            <banner background={@Mods/StardewUI/Sprites/BannerBackground}
                    background-border-thickness="48,0"
                    padding="12"
                    text="Time & Activity" />

            <frame layout="stretch content"
                   background={@Mods/StardewUI/Sprites/ControlBorder}
                   padding="16">
                <lane orientation="vertical" spacing="6">
                    <label text="Today" color="#FFD700" />
                    <lane orientation="horizontal" spacing="10">
                        <label text="Playtime" color="#FFFFFF" />
                        <label text={TimeActivity.Summary.TodayPlaytime} color="#C0C0C0" />
                        <spacer layout="stretch" />
                        <label text="Gold/hr" color="#FFFFFF" />
                        <label text={TimeActivity.Summary.GoldPerHour} color="#C0C0C0" />
                    </lane>
                    <label *if={TimeActivity.Summary.HasRecommendation}
                           text={TimeActivity.Summary.Recommendation}
                           color="#FFFFFF" />
                </lane>
            </frame>

            <frame layout="stretch 200px"
                   background={@Mods/StardewUI/Sprites/ControlBorder}
                   padding="16">
                <label text="Activity Breakdown" color="#FFD700" />
                <lane orientation="vertical" spacing="6" margin="0,8,0,0">
                    <lane *repeat={TimeActivity.Activities} orientation="horizontal" spacing="10">
                        <label text={Label} color="#FFFFFF" />
                        <spacer layout="stretch" />
                        <label text={Duration} color="#C0C0C0" />
                        <label *if={HasPercentage} text={PercentageText} color="#FFD700" />
                    </lane>
                </lane>
            </frame>

            <frame layout="stretch content"
                   background={@Mods/StardewUI/Sprites/ControlBorder}
                   padding="16">
                <label text="Recommendations" color="#FFD700" />
                <lane orientation="vertical" spacing="6" margin="0,8,0,0">
                    <frame *repeat={TimeActivity.Recommendations}
                           layout="stretch content"
                           background={@Mods/StardewUI/Sprites/ControlBorder}
                           padding="10">
                        <lane orientation="vertical" spacing="4">
                            <label text={Title} color="#FFFFFF" />
                            <label text={Message} color="#C0C0C0" />
                            <label text={Category} color="#7FDBFF" />
                        </lane>
                    </frame>
                </lane>
            </frame>

            <frame layout="stretch content"
                   background={@Mods/StardewUI/Sprites/ControlBorder}
                   padding="16">
                <label text="Recent Gold Flow" color="#FFD700" />
                <lane orientation="vertical" spacing="6" margin="0,8,0,0">
                    <lane *repeat={TimeActivity.RecentGoldFlow} orientation="vertical" spacing="4">
                        <label text={Label} color="#FFFFFF" />
                        <lane orientation="horizontal" spacing="6">
                            <label text="Earned" color="#2ECC40" />
                            <label text={Earnings} color="#FFFFFF" />
                            <label *if={HasPositiveBar} text={PositiveBar} color="#2ECC40" />
                        </lane>
                        <lane orientation="horizontal" spacing="6">
                            <label text="Spent" color="#FF4136" />
                            <label text={Expenses} color="#FFFFFF" />
                            <label *if={HasNegativeBar} text={NegativeBar} color="#FF4136" />
                        </lane>
                        <label text={Net} color={NetColor} />
                    </lane>
                </lane>
            </frame>
        </lane>

        <lane *if={ShowGoalsTab} orientation="vertical" spacing="16">
            <banner background={@Mods/StardewUI/Sprites/BannerBackground}
                    background-border-thickness="48,0"
                    padding="12"
                    text="Goals & Rewards" />

            <frame layout="stretch content"
                   background={@Mods/StardewUI/Sprites/ControlBorder}
                   padding="16">
                <label text="Active Goals" color="#FFD700" />
                <lane orientation="vertical" spacing="6" margin="0,8,0,0">
                    <frame *repeat={Goals.Goals}
                           layout="stretch content"
                           background={@Mods/StardewUI/Sprites/ControlBorder}
                           padding="10">
                        <lane orientation="vertical" spacing="4">
                            <label text={Name} color="#FFFFFF" />
                            <label text={Description} color="#C0C0C0" />
                            <lane orientation="horizontal" spacing="6">
                                <label text={StatusText} color={StatusColor} />
                                <spacer layout="stretch" />
                                <label *if={HasProgress} text={ProgressText} color="#FFFFFF" />
                            </lane>
                        </lane>
                    </frame>
                </lane>
            </frame>

            <frame layout="stretch content"
                   background={@Mods/StardewUI/Sprites/ControlBorder}
                   padding="16">
                <label text="Custom Goals" color="#FFD700" />
                <lane orientation="vertical" spacing="6" margin="0,8,0,0">
                    <frame *repeat={Goals.CustomGoals}
                           layout="stretch content"
                           background={@Mods/StardewUI/Sprites/ControlBorder}
                           padding="10">
                        <lane orientation="vertical" spacing="4">
                            <label text={Name} color="#FFFFFF" />
                            <label text={Description} color="#C0C0C0" />
                            <label text={StatusText} color={AccentColor} />
                        </lane>
                    </frame>
                </lane>
            </frame>
        </lane>

        <label text={LastUpdatedText} color="#B0B0B0" margin="0,16,0,0" horizontal-alignment="right" />
    </frame>
</lane>
