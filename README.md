# Drawing Activity Mod

A Stardew Valley mod that adds a new Drawing skill to the game. Create various drawing works and participate in art exhibitions.

## Features

### 🎨 Drawing Skill System
- **New Skill**: Adds a "Drawing" skill to Stardew Valley
- **Experience Gain**: Earn experience through daily activities and drawing creation
- **Level Progression**: 10 levels with unique bonuses and effects

### ✨ Inspiration System
- **25 Unique Inspirations**: Each with specific unlock conditions
- **Permanent Unlock**: Once gained, inspirations are permanently available
- **1:1 Mapping**: Each inspiration corresponds to one drawing work
- **Visual Encyclopedia**: Track all your inspirations and their status

### 🛠️ Tool Management
- **NPC Relationship Based**: Acquire tools by building relationships with NPCs
- **Abigail (2 hearts)**: Brush - Basic drawing creation
- **Elliott (2 hearts)**: Pencil - Pencil drawing creation
- **Leah (2 hearts)**: Paint - Watercolor painting creation
- **Robin (3 hearts)**: Advanced Drawing Tools - High-quality drawing creation

### 🖼️ Drawing Works
- **25 Unique Works**: Each corresponding to a specific inspiration
- **Quality System**: Tool count affects success rate and quality
- **Special Effects**: Some works provide gameplay bonuses
- **NPC Reactions**: Gift drawing works to NPCs for relationship bonuses

### 🎯 Daily Activities
- **Automatic Experience**: Gain drawing experience from routine activities
- **Crop Harvesting**: +1-2 experience per harvest
- **Animal Care**: +1-2 experience per animal product
- **Foraging**: +1-2 experience per foraged item
- **Fishing**: +1-2 experience per fish caught
- **NPC Interaction**: +1-2 experience per conversation

## Installation

### Prerequisites
- [SMAPI](https://smapi.io/) (Stardew Modding API)
- [SpaceCore](https://www.nexusmods.com/stardewvalley/mods/1348) mod
- [Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915) mod

### Installation Steps
1. Download the mod files
2. Extract to your Stardew Valley Mods folder
3. Launch the game with SMAPI
4. The Drawing skill will be automatically added

## Usage

### Getting Started
1. **Acquire Tools**: Build relationships with NPCs to get drawing tools
2. **Gain Inspirations**: Complete special events and conditions to unlock inspirations
3. **Create Works**: Use the Drawing Workbench to create drawing works
4. **Level Up**: Gain experience through daily activities and work creation

### Drawing Workbench
- **Inspiration Encyclopedia**: View all unlocked inspirations and create specific works
- **Random Creation**: Create a random work from your unlocked inspirations
- **Tool Status**: See which tools you have and their effects

### Inspiration System
- **Unlock Conditions**: Each inspiration has specific requirements
- **Permanent Access**: Once unlocked, inspirations are always available
- **Work Creation**: Use inspirations to create corresponding drawing works

## Building from Source

### Requirements
- .NET 5.0 SDK
- Stardew Valley game files
- SMAPI development environment

### Build Steps
1. Clone the repository
2. Open the project in Visual Studio or use command line
3. Build the project
4. Copy the output to your Mods folder

```bash
# Windows
build.bat

# Manual build
dotnet build
```

## Configuration

The mod uses Content Patcher for configuration. You can modify:
- Item information in `content.json`
- Localization in `i18n/` folder
- Inspiration data in the code

## Localization

The mod supports multiple languages:
- English (en.json)
- Korean (ko.json)
- Japanese (ja.json)
- Chinese (zh.json)

## Compatibility

- **SMAPI**: 3.0+
- **Stardew Valley**: 1.5+
- **SpaceCore**: Required
- **Content Patcher**: Required

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

## License

This mod is released under the MIT License. See LICENSE file for details.

## Credits

- **Author**: YourName
- **Stardew Valley**: ConcernedApe
- **SMAPI**: Pathoschild
- **SpaceCore**: spacechase0