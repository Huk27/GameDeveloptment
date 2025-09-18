<lane orientation="vertical" horizontal-content-alignment="middle">
    <banner background={@Mods/StardewUI/Sprites/BannerBackground} background-border-thickness="48,0" padding="12" text={:HeaderText} />
    <frame layout="400px content" 
           background={@Mods/StardewUI/Sprites/MenuBackground}
           border={@Mods/StardewUI/Sprites/MenuBorder}
           border-thickness="36, 36, 40, 36"
           margin="0,16,0,0"
           horizontal-content-alignment="middle" 
           vertical-content-alignment="middle">
        <lane orientation="vertical" horizontal-content-alignment="middle">
            <label text={:TestProperty} color="#FF00FF" margin="0,0,0,8" />
            <label text={:PlayerName} color="#4A9EFF" margin="0,0,0,8" />
            <label text={:HealthText} color="#4AFF4A" margin="0,0,0,8" />
            <label text={:EnergyText} color="#FFFF4A" margin="0,0,0,8" />
        </lane>
    </frame>
</lane>
