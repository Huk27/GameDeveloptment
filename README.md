# Game Development Projects

This repository contains various game development projects and mods.

## Projects

### 🎨 Stardew Valley Mods

#### [DrawingSkill](./Stardew/DrawingSkill/)
A comprehensive Stardew Valley mod that adds a new Drawing skill to the game. Create various drawing works, manage inspirations, and participate in art exhibitions.

**Features:**
- New Drawing skill with 10 levels
- 25 unique inspirations with permanent unlock system
- Tool management based on NPC relationships
- Daily activities for automatic experience gain
- Multilingual support (EN, KO, JA, ZH)
- Simplified UI with inspiration encyclopedia

**Installation:**
1. Download the mod files from `Stardew/DrawingSkill/`
2. Extract to your Stardew Valley Mods folder
3. Launch the game with SMAPI

**Requirements:**
- SMAPI 3.0+
- Stardew Valley 1.5+
- SpaceCore mod
- Content Patcher mod

## Repository Structure

```
GameDeveloptment/
├── README.md
└── Stardew/
    └── DrawingSkill/
        ├── DrawingActivityMod.cs
        ├── DrawingSkill.cs
        ├── DrawingWorkbenchMenu.cs
        ├── DrawingInspirationSystem.cs
        ├── DrawingToolManager.cs
        ├── DrawingDailyActivities.cs
        ├── DrawingInspirationEncyclopedia.cs
        ├── DrawingInspirationEncyclopediaMenu.cs
        ├── DrawingInspirationState.cs
        ├── LocalizationManager.cs
        ├── manifest.json
        ├── content.json
        ├── i18n/
        │   ├── en.json
        │   ├── ko.json
        │   ├── ja.json
        │   └── zh.json
        ├── build.bat
        ├── test_mod.bat
        └── README.md
```

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

## License

This repository is released under the MIT License. See individual project directories for specific license information.

## Credits

- **Stardew Valley**: ConcernedApe
- **SMAPI**: Pathoschild
- **SpaceCore**: spacechase0