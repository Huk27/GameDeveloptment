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
                <frame *repeat={OverviewCards}
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
                        <frame *repeat={FarmHealthMetrics}
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
                                <label *repeat={Highlights} text={.} color="#FFFFFF" />
                            </lane>
                        </frame>
                    </lane>
                </frame>

                <frame layout="420px stretch"
                       background={@Mods/StardewUI/Sprites/ControlBorder}
                       padding="16">
                    <lane orientation="vertical" spacing="12">
                        <label text="Daily Flow" color="#FFD700" />
                        <frame *repeat={DailyFlow}
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
                    <lane *repeat={OverviewDetails} orientation="horizontal" spacing="10">
                        <label text={Label} color="#C0C0C0" />
                        <spacer layout="stretch" />
                        <label text={Value} color="#FFFFFF" />
                    </lane>
                </lane>
            </frame>
        </lane>
    </frame>
</lane>
