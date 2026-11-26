# Changelog

## 1.1.0

- Added optional ValheimArmory support. Supported gear appears in its own ConfigurationManager section, grouped by biome and weapon type.
- Added **Hide Forge of Potential glow** under **Defaults**. It is off by default and controls vanilla refinement glow separately from the item’s original effects.
- Added Off, Basic, and Verbose logging levels. Verbose includes the reason a ValheimArmory candidate was skipped.

## 1.0.35 (Forge glow test)

- Added an opt-in setting under Defaults to hide Forge of Potential refinement glow independently of each item's original effects.

## 1.0.34 (ValheimArmory test)

- Added optional ValheimArmory integration to the standard build. Compatible gear appears in a separate ConfigurationManager section, organized by biome and weapon type.
- Completed the ValheimArmory candidate list against the published weapon, shield, and tool catalog. The mod checks the attached models at runtime and adds settings for supported effects.
- Added advanced Off, Basic, and Verbose logging levels for troubleshooting. Verbose includes the reason each ValheimArmory candidate was skipped.

## 1.0.33 (ValheimArmory test)

- Expanded the ValheimArmory candidate list with more Antler and Vine weapons, Dragonfrost shields, Assassin's Knives, Soulstealer, and elemental Nord dual axes.

## 1.0.32 (ValheimArmory test)

- Added Off, Basic, and Verbose logging levels. Verbose reports why a ValheimArmory candidate was skipped.

## 1.0.31 (ValheimArmory test)

- Put per-item effect counts and ValheimArmory registration summaries behind an advanced Debug logging setting, off by default.

## 1.0.30 (ValheimArmory test)

- Added more candidates from ValheimArmory's Bonerot, Dragonfrost, Goblin King, Carapace, Dreadfire, Assassin's, and Thunderblood families.
- Gives each registered candidate effect controls only when the model has matching attached components.

## 1.0.29 (ValheimArmory test)

- Added a separate material glow control for both blades of Dual Dyrnwyn.

## 1.0.28 (ValheimArmory test)

- Matched the vanilla menu's weapons, staves, and elemental sections while retaining existing ValheimArmory config keys.

## 1.0.27 (ValheimArmory test)

- Added more ValheimArmory candidates across the biome list, including dual boss weapons, blood bows, Frostfire Daggers, and staves.
- Candidates show settings when the registered model has attached lights or particle renderers.

## 1.0.26 (ValheimArmory test)

- Added thirteen boss-themed ValheimArmory weapons, including the first Meadows, Black Forest, and Swamp entries.
- Applied the same weapon-type ordering as the vanilla menu within each ValheimArmory biome.

## 1.0.25 (ValheimArmory test)

- Expanded the optional ValheimArmory integration to thirteen audited dual-wield weapons.
- Grouped ValheimArmory settings by biome in their own ConfigurationManager entry and preserved pickup and upgrade cues.

## 1.0.24

- First public vanilla release. Includes per-item world-light, particle, and material-glow controls, biome-based settings, and live changes after re-equipping.
- Includes custom prefab lists for additional gear. ValheimArmory integration remains in a separate test build.

Earlier versions below were development builds.

## 1.0.23

- Added optional ValheimArmory test support for Dual Thunderblood Sword in a separate ConfigurationManager entry.

## 1.0.22

- Replaced Dragon Tear’s material control with a particle control for the attached fog, flare, and flakes. Pickup sparkles are preserved.

## 1.0.21

- Added a material glow control for Dragon Tear’s blue core.
- Added a particle control for Blob Bomb: Frost.

## 1.0.20

- Added world-light controls for Dragon Tear and Surtling Core.
- Removed legacy config migration and unused upstream build files. Existing settings in the current format remain readable.

## 1.0.19

- Added controls for Ancient Seed, Dragon Egg, Skull Splittur, and Intricate Key.
- Moved legacy configuration upgrades into a separate source file.

## 1.0.18

- Removed the experimental Wisplight toggle.
- Shortened the Custom Prefabs labels and clarified what each list controls.

## 1.0.17

- Added a Feather Cape particle control.
- Replaced the two optional mushrooms with an advanced Debug and Tests section for Cheat Sword and an opt-in Wisplight visuals test.
- Added a custom prefab list for material glow suppression.

## 1.0.16

- Migrated the remaining A2 controls for Artisan Table, Thistle, Yellow Mushroom, Blue Mushroom, and Large Glowing Mushroom.
- Split the early biome sections by item type and kept food above mead.

## 1.0.15

- Split Mistlands Structures and Consumables, with food above mead.
- Added a light control for Lingering Eitr Mead under Ashlands Consumables.
- Removed six low-impact Mistlands material glow controls.

## 1.0.14

- Migrated Galdr Table, Jotun Puffs, and Magecap to individual world effect controls.
- Added material glow controls for Black Core, Sealbreaker, Sealbreaker Fragment, Royal Jelly, Sap, and Major Healing Mead.

## 1.0.13

- Moved Mistlands items and structures above weapons.
- Migrated the original meads, Swamp Key, Crystal Battleaxe, Draugr Fang, Frostner, and Porcupine to individual effect controls.
- Added targeted material glow controls for Dead Raiser and supported meads.

## 1.0.12

- Migrated eight Mistlands weapons and staves to individual effect controls and added Bile Bomb's attached particles.
- Added material glow controls for Staff of Embers, Staff of Frost, and Staff of Protection.

## 1.0.11

- Migrated Dyrnwyn and Staff of the Wild to individual effect controls.
- Added a material glow control for Dyrnwyn and attached particle rules for Ember Charge and Basalt Bomb.
- Moved Other Vanilla Effects to the end of the built-in settings.

## 1.0.10

- Ordered the ConfigurationManager sections by progression and shared weapon-type order, with staves first in each tier that has them.
- Renamed the advanced section to Custom Prefabs in ConfigurationManager.

## 1.0.9

- Added Ashlands light and particle controls for 21 elemental weapons, Dundr, and Trollstav.
- Advanced custom prefab lists now start empty. Existing custom IDs are kept, and IDs with dedicated controls are removed from the lists.

## 1.0.8

- Grouped Deep North settings into Staves, Thunderblood, and Frostfire categories in ConfigurationManager.

## 1.0.7

- Added individual controls for the remaining Thunderblood and Frostfire weapon variants.
- Preserved the named pickup sparkles and upgrade visuals when applying extra gear effects.

## 1.0.6

- Deep North settings display the in-game item names in ConfigurationManager.

## 1.0.5

- The Staff of the Wild toggle now also suppresses the two idle spore emitters.

## 1.0.4

- Preserved the pickup effects on Orb of Ahri and ThunderBlood Staff.

## 1.0.3

- Prototyped the Defaults and Deep North sections in ConfigurationManager.
- Added per-weapon settings that take effect after re-equipping.

## 1.0.2

- Disabled light-distance and flicker controllers on configured gear to prevent lights returning after loading.

## 1.0.1

- Suppressed particle-emitted lights separately from particle visuals.

## 1.0.0

- First test build with additional Deep North gear and separate prefab lists for world light and particles.
