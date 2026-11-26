using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;

namespace Pechyvo.NoGlowExpanded
{
    internal static class NoGlowExpanded
    {
        private const string Section = "Extra gear effects";
        private static ConfigEntry<string> _lights = null!;
        private static ConfigEntry<string> _particles = null!;
        private static ConfigEntry<string> _emission = null!;
        private static ConfigEntry<bool> _defaultLights = null!;
        private static ConfigEntry<bool> _defaultParticles = null!;
        private static ConfigEntry<bool> _defaultEmission = null!;
        private static ConfigEntry<bool>? _hideForgeGlow;
        private const string ForgeGlowName = "UpgraderGlow";
        private static readonly HashSet<GameObject> HiddenForgeGlows = new();
        private enum DiagnosticLevel { Off, Basic, Verbose }
        private static ConfigEntry<DiagnosticLevel>? _loggingLevel;
        internal static bool BasicLoggingEnabled => _loggingLevel?.Value is DiagnosticLevel.Basic or DiagnosticLevel.Verbose;
        internal static bool VerboseLoggingEnabled => _loggingLevel?.Value == DiagnosticLevel.Verbose;
        private const string DyrnwynPrefab = "SwordDyrnwyn";
        private const string LegacyDyrnwynPrefab = "SwordIronFire";
        private static readonly Dictionary<Renderer, Material[]> OriginalMaterials = new();
        private static readonly Dictionary<Material, Material> MutedMaterials = new();
        private static readonly Dictionary<string, string> EmissionRenderers = new(StringComparer.Ordinal)
        {
            [DyrnwynPrefab] = "default",
            ["StaffFireball"] = "default (1)",
            ["StaffIceShards"] = "default (1)",
            ["StaffShield"] = "default",
            ["StaffSkeleton"] = "fi_vil_combs_props_bone_skull",
            ["BattleaxeCrystal"] = "default",
            ["MeadFrostResist"] = "default",
            ["MeadEitrMinor"] = "potion",
            ["MeadStaminaLingering"] = "potion",
            ["AncientSeed"] = "model",
            ["DragonEgg"] = "model",
            ["BattleaxeSkullSplittur"] = "battleaxe"
        };
        private const string GaldrTable = "piece_magetable";
        private const string JotunPuffs = "Pickable_Mushroom_JotunPuffs";
        private const string Magecap = "Pickable_Mushroom_Magecap";
        private const string Thistle = "Pickable_Thistle";
        private const string YellowMushroom = "Pickable_Mushroom_yellow";
        private const string ArtisanTable = "piece_artisanstation";
        private static readonly Dictionary<string, string> WorldEmissionRenderers = new(StringComparer.Ordinal)
        {
            [YellowMushroom] = "visual"
        };
        private static bool IsWorldObject(string prefab) => prefab is GaldrTable or JotunPuffs or Magecap
            or Thistle or YellowMushroom or ArtisanTable;
        private static readonly HashSet<string> LightlessGear = new(StringComparer.Ordinal)
        {
            "StaffShield", "BombBile", "BombLava", "BombDynamite", "BombBlob_Frost", "MaceSilver", "CapeFeather", "SwordCheat", "BloodGoldKey"
        };
        private static readonly (string Prefab, string Token, string Name, string Category)[] EarlierGear =
        {
            ("MeadStaminaMinor", "mead_stamina_minor", "Minor Stamina Mead", "Meadows - Consumables"),
            (YellowMushroom, "mushroomyellow", "Yellow Mushroom", "Black Forest - Consumables"),
            ("MeadPoisonResist", "mead_poisonres", "Poison Resistance Mead", "Black Forest - Consumables"),
            (Thistle, "thistle", "Thistle", "Black Forest - Ingredients"),
            ("AncientSeed", "ancientseed", "Ancient Seed", "Black Forest - Items"),
            ("SurtlingCore", "surtlingcore", "Surtling Core", "Black Forest - Items"),
            ("CryptKey", "cryptkey", "Swamp Key", "Swamp - Items"),
            ("MeadFrostResist", "mead_frostres", "Frost Resistance Mead", "Swamp - Consumables"),
            ("DragonEgg", "dragonegg", "Dragon Egg", "Mountains - Items"),
            ("DragonTear", "dragontear", "Dragon Tear", "Mountains - Items"),
            ("BattleaxeCrystal", "battleaxe_crystal", "Crystal Battleaxe", "Mountains - Weapons"),
            ("BowDraugrFang", "bow_draugrfang", "Draugr Fang", "Mountains - Weapons"),
            ("MaceSilver", "mace_silver", "Frostner", "Mountains - Weapons"),
            ("BombBlob_Frost", "bombblob_frost", "Blob Bomb: Frost", "Mountains - Weapons"),
            (ArtisanTable, "$piece_artisanstation", "Artisan Table", "Plains - Structures"),
            ("MeadStaminaMedium", "mead_stamina_medium", "Medium Stamina Mead", "Plains - Consumables"),
            ("MaceNeedle", "mace_needle", "Porcupine", "Plains - Weapons"),
            ("MeadEitrMinor", "mead_eitr_minor", "Minor Eitr Mead", "Mistlands - Consumables"),
            ("MeadStaminaLingering", "mead_stamina_lingering", "Lingering Stamina Mead", "Mistlands - Consumables")
        };
        private static bool HasParticleControl(string prefab) => ArmoryEffects.TryGetValue(prefab, out var armory) ? armory.Particles
            : prefab != "MaceNeedle" && prefab != JotunPuffs
            && prefab is not ("AncientSeed" or "DragonEgg" or "SurtlingCore" or "BattleaxeSkullSplittur")
            && !WorldEmissionRenderers.ContainsKey(prefab)
            && !prefab.StartsWith("Mead", StringComparison.Ordinal);
        private static bool HasLightControl(string prefab) => ArmoryEffects.TryGetValue(prefab, out var armory) ? armory.Light
            : !LightlessGear.Contains(prefab);
        private static bool _prefabsReady;
        private static readonly Dictionary<string, ConfigEntry<EffectChoice>> GearChoices = new();
        private static readonly Dictionary<string, (bool Light, bool Particles)> ArmoryEffects = new(StringComparer.Ordinal);
        private const string DualDyrnwynPrefab = "VADual_SwordDyrnwyn";
        private const string DyrnwynBladeMaterial = "Dyrnwyn_mat";

        private enum EffectChoice
        {
            UseDefaults = 0, SuppressLight = 1, SuppressParticles = 2, SuppressBoth = 3,
            SuppressMaterialGlow = 4, SuppressLightAndGlow = 5, SuppressParticlesAndGlow = 6,
            SuppressAll = 7, KeepOriginalEffects = 8
        }

        // ConfigurationManager reads this tag by name and also supports the native enum editor.
        private sealed class ConfigurationManagerAttributes
        {
            public bool? IsAdvanced;
            public string? DispName;
            public string? Category;
            public int? Order;
            public Action<ConfigEntryBase>? CustomDrawer;
        }
        private static ManualLogSource _logger = null!;
        private static readonly HashSet<Light> DisabledLights = new();
        private static readonly HashSet<Behaviour> DisabledLightControllers = new();
        private static readonly HashSet<ParticleSystem> DisabledParticleLights = new();
        private static readonly HashSet<ParticleSystemRenderer> DisabledParticles = new();

        internal static void Bind(ConfigFile config, ManualLogSource logger)
        {
            _logger = logger;
            _defaultLights = config.Bind("Defaults", "Suppress world light", true,
                "Turn off light cast onto nearby surfaces for gear set to Use defaults.");
            _defaultParticles = config.Bind("Defaults", "Hide particles", true,
                "Hide attached particle visuals for gear set to Use defaults.");
            _defaultEmission = config.Bind("Defaults", "Suppress material glow", true,
                "Suppress verified emissive materials for gear set to Use defaults.");
            _hideForgeGlow = config.Bind("Defaults", "Hide Forge of Potential glow", false,
                "Hide the glow added by Forge of Potential refinement. This setting applies independently of each item's effect settings.");
            _hideForgeGlow.SettingChanged += (_, _) => RefreshForgeGlow();
            foreach (var item in EarlierGear)
            {
                if (item.Category != "Mistlands - Consumables")
                    BindGear(config, item.Prefab, item.Prefab, item.Token.StartsWith("$", StringComparison.Ordinal) ? item.Token : "$item_" + item.Token, item.Name, item.Category);
            }
            BindMistlands(config);
            foreach (var item in EarlierGear)
            {
                if (item.Category == "Mistlands - Consumables")
                    BindGear(config, item.Prefab, item.Prefab, "$item_" + item.Token, item.Name, item.Category);
            }
            BindAshlands(config);
            BindGear(config, "BloodGoldKey", "BloodGoldKey", "$item_bloodgoldkey", "Intricate Key", "Deep North - Items");
            BindGear(config, "StaffFrostOrbs", "Frost Orbs Staff", "$item_staff_frostorbs", "Northern Vengeance", "Deep North - Staves");
            BindGear(config, "StaffSpiritCaller", "Spirit Caller", "$item_staff_spiritcaller", "Spirit Caller", "Deep North - Staves");
            BindGear(config, "SwordGold_BloodLightning", "BloodLightning Sword", "$item_sword_gold_bloodlightning", "Thunderblood Sword", "Deep North - Thunderblood");
            BindGear(config, "SwordGold_FrostFire", "FrostFire Sword", "$item_sword_gold_frostfire", "Frostfire Sword", "Deep North - Frostfire");
            BindVariants(config, "Atgeir", "Atgeir", "atgeir");
            BindVariants(config, "Axe", "Axe", "axe");
            BindVariants(config, "Battleaxe", "Greataxe", "axe2h");
            BindVariants(config, "Bow", "Bow", "bow");
            BindVariants(config, "Crossbow", "Crossbow", "crossbow", frostfireGoldSuffix: true, bloodlightningGoldSuffix: true);
            BindVariants(config, "Fist", "Knucklechains", "fistweapon", frostfireGoldSuffix: true);
            BindVariants(config, "Knife", "Dagger", "knife");
            BindVariants(config, "Mace", "Mace", "mace");
            BindVariants(config, "Sledge", "Sledge", "mace2h");
            BindVariants(config, "Spear", "Spear", "spear");
            BindVariants(config, "THSword", "Greatsword", "sword2h");
            _loggingLevel = config.Bind("Debug and Tests", "Logging level", DiagnosticLevel.Off,
                new ConfigDescription("Off hides diagnostics. Basic shows registration totals. Verbose adds per-item effect counts and skip reasons. Warnings and errors are always logged.",
                    null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 100 }));
            BindGear(config, "SwordCheat", "SwordCheat", "", "Cheat Sword", "Debug and Tests", advanced: true,
                defaultChoice: EffectChoice.KeepOriginalEffects);
            _lights = config.Bind(Section, "DisableLights", "", new ConfigDescription(
                "Comma-separated prefab IDs. Turns off light cast onto nearby surfaces. Dedicated settings take precedence. Re-equip to apply.", null,
                new ConfigurationManagerAttributes { Category = "Custom Prefabs", IsAdvanced = true, DispName = "Suppress world light", Order = 3 }));
            _particles = config.Bind(Section, "HideParticles", "", new ConfigDescription(
                "Comma-separated prefab IDs. Hides attached sparks, smoke, and similar particles. Dedicated settings take precedence. Re-equip to apply.", null,
                new ConfigurationManagerAttributes { Category = "Custom Prefabs", IsAdvanced = true, DispName = "Hide particles", Order = 2 }));
            _emission = config.Bind(Section, "SuppressMaterialGlow", "", new ConfigDescription(
                "Comma-separated prefab IDs. Dims supported glowing surfaces, such as runes or blades. Dedicated settings take precedence. Re-equip to apply.", null,
                new ConfigurationManagerAttributes { Category = "Custom Prefabs", IsAdvanced = true, DispName = "Suppress material glow", Order = 1 }));
            _defaultLights.SettingChanged += OnSettingChanged;
            _defaultParticles.SettingChanged += OnSettingChanged;
            _defaultEmission.SettingChanged += OnSettingChanged;
            _lights.SettingChanged += OnSettingChanged;
            _particles.SettingChanged += OnSettingChanged;
            _emission.SettingChanged += OnSettingChanged;
        }

        private static void BindMistlands(ConfigFile config)
        {
            var gear = new (string Prefab, string Token, string Name, string Group)[]
            {
                (GaldrTable, "piece_magetable", "Galdr Table", "Structures"),
                (JotunPuffs, "item_mushroom_jotunpuffs", "Jotun Puffs", "Consumables"),
                (Magecap, "item_mushroom_magecap", "Magecap", "Consumables"),
                ("AtgeirHimminAfl", "atgeir_himminafl", "Himminafl", "Weapons"),
                ("BattleaxeSkullSplittur", "battleaxe_skullsplittur", "Skull Splittur", "Weapons"),
                ("AxeJotunBane", "axe_jotunbane", "Jotun Bane", "Weapons"),
                ("BowSpineSnap", "bow_snipesnap", "Spinesnap", "Weapons"),
                ("SwordMistwalker", "sword_mistwalker", "Mistwalker", "Weapons"),
                ("BombBile", "bilebomb", "Bile Bomb", "Weapons"),
                ("StaffSkeleton", "staffskeleton", "Dead Raiser", "Staves"),
                ("StaffFireball", "stafffireball", "Staff of Embers", "Staves"),
                ("StaffIceShards", "stafficeshards", "Staff of Frost", "Staves"),
                ("StaffShield", "staffshield", "Staff of Protection", "Staves")
            };
            foreach (var item in gear)
                BindGear(config, item.Prefab, item.Prefab,
                    IsWorldObject(item.Prefab) ? "$" + item.Token : "$item_" + item.Token,
                    item.Name, "Mistlands - " + item.Group);
            BindGear(config, "CapeFeather", "CapeFeather", "$item_cape_feather", "Feather Cape", "Mistlands - Armor");
        }

        private static void BindAshlands(ConfigFile config)
        {
            BindGear(config, "MeadEitrLingering", "MeadEitrLingering", "$item_mead_eitr_lingering", "Lingering Eitr Mead", "Ashlands - Consumables");
            BindGear(config, DyrnwynPrefab, DyrnwynPrefab, "$item_sword_dyrnwyn", "Dyrnwyn", "Ashlands - Weapons");
            BindGear(config, "BombDynamite", "BombDynamite", "$item_bomb_dynamite", "Ember Charge", "Ashlands - Weapons");
            BindGear(config, "BombLava", "BombLava", "$item_lavabomb", "Basalt Bomb", "Ashlands - Weapons");
            var weapons = new (string Prefab, string Token, string Name, string Group)[]
            {
                ("StaffLightning", "staff_lightning", "Dundr", "Staves"),
                ("StaffRedTroll", "staffredtroll", "Trollstav", "Staves"),
                ("StaffGreenRoots", "staffgreenroots", "Staff of the Wild", "Staves"),
                ("AxeBerzerkrBlood", "axe_berzerkr_blood", "Bleeding Berserkir Axes", "Blood"),
                ("AxeBerzerkrLightning", "axe_berzerkr_lightning", "Thundering Berserkir Axes", "Lightning"),
                ("AxeBerzerkrNature", "axe_berzerkr_nature", "Primal Berserkir Axes", "Nature"),
                ("BowAshlandsBlood", "bow_ashlandsblood", "Blood Fang", "Blood"),
                ("BowAshlandsStorm", "bow_ashlandsstorm", "Storm Fang", "Lightning"),
                ("BowAshlandsRoot", "bow_ashlandsroot", "Root Fang", "Nature"),
                ("CrossbowRipperBlood", "crossbow_ripper_blood", "Wound Ripper", "Blood"),
                ("CrossbowRipperLightning", "crossbow_ripper_lightning", "Storm Ripper", "Lightning"),
                ("CrossbowRipperNature", "crossbow_ripper_nature", "Root Ripper", "Nature"),
                ("MaceEldnerBlood", "mace_eldner_blood", "Bloodgeon", "Blood"),
                ("MaceEldnerLightning", "mace_eldner_lightning", "Storm Star", "Lightning"),
                ("MaceEldnerNature", "mace_eldner_nature", "Klossen", "Nature"),
                ("SpearSplitner_Blood", "spear_splitner_blood", "Splitnir the Bleeding", "Blood"),
                ("SpearSplitner_Lightning", "spear_splitner_lightning", "Splitnir the Storming", "Lightning"),
                ("SpearSplitner_Nature", "spear_splitner_nature", "Splitnir the Primal", "Nature"),
                ("SwordNiedhoggBlood", "sword_niedhogg_blood", "Nidhögg the Bleeding", "Blood"),
                ("SwordNiedhoggLightning", "sword_niedhogg_lightning", "Nidhögg the Thundering", "Lightning"),
                ("SwordNiedhoggNature", "sword_niedhogg_nature", "Nidhögg the Primal", "Nature"),
                ("THSwordSlayerBlood", "sword_slayer_blood", "Brutal Slayer", "Blood"),
                ("THSwordSlayerLightning", "sword_slayer_lightning", "Scourging Slayer", "Lightning"),
                ("THSwordSlayerNature", "sword_slayer_nature", "Primal Slayer", "Nature")
            };
            foreach (var weapon in weapons)
                BindGear(config, weapon.Prefab, weapon.Prefab, "$item_" + weapon.Token, weapon.Name, "Ashlands - " + weapon.Group);
        }


        private static void BindVariants(ConfigFile config, string family, string weaponName, string tokenFamily,
            bool frostfireGoldSuffix = false, bool bloodlightningGoldSuffix = false)
        {
            var thunderblood = family + "Gold_BloodLightning";
            var frostfire = family + "Gold_FrostFire";
            var thunderbloodToken = bloodlightningGoldSuffix ? "bloodlightning_gold" : "gold_bloodlightning";
            var frostfireToken = frostfireGoldSuffix ? "frostfire_gold" : "gold_frostfire";
            BindGear(config, thunderblood, thunderblood, $"$item_{tokenFamily}_{thunderbloodToken}", $"Thunderblood {weaponName}", "Deep North - Thunderblood");
            BindGear(config, frostfire, frostfire, $"$item_{tokenFamily}_{frostfireToken}", $"Frostfire {weaponName}", "Deep North - Frostfire");
        }

        internal enum ArmoryRegistration { Added, AlreadyRegistered, NoSupportedEffects }

        internal static ArmoryRegistration RegisterArmoryGear(ConfigFile config, string prefab, GameObject root, string token, string fallbackName, string category, string weaponType)
        {
            if (GearChoices.ContainsKey(prefab)) return ArmoryRegistration.AlreadyRegistered;
            var light = Array.Exists(root.GetComponentsInChildren<Light>(true), component => IsAttachedArmoryEffect(component, root))
                || Array.Exists(root.GetComponentsInChildren<ParticleSystem>(true),
                    component => component.lights.enabled && IsAttachedArmoryEffect(component, root));
            var particles = Array.Exists(root.GetComponentsInChildren<ParticleSystemRenderer>(true),
                component => IsAttachedArmoryEffect(component, root));
            if (!light && !particles) return ArmoryRegistration.NoSupportedEffects;

            ArmoryEffects.Add(prefab, (light, particles));
            BindGear(config, prefab, prefab, token, fallbackName, category, sortFamily: weaponType);
            return ArmoryRegistration.Added;
        }

        internal static void RefreshRegisteredGear()
        {
            if (_prefabsReady) Apply();
        }

        private static void BindGear(ConfigFile config, string prefab, string key, string token, string fallbackName,
            string category, bool advanced = false, EffectChoice defaultChoice = EffectChoice.UseDefaults, string? sortFamily = null)
        {
            var open = false;
            var localized = token.Length == 0 ? null : Localization.instance?.Localize(token);
            var displayName = string.IsNullOrWhiteSpace(localized) || localized == token || localized.StartsWith("[", StringComparison.Ordinal)
                ? fallbackName : localized;
            var separator = category.IndexOf(" - ", StringComparison.Ordinal);
            var section = separator >= 0 ? category.Substring(0, separator) : category;
            var entry = config.Bind(section, key, defaultChoice, new ConfigDescription(
                IsWorldObject(prefab) ? "Changes apply to supported loaded world objects immediately."
                    : $"Visual effects for {prefab}. Re-equip after changing. Suppress options override Defaults.", null,
                new ConfigurationManagerAttributes
                {
                    DispName = displayName,
                    Category = category,
                    IsAdvanced = advanced,
                    Order = WeaponOrder(sortFamily ?? prefab),
                    CustomDrawer = setting => DrawGearChoice((ConfigEntry<EffectChoice>)setting, ref open,
                        EmissionRenderers.ContainsKey(prefab) || WorldEmissionRenderers.ContainsKey(prefab) || prefab == DualDyrnwynPrefab,
                        HasLightControl(prefab), HasParticleControl(prefab))
                }));
            GearChoices.Add(prefab, entry);
            entry.SettingChanged += OnSettingChanged;
        }


        private static int WeaponOrder(string prefab)
        {
            if (prefab is JotunPuffs or Magecap or YellowMushroom) return 150;
            if (prefab.StartsWith("Mead", StringComparison.Ordinal) || prefab == "CryptKey") return 140;
            if (prefab.StartsWith("Staff", StringComparison.Ordinal)) return 130;
            if (prefab.StartsWith("Atgeir", StringComparison.Ordinal)) return 120;
            if (prefab.StartsWith("Axe", StringComparison.Ordinal)) return 110;
            if (prefab.StartsWith("Bow", StringComparison.Ordinal)) return 100;
            if (prefab.StartsWith("Crossbow", StringComparison.Ordinal)) return 90;
            if (prefab.StartsWith("Knife", StringComparison.Ordinal)) return 80;
            if (prefab.StartsWith("Battleaxe", StringComparison.Ordinal)) return 70;
            if (prefab.StartsWith("THSword", StringComparison.Ordinal)) return 60;
            if (prefab.StartsWith("Fist", StringComparison.Ordinal)) return 50;
            if (prefab.StartsWith("Mace", StringComparison.Ordinal)) return 40;
            if (prefab.StartsWith("Sledge", StringComparison.Ordinal)) return 30;
            if (prefab.StartsWith("Spear", StringComparison.Ordinal)) return 20;
            return 10;
        }

        private static void DrawGearChoice(ConfigEntry<EffectChoice> entry, ref bool open, bool supportsEmission, bool supportsLight, bool supportsParticles)
        {
            GUILayout.BeginVertical();
            var label = entry.Value switch
            {
                EffectChoice.UseDefaults => "Use defaults",
                EffectChoice.KeepOriginalEffects => "Keep original effects",
                _ => "Custom"
            };
            if (GUILayout.Button((open ? "▼ " : "▶ ") + label)) open = !open;
            if (open)
            {
                if (GUILayout.Button("Use defaults")) entry.Value = EffectChoice.UseDefaults;
                if (GUILayout.Button("Keep original effects")) entry.Value = EffectChoice.KeepOriginalEffects;
                var custom = entry.Value != EffectChoice.UseDefaults && entry.Value != EffectChoice.KeepOriginalEffects;
                if (!custom && GUILayout.Button("Customize effects"))
                    entry.Value = (EffectChoice)((supportsEmission ? 4 : 0) | (supportsLight ? 1 : 0) | (supportsParticles ? 2 : 0));
                if (entry.Value != EffectChoice.UseDefaults && entry.Value != EffectChoice.KeepOriginalEffects)
                {
                    var light = supportsLight && GUILayout.Toggle(HasEffect(entry.Value, EffectChoice.SuppressLight), "Suppress world light");
                    var particles = supportsParticles && GUILayout.Toggle(HasEffect(entry.Value, EffectChoice.SuppressParticles), "Hide particles");
                    var emission = supportsEmission && GUILayout.Toggle(HasEffect(entry.Value, EffectChoice.SuppressMaterialGlow), "Suppress material glow");
                    var selected = (light ? 1 : 0) | (particles ? 2 : 0) | (emission ? 4 : 0);
                    entry.Value = selected == 0 ? EffectChoice.KeepOriginalEffects : (EffectChoice)selected;
                }
            }
            GUILayout.EndVertical();
        }

        private static bool HasEffect(EffectChoice choice, EffectChoice effect) => (choice & effect) != 0;

        private static void OnSettingChanged(object sender, EventArgs args)
        {
            if (_prefabsReady) Apply();
        }

        internal static void Apply()
        {
            _prefabsReady = true;
            Restore();
            var lightNames = ReadNames(_lights.Value);
            var particleNames = ReadNames(_particles.Value);
            var emissionNames = ReadNames(_emission.Value);
            emissionNames.ExceptWith(GearChoices.Keys);
            emissionNames.Remove(LegacyDyrnwynPrefab);
            // Older configs can still list these pickup-only candidates.
            lightNames.Remove("StaffThunderBlood");
            lightNames.Remove("StaffOrbofAhri");
            lightNames.Remove(LegacyDyrnwynPrefab);
            particleNames.Remove("StaffThunderBlood");
            particleNames.Remove("StaffOrbofAhri");
            particleNames.Remove(LegacyDyrnwynPrefab);
            foreach (var pair in GearChoices)
            {
                lightNames.Remove(pair.Key);
                particleNames.Remove(pair.Key);
                var choice = pair.Value.Value;
                if (IsWorldObject(pair.Key)) continue;
                if (HasLightControl(pair.Key) && (HasEffect(choice, EffectChoice.SuppressLight)
                    || (choice == EffectChoice.UseDefaults && _defaultLights.Value))) lightNames.Add(pair.Key);
                if (HasParticleControl(pair.Key) && (HasEffect(choice, EffectChoice.SuppressParticles)
                    || (choice == EffectChoice.UseDefaults && _defaultParticles.Value))) particleNames.Add(pair.Key);
            }
            if (lightNames.Contains(DyrnwynPrefab)) lightNames.Add(LegacyDyrnwynPrefab);
            if (particleNames.Contains(DyrnwynPrefab)) particleNames.Add(LegacyDyrnwynPrefab);
            foreach (var name in emissionNames)
            {
                var prefab = GetGear(name);
                if (prefab != null) SuppressEmission(prefab, name, null);
            }
            foreach (var rule in EmissionRenderers)
            {
                var choice = GearChoices[rule.Key].Value;
                if (!HasEffect(choice, EffectChoice.SuppressMaterialGlow)
                    && (choice != EffectChoice.UseDefaults || !_defaultEmission.Value)) continue;
                SuppressEmission(rule.Key, rule.Value);
                if (rule.Key == DyrnwynPrefab) SuppressEmission(LegacyDyrnwynPrefab, "Viking_Sword (1)");
            }
            if (GearChoices.TryGetValue(DualDyrnwynPrefab, out var dualDyrnwyn)
                && (HasEffect(dualDyrnwyn.Value, EffectChoice.SuppressMaterialGlow)
                    || (dualDyrnwyn.Value == EffectChoice.UseDefaults && _defaultEmission.Value)))
            {
                var prefab = GetGear(DualDyrnwynPrefab);
                if (prefab != null)
                {
                    var before = OriginalMaterials.Count;
                    SuppressEmission(prefab, DualDyrnwynPrefab, null, DyrnwynBladeMaterial);
                    if (VerboseLoggingEnabled)
                        _logger.LogInfo($"Dual Dyrnwyn: suppressed material glow on {OriginalMaterials.Count - before} renderer(s).");
                }
            }
            foreach (var name in lightNames)
            {
                var prefab = GetGear(name);
                if (prefab == null) continue;
                var controllersChanged = 0;
                foreach (var controller in prefab.GetComponentsInChildren<LightLod>(true))
                {
                    if (!IsGearEffect(name, controller, prefab)) continue;
                    if (DisableLightController(controller)) controllersChanged++;
                }
                foreach (var controller in prefab.GetComponentsInChildren<LightFlicker>(true))
                {
                    if (!IsGearEffect(name, controller, prefab)) continue;
                    if (DisableLightController(controller)) controllersChanged++;
                }
                var changed = 0;
                foreach (var light in prefab.GetComponentsInChildren<Light>(true))
                {
                    if (!IsGearEffect(name, light, prefab)) continue;
                    if (!light.enabled || !DisabledLights.Add(light)) continue;
                    light.enabled = false;
                    changed++;
                }
                var particleLightsChanged = 0;
                foreach (var particles in prefab.GetComponentsInChildren<ParticleSystem>(true))
                {
                    if (!IsGearEffect(name, particles, prefab)) continue;
                    var lights = particles.lights;
                    if (!lights.enabled || !DisabledParticleLights.Add(particles)) continue;
                    lights.enabled = false;
                    particleLightsChanged++;
                }
                if (VerboseLoggingEnabled)
                    _logger.LogInfo($"'{name}': disabled {changed} light(s), {controllersChanged} light controller(s), and {particleLightsChanged} particle light module(s).");
            }

            foreach (var name in particleNames)
            {
                var prefab = GetGear(name);
                if (prefab == null) continue;
                var changed = 0;
                foreach (var renderer in prefab.GetComponentsInChildren<ParticleSystemRenderer>(true))
                {
                    if (!IsGearEffect(name, renderer, prefab)) continue;
                    if (name == "DragonTear" && renderer.name is not ("flare" or "smoke_expl" or "pixel flakes")) continue;
                    if (name is "CryptKey" or "BloodGoldKey" && renderer.gameObject == prefab) continue;
                    if (!renderer.enabled || !DisabledParticles.Add(renderer)) continue;
                    renderer.enabled = false;
                    changed++;
                }
                if (VerboseLoggingEnabled)
                    _logger.LogInfo($"'{name}': hid {changed} particle renderer(s).");
            }
            foreach (var transform in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                ApplyWorldEffects(transform.gameObject);
                if (_hideForgeGlow?.Value == true) HideForgeGlow(transform);
            }
        }

        private static void RefreshForgeGlow()
        {
            if (_hideForgeGlow?.Value != true)
            {
                RestoreForgeGlow();
                return;
            }
            foreach (var transform in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                HideForgeGlow(transform);
        }

        private static void HideForgeGlow(Transform node)
        {
            if (!IsForgeGlow(node) || !node.gameObject.activeSelf) return;
            HiddenForgeGlows.RemoveWhere(glow => glow == null);
            HiddenForgeGlows.Add(node.gameObject);
            node.gameObject.SetActive(false);
        }

        private static bool IsForgeGlow(Transform node)
            => node.name.Equals(ForgeGlowName, StringComparison.OrdinalIgnoreCase)
                || node.name.Equals(ForgeGlowName + "(Clone)", StringComparison.OrdinalIgnoreCase);

        private static void RestoreForgeGlow()
        {
            foreach (var glow in HiddenForgeGlows)
            {
                if (glow != null) glow.SetActive(true);
            }
            HiddenForgeGlows.Clear();
        }

        private static void HideAttachedForgeGlow(GameObject root)
        {
            if (_hideForgeGlow?.Value != true) return;
            foreach (var node in root.GetComponentsInChildren<Transform>(true))
                HideForgeGlow(node);
        }

        [HarmonyPatch]
        private static class HideEquippedForgeGlow
        {
            private static IEnumerable<MethodBase> TargetMethods()
            {
                foreach (var method in AccessTools.GetDeclaredMethods(typeof(VisEquipment)))
                {
                    if (method.Name is "AttachItem" or "AttachArmor") yield return method;
                }
            }

            private static void Postfix(VisEquipment __instance) => HideAttachedForgeGlow(__instance.gameObject);
        }

        [HarmonyPatch(typeof(ZNetView), "Awake")]
        private static class ApplyVisualsToSpawnedWorldObject
        {
            private static bool Prepare() => AccessTools.Method(typeof(ZNetView), "Awake") != null;

            private static void Postfix(ZNetView __instance)
            {
                if (!_prefabsReady || !__instance.gameObject.scene.IsValid()) return;
                ApplyWorldEffects(__instance.gameObject);
                if (_hideForgeGlow?.Value == true && __instance.TryGetComponent<ItemDrop>(out _))
                    HideAttachedForgeGlow(__instance.gameObject);
            }
        }

        private static void ApplyWorldEffects(GameObject root)
        {
            var name = root.name;
            if (name.EndsWith("(Clone)", StringComparison.Ordinal)) name = name.Substring(0, name.Length - 7);
            if (name == "MushroomYellow") name = YellowMushroom;
            if (!IsWorldObject(name) || !root.scene.IsValid()) return;
            var choice = GearChoices[name].Value;
            var hideLight = HasEffect(choice, EffectChoice.SuppressLight) || (choice == EffectChoice.UseDefaults && _defaultLights.Value);
            var hideParticles = HasParticleControl(name)
                && (HasEffect(choice, EffectChoice.SuppressParticles) || (choice == EffectChoice.UseDefaults && _defaultParticles.Value));
            if (WorldEmissionRenderers.TryGetValue(name, out var rendererName)
                && (HasEffect(choice, EffectChoice.SuppressMaterialGlow) || (choice == EffectChoice.UseDefaults && _defaultEmission.Value)))
                SuppressEmission(root, name, rendererName);
            if (!hideLight && !hideParticles) return;
            DisabledLights.RemoveWhere(light => light == null);
            DisabledLightControllers.RemoveWhere(controller => controller == null);
            DisabledParticles.RemoveWhere(renderer => renderer == null);

            if (hideLight)
            {
                foreach (var light in root.GetComponentsInChildren<Light>(true))
                {
                    if (!IsWorldEffect(light, root, name, "Point Light")) continue;
                    var lod = light.GetComponent<LightLod>();
                    if (lod != null) DisableLightController(lod);
                    var flicker = light.GetComponent<LightFlicker>();
                    if (flicker != null) DisableLightController(flicker);
                    if (light.enabled && DisabledLights.Add(light)) light.enabled = false;
                }
            }
            if (hideParticles)
            {
                foreach (var renderer in root.GetComponentsInChildren<ParticleSystemRenderer>(true))
                {
                    var matches = name == Thistle
                        ? renderer.name.Equals("flare", StringComparison.OrdinalIgnoreCase) || renderer.name.Equals("bees", StringComparison.OrdinalIgnoreCase)
                        : IsWorldEffect(renderer, root, name, "Particle System");
                    if (matches
                        && renderer.enabled && DisabledParticles.Add(renderer)) renderer.enabled = false;
                }
            }
        }

        private static bool IsWorldEffect(Component effect, GameObject root, string prefab, string plantTarget)
        {
            if (prefab == ArtisanTable)
            {
                if (!effect.name.Equals("Point Light", StringComparison.OrdinalIgnoreCase)
                    && !effect.name.Equals("flare", StringComparison.OrdinalIgnoreCase)
                    && !effect.name.Equals("smoke_expl", StringComparison.OrdinalIgnoreCase)
                    && !effect.name.Equals("pixel flakes", StringComparison.OrdinalIgnoreCase)) return false;
            }
            else if (prefab != GaldrTable) return effect.name.Equals(plantTarget, StringComparison.OrdinalIgnoreCase);
            var effectParent = prefab == ArtisanTable ? "Tear" : "Effects";
            for (var node = effect.transform; node != null && node != root.transform; node = node.parent)
            {
                if (node.name.Equals(effectParent, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private static void SuppressEmission(string prefabName, string rendererName)
        {
            var prefab = PrefabManager.Instance.GetPrefab(prefabName);
            if (prefab != null) SuppressEmission(prefab, prefabName, rendererName);
        }

        private static void SuppressEmission(GameObject prefab, string prefabName, string? rendererName, string? materialName = null)
        {
            foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                if (rendererName != null && !renderer.name.Equals(rendererName, StringComparison.OrdinalIgnoreCase)) continue;
                if (rendererName == null && (renderer is ParticleSystemRenderer || IsPickupOrUpgradeEffect(renderer, prefab))) continue;
                if (materialName != null && !IsAttachedArmoryEffect(renderer, prefab)) continue;
                if (OriginalMaterials.ContainsKey(renderer)) continue;
                var originals = renderer.sharedMaterials;
                var replacements = (Material[])originals.Clone();
                var changed = false;
                for (var i = 0; i < originals.Length; i++)
                {
                    var material = originals[i];
                    if (material == null) continue;
                    if (materialName != null && !material.name.Equals(materialName, StringComparison.OrdinalIgnoreCase)
                        && !material.name.Equals(materialName + " (Instance)", StringComparison.OrdinalIgnoreCase)) continue;
                    var keyword = material.IsKeywordEnabled("_EMISSION");
                    var skullEmission = prefabName == "StaffSkeleton" && material.name.StartsWith("SkeletonItem_mat", StringComparison.Ordinal)
                        && material.HasProperty("_EmissionColor");
                    var axeEmission = prefabName == "BattleaxeSkullSplittur"
                        && material.name.StartsWith("battleaxe_mistlands_mat", StringComparison.Ordinal)
                        && material.HasProperty("_EmissionColor");
                    var customCreature = rendererName == null && material.shader != null && material.shader.name == "Custom/Creature"
                        && material.HasProperty("_EmissionColor") && material.GetColor("_EmissionColor").maxColorComponent > 0f;
                    if (!keyword && !skullEmission && !axeEmission && !customCreature
                        && !(materialName != null && material.HasProperty("_EmissionColor"))) continue;
                    if (!MutedMaterials.TryGetValue(material, out var muted))
                    {
                        muted = new Material(material);
                        muted.DisableKeyword("_EMISSION");
                        if (muted.HasProperty("_EmissionColor")) muted.SetColor("_EmissionColor", Color.black);
                        MutedMaterials.Add(material, muted);
                    }
                    replacements[i] = muted;
                    changed = true;
                }
                if (!changed) continue;
                OriginalMaterials.Add(renderer, originals);
                renderer.sharedMaterials = replacements;
            }
        }

        private static bool IsPickupOrUpgradeEffect(Component component, GameObject prefab)
        {
            for (var node = component.transform; node != null && node != prefab.transform; node = node.parent)
            {
                if (node.name.Equals("fx_ItemSparkles", StringComparison.OrdinalIgnoreCase)
                    || IsForgeGlow(node))
                    return true;
            }
            return false;
        }

        private static bool IsGearEffect(string name, Component effect, GameObject root)
            => ArmoryEffects.ContainsKey(name) ? IsAttachedArmoryEffect(effect, root) : !IsPickupOrUpgradeEffect(effect, root);

        private static bool IsAttachedArmoryEffect(Component effect, GameObject root)
        {
            if (IsPickupOrUpgradeEffect(effect, root)) return false;
            for (var node = effect.transform; node != null && node != root.transform; node = node.parent)
            {
                if (node.parent == root.transform)
                    return node.name is "attach" or "attach_back" or "attach_skin";
            }
            return false;
        }

        internal static void Restore()
        {
            foreach (var pair in OriginalMaterials)
            {
                if (pair.Key != null) pair.Key.sharedMaterials = pair.Value;
            }
            OriginalMaterials.Clear();
            foreach (var light in DisabledLights)
            {
                if (light != null) light.enabled = true;
            }
            foreach (var renderer in DisabledParticles)
            {
                if (renderer != null) renderer.enabled = true;
            }
            foreach (var particles in DisabledParticleLights)
            {
                if (particles == null) continue;
                var lights = particles.lights;
                lights.enabled = true;
            }
            foreach (var controller in DisabledLightControllers)
            {
                if (controller != null) controller.enabled = true;
            }
            DisabledLights.Clear();
            DisabledLightControllers.Clear();
            DisabledParticleLights.Clear();
            DisabledParticles.Clear();
        }

        internal static void Shutdown()
        {
            _prefabsReady = false;
            RestoreForgeGlow();
            Restore();
            foreach (var material in MutedMaterials.Values) UnityEngine.Object.Destroy(material);
            MutedMaterials.Clear();
        }

        private static bool DisableLightController(Behaviour controller)
        {
            if (!controller.enabled || !DisabledLightControllers.Add(controller)) return false;
            controller.enabled = false;
            return true;
        }

        private static HashSet<string> ReadNames(string value)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in value.Split(','))
            {
                var name = entry.Trim();
                if (name.Length > 0) names.Add(name);
            }
            return names;
        }

        private static GameObject? GetGear(string name)
        {
            if (ArmoryEffects.ContainsKey(name))
            {
                var item = ObjectDB.instance?.GetItemPrefab(name);
                if (item != null && item.GetComponent<ItemDrop>() != null) return item;
            }
            var prefab = PrefabManager.Instance.GetPrefab(name);
            if (prefab != null && prefab.GetComponent<ItemDrop>() != null) return prefab;
            prefab = ObjectDB.instance?.GetItemPrefab(name);
            if (prefab != null && prefab.GetComponent<ItemDrop>() != null) return prefab;
            _logger.LogWarning($"Extra gear effects: '{name}' did not resolve to an item prefab.");
            return null;
        }
    }
}
