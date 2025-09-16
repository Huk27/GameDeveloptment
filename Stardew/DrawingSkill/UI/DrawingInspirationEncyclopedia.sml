<lane orientation="vertical" horizontal-content-alignment="middle">
    <banner background={@Mods/StardewUI/Sprites/BannerBackground} text={@Mods/jinhyy.DrawingActivity/i18n/ui.encyclopedia.title} />
    <frame layout="900px 700px" padding="32,24" background={@Mods/StardewUI/Sprites/ControlBorder}>
        <lane orientation="vertical" spacing="16">
            <!-- 상태 텍스트 -->
            <text text={StatusText} font-size="18" color="Gold" />
            
            <!-- 페이지네이션 -->
            <lane orientation="horizontal" spacing="16" horizontal-content-alignment="center">
                <button text={@Mods/jinhyy.DrawingActivity/i18n/ui.encyclopedia.previous} 
                        on-click={PreviousPage} enabled={HasPreviousPage} />
                <text text={PageInfo} font-size="16" />
                <button text={@Mods/jinhyy.DrawingActivity/i18n/ui.encyclopedia.next} 
                        on-click={NextPage} enabled={HasNextPage} />
            </lane>
            
            <!-- 영감 목록 -->
            <frame layout="fill" padding="16" background={@Mods/StardewUI/Sprites/ControlBorder}>
                <lane orientation="vertical" spacing="8">
                    <foreach item={InspirationItems} as="inspiration">
                        <frame layout="fill" padding="12" background={@Mods/StardewUI/Sprites/ControlBorder}>
                            <lane orientation="horizontal" spacing="12">
                                <text text={inspiration.Name} font-size="16" color="Gold" />
                                <text text={inspiration.Status} font-size="14" color="White" />
                                <button text={@Mods/jinhyy.DrawingActivity/i18n/ui.encyclopedia.create_artwork} 
                                        on-click={inspiration.CreateArtwork} 
                                        enabled={inspiration.CanCreate} />
                            </lane>
                            <text text={inspiration.Description} font-size="12" color="Gray" />
                        </frame>
                    </foreach>
                </lane>
            </frame>
            
            <!-- 하단 버튼들 -->
            <lane orientation="horizontal" spacing="16" horizontal-content-alignment="center">
                <button text={@Mods/jinhyy.DrawingActivity/i18n/ui.encyclopedia.close} 
                        on-click={Close} />
            </lane>
        </lane>
    </frame>
</lane>
