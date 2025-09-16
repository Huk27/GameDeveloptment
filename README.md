# Game Development Projects

This repository contains various game development projects and mods.

## Projects

### 🎨 Stardew Valley Mods

#### [DrawingSkill](./Stardew/DrawingSkill/)
A comprehensive Stardew Valley mod that adds a new Drawing skill to the game. Create various drawing works, manage inspirations, and participate in art exhibitions.

**Features:**
- New Drawing skill with 10 levels and profession system
- 25 unique inspirations with permanent unlock system
- Tool management based on NPC relationships
- Daily activities for automatic experience gain
- Modern StardewUI-based interface
- Multilingual support (EN, KO, JA, ZH)
- Content Patcher architecture for easy customization

**Technical Implementation:**
- ✅ SMAPI 4.3 compatible
- ✅ SpaceCore framework integration
- ✅ StardewUI modern interface system
- ✅ Content Patcher architecture separation
- ✅ i18n internationalization support
- ✅ Best practices from official guides

**Installation:**
1. Download both mod folders: `Stardew/DrawingSkill/` and `Stardew/CP_DrawingActivity/`
2. Extract both to your Stardew Valley Mods folder
3. Launch the game with SMAPI

**Requirements:**
- SMAPI 4.3+
- Stardew Valley 1.6+
- SpaceCore mod
- StardewUI mod
- Content Patcher mod

## Repository Structure

```
GameDeveloptment/
├── README.md
└── Stardew/
    ├── DrawingSkill/                    # Logic Mod
    │   ├── DrawingActivityMod.cs
    │   ├── DrawingSkill.cs
    │   ├── DrawingInspirationSystem.cs
    │   ├── DrawingToolManager.cs
    │   ├── DrawingDailyActivities.cs
    │   ├── DrawingInspirationEncyclopedia.cs
    │   ├── DrawingInspirationState.cs
    │   ├── manifest.json
    │   ├── i18n/
    │   │   ├── default.json
    │   │   ├── ko.json
    │   │   ├── ja.json
    │   │   └── zh.json
    │   ├── UI/
    │   │   ├── DrawingWorkbench.sml
    │   │   ├── DrawingWorkbenchViewModel.cs
    │   │   ├── DrawingInspirationEncyclopedia.sml
    │   │   └── DrawingInspirationEncyclopediaViewModel.cs
    │   └── DrawingActivityMod_DesignDocument.md
    └── CP_DrawingActivity/              # Content Pack
        ├── manifest.json
        ├── content.json
        └── assets/
            └── drawing_skill_icon.png
```

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

## License

This repository is released under the MIT License. See individual project directories for specific license information.

## Credits

- **Stardew Valley**: ConcernedApe
- **SMAPI**: Pathoschild
- **SpaceCore**: spacechase0
- **StardewUI**: spacechase0
- **Content Patcher**: Pathoschild

## Development Status

### ✅ Completed
- [x] SMAPI 4.3 compatibility
- [x] SpaceCore skill system implementation
- [x] StardewUI modern interface
- [x] Content Patcher architecture separation
- [x] i18n multilingual support
- [x] Code refactoring based on official guides

### 🔄 In Progress
- [ ] Icon assets creation
- [ ] Game testing and optimization
- [ ] Performance tuning

### 📝 Future Plans
- [ ] Additional language support
- [ ] Advanced UI features
- [ ] Mod compatibility testing

---

**Updated by jinhyy** - All code now follows best practices from official SMAPI, SpaceCore, and StardewUI guides.

