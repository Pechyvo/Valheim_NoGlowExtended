# No Glow Expanded

Disable the light, particles, or material glow on selected Valheim items. Settings are applied on your client and can be changed in-game with ConfigurationManager.

Built for **Valheim 1.0.30**.

## Supported effects

- **World light** — light an item casts on the floor, walls, or your character.
- **Particles** — effects attached to an item, such as sparks, flames, or fog.
- **Material glow** — parts of the model that glow on their own, such as runes, crystals, or a blade.

Each item shows the controls it supports. The mod is aimed at persistent visuals and preserves ordinary pickup sparkles.

## Coverage

| Category | Examples |
| --- | --- |
| **Deep North** | Thunderblood and Frostfire weapon variants, Northern Vengeance, Spirit Caller, Intricate Key |
| **Ashlands** | Blood, Lightning, and Nature weapon variants, Dyrnwyn, Dundr, Trollstav, Staff of the Wild |
| **Mistlands** | Mistwalker, Himminafl, Jotun Bane, Spinesnap, Skull Splittur, staves, Feather Cape |
| **Earlier biomes** | Frostner, Draugr Fang, Crystal Battleaxe, Porcupine, Dragon Egg, Dragon Tear, Surtling Core, Ancient Seed |
| **Plants and crafting stations** | Thistle, Yellow Mushroom, Jotun Puffs, Magecap, Artisan Table, Galdr Table |

The complete list is in the config, grouped by biome and item type. This release covers vanilla items. Dedicated ValheimArmory support is planned separately.

## Installation

- **Required:** [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/) and [Jötunn](https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/).
- **Optional:** [ConfigurationManager by shudnal](https://thunderstore.io/c/valheim/p/shudnal/ConfigurationManager/) for in-game settings.

Install with your mod manager and launch Valheim modded. If you already use A2 No Glow, disable it before enabling this fork.

## Configuration

Press **F1 → No Glow Expanded** in ConfigurationManager. Set your preferences in **Defaults**, then open an item’s row for exceptions.

- **Use defaults** applies your shared settings to the effects that item supports.
- **Keep original effects** restores the item’s original visuals.
- **Custom** lets you choose effects individually. For example, enable **Suppress world light** and leave **Hide particles** off to keep a weapon’s sparks.

Re-equip gear or pick up and re-drop an item after changing its settings. Supported world objects can update in place. Without ConfigurationManager, close the game and edit `BepInEx/config/NoGlowExpanded.cfg` in your mod profile after the first launch.

For items without a dedicated row, enable **Advanced** in ConfigurationManager and open **Custom Prefabs**. Each field takes a comma-separated list of prefab IDs. Dedicated item settings take precedence.

### Known limitation

Some effects are created separately or use custom shaders. **Dead Raiser** can still show a pink glow around the skull.

## Source and credits

- [No Glow Expanded source on GitHub](https://github.com/Pechyvo/Valheim_NoGlowExtended)
- Based on [A2 No Glow](https://thunderstore.io/c/valheim/p/A2/No_Glow/) v1.2.3. Original code and icon design by **A2 Valheim Mods**. The original MIT license is included.
