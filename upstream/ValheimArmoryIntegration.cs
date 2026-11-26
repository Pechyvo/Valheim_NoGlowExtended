using BepInEx;
using BepInEx.Bootstrap;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pechyvo.NoGlowExpanded
{
    [BepInPlugin(PluginInfo.PluginGUID + ".ValheimArmory", PluginInfo.PluginName + " - ValheimArmory", PluginInfo.PluginSemanticVersion)]
    [BepInDependency(PluginInfo.PluginGUID)]
    [BepInDependency(ArmoryGuid, BepInDependency.DependencyFlags.SoftDependency)]
    internal sealed class ValheimArmoryIntegration : BaseUnityPlugin
    {
        private const string ArmoryGuid = "MidnightsFX.ValheimArmory";
        private int _lastLoggedEligible = -1;
        private static readonly (string Prefab, string Name, string Category, string WeaponType)[] Weapons =
        {
            ("VAatgeir_antler", "Antler Atgeir", "Meadows - Weapons", "Atgeir"),
            ("VAAtgeir_Flint", "Flint Atgeir", "Meadows - Weapons", "Atgeir"),
            ("VAFlint_Axe", "Flint Axe", "Meadows - Weapons", "Axe"),
            ("VAFlint_dualaxes", "Flint Dualaxes", "Meadows - Weapons", "Axe"),
            ("VAAntler_Bow", "Antler Bow", "Meadows - Weapons", "Bow"),
            ("VACrossbowWood", "Wood Crossbow", "Meadows - Weapons", "Crossbow"),
            ("VAAntler_dagger", "Antler Dagger", "Meadows - Weapons", "Knife"),
            ("VADagger_Flint_2h", "Flint Knives", "Meadows - Weapons", "Knife"),
            ("VAAntler_greataxe", "Antler Greataxe", "Meadows - Weapons", "Battleaxe"),
            ("VAFlint_greataxe", "Flint Greataxe", "Meadows - Weapons", "Battleaxe"),
            ("VAFlint_Greatsword", "Flint Greatsword", "Meadows - Weapons", "THSword"),
            ("VAFist_Flint", "Flint Knuckles", "Meadows - Weapons", "Fist"),
            ("VAFlintMace", "Flint Mace", "Meadows - Weapons", "Mace"),
            ("VADual_Club", "Dual Club", "Meadows - Weapons", "Mace"),
            ("VADual_VAFlintMace", "Dual Flint Mace", "Meadows - Weapons", "Mace"),
            ("VASpearFlint", "Flint Spear", "Meadows - Weapons", "Spear"),
            ("VAAntler_Sword", "Antler Sword", "Meadows - Weapons", "Sword"),
            ("VADual_VAAntler_Sword", "Dual Antler Sword", "Meadows - Weapons", "Sword"),
            ("VAFlint_Sword", "Flint Sword", "Meadows - Weapons", "Sword"),
            ("VADual_SwordWood", "Dual Wooden Sword", "Meadows - Weapons", "Sword"),
            ("VADual_VAFlint_Sword", "Dual Flint Sword", "Meadows - Weapons", "Sword"),
            ("VABronze_dualaxes", "Bronze Dualaxes", "Black Forest - Weapons", "Axe"),
            ("VACrossbowElder", "Vine Crossbow", "Black Forest - Weapons", "Crossbow"),
            ("VACrossbowBronze", "Bronze Crossbow", "Black Forest - Weapons", "Crossbow"),
            ("VAdagger_copper_2h", "Copper Knives", "Black Forest - Weapons", "Knife"),
            ("VAbronze_battleaxe", "Bronze Lumber Axe", "Black Forest - Weapons", "Battleaxe"),
            ("VAbronze_greatsword", "Bronze Greatsword", "Black Forest - Weapons", "THSword"),
            ("VAFist_Bronze", "Bronze Knuckles", "Black Forest - Weapons", "Fist"),
            ("VAElder_mace", "Vine Mace", "Black Forest - Weapons", "Mace"),
            ("VADual_VAElder_mace", "Dual Vine Mace", "Black Forest - Weapons", "Mace"),
            ("VADual_MaceBronze", "Dual Bronze Mace", "Black Forest - Weapons", "Mace"),
            ("VAElderHammer", "Vine Hammer", "Black Forest - Weapons", "Sledge"),
            ("VABronzeSledge", "Bronze Sledge", "Black Forest - Weapons", "Sledge"),
            ("VAVine_Sword", "Vine Sword", "Black Forest - Weapons", "Sword"),
            ("VADual_VAVine_Sword", "Dual Vine Sword", "Black Forest - Weapons", "Sword"),
            ("VASwordChitin", "Chitin Sword", "Black Forest - Weapons", "Sword"),
            ("VADual_SwordBronze", "Dual Bronze Sword", "Black Forest - Weapons", "Sword"),
            ("VADual_VASwordChitin", "Dual Abyssal Sword", "Black Forest - Weapons", "Sword"),
            ("VAElderRoundShield", "Vine Shield", "Black Forest - Weapons", "Shield"),
            ("VABlood_Bones_pickaxe", "Blood Bone Battlepick", "Black Forest - Weapons", "Pickaxe"),
            ("VABone_axe", "Bonerot Axe", "Swamp - Weapons", "Axe"),
            ("VABone_dualaxes", "Bonerot Dualaxes", "Swamp - Weapons", "Axe"),
            ("VAIron_dualaxes", "Iron Dualaxes", "Swamp - Weapons", "Axe"),
            ("VACrossbowIron", "Iron Crossbow", "Swamp - Weapons", "Crossbow"),
            ("VABonemassDagger", "Bonerot Knife", "Swamp - Weapons", "Knife"),
            ("VAdagger_iron", "Iron Knife", "Swamp - Weapons", "Knife"),
            ("VAdagger_iron_2h", "Iron Knives", "Swamp - Weapons", "Knife"),
            ("VAdagger_chitin_2h", "Chitin Knives", "Swamp - Weapons", "Knife"),
            ("VABonemassGreatsword", "Bonerot Greatsword", "Swamp - Weapons", "THSword"),
            ("VAiron_greatsword", "Iron Greatsword", "Swamp - Weapons", "THSword"),
            ("VAFist_Iron", "Iron Knuckles", "Swamp - Weapons", "Fist"),
            ("VADual_MaceIron", "Dual Iron Mace", "Swamp - Weapons", "Mace"),
            ("VABonemassWarhammer", "Bonerot Sledge", "Swamp - Weapons", "Sledge"),
            ("VABonemassSword", "Bonerot Sword", "Swamp - Weapons", "Sword"),
            ("VADual_VABonemassSword", "Dual Bonerot Sword", "Swamp - Weapons", "Sword"),
            ("VADual_SwordIron", "Dual Iron Sword", "Swamp - Weapons", "Sword"),
            ("VAserpent_buckler", "Serpent Scale Buckler", "Swamp - Weapons", "Shield"),
            ("VAStaff_Druid_Fire", "Druidic Staff of Fire", "Swamp - Staves", "Staff"),
            ("VAStaff_Druid_Poison", "Druidic Staff of Poison", "Swamp - Staves", "Staff"),
            ("VAStaff_Druid_Spirit", "Druidic Staff of Spirit", "Swamp - Staves", "Staff"),
            ("VAAtgeirChitin", "Royal Abyssal Atgeir", "Mountains - Weapons", "Atgeir"),
            ("VASilverAtgeir", "Silver Atgeir", "Mountains - Weapons", "Atgeir"),
            ("VAModer_Axe", "Dragonfrost Axe", "Mountains - Weapons", "Axe"),
            ("VAModer_dualaxes", "Dragonfrost Dual Axes", "Mountains - Weapons", "Axe"),
            ("VAcrystal_axe", "Silver Axe", "Mountains - Weapons", "Axe"),
            ("VACrystal_dualaxes", "Crystal Dualaxes", "Mountains - Weapons", "Axe"),
            ("VABlood_bone_bow", "Blood Bone Bow", "Mountains - Weapons", "Bow"),
            ("VACrossbowModerNew", "Dragonfrost Crossbow", "Mountains - Weapons", "Crossbow"),
            ("VACrossbowModer", "Dragonfrost Crossbow (Legacy)", "Mountains - Weapons", "Crossbow"),
            ("VACrossbowSilver", "Draugr Claw", "Mountains - Weapons", "Crossbow"),
            ("VAdagger_moder", "Dragonfrost Knife", "Mountains - Weapons", "Knife"),
            ("VAdagger_moder_2h", "Dragonfrost Dual Knives", "Mountains - Weapons", "Knife"),
            ("VAdagger_silver_2h", "Silver Knives", "Mountains - Weapons", "Knife"),
            ("VAModer_greatsword", "Dragonfrost Greatsword", "Mountains - Weapons", "THSword"),
            ("VAsilver_greatsword", "Silver Greatsword", "Mountains - Weapons", "THSword"),
            ("VADual_MaceSilver", "Dual Frostner", "Mountains - Weapons", "Mace"),
            ("VASilverSledge", "Silver Sledge", "Mountains - Weapons", "Sledge"),
            ("VASpearModer", "Dragonfrost Spear", "Mountains - Weapons", "Spear"),
            ("VASwordModer", "Dragonfrost Sword", "Mountains - Weapons", "Sword"),
            ("VADual_VASwordModer", "Dual Dragonfrost Sword", "Mountains - Weapons", "Sword"),
            ("VADual_SwordSilver", "Dual Silver Sword", "Mountains - Weapons", "Sword"),
            ("VAModer_shield", "Dragonfrost Shield", "Mountains - Weapons", "Shield"),
            ("VAModer_RoundShield", "Dragonfrost Buckler", "Mountains - Weapons", "Shield"),
            ("VAsilver_tower", "Silver Tower Shield", "Mountains - Weapons", "Shield"),
            ("VAStaff_Druid_Ice", "Druidic Staff of Ice", "Mountains - Staves", "Staff"),
            ("VAYagluthAtgeir", "Goblin King Atgeir", "Plains - Weapons", "Atgeir"),
            ("VABlackmetal_dualaxes", "Blackmetal Dualaxes", "Plains - Weapons", "Axe"),
            ("VABlackmetal_bow", "Blackmetal Bow", "Plains - Weapons", "Bow"),
            ("VACrossbowBlackmetal", "Blackmetal Crossbow", "Plains - Weapons", "Crossbow"),
            ("VAArbalistBronze", "Bronze Arbalist", "Plains - Weapons", "Crossbow"),
            ("VAknife_blackmetal", "Blackmetal Knives", "Plains - Weapons", "Knife"),
            ("VAblackmetal_2h_axe", "Blackmetal Greataxe", "Plains - Weapons", "Battleaxe"),
            ("VAblackmetal_battleaxe", "Herkirs Wrath", "Plains - Weapons", "Battleaxe"),
            ("VAYagluth_greatsword", "Goblin King Greatsword", "Plains - Weapons", "THSword"),
            ("VABlackmetal_greatsword", "Blackmetal Greatsword", "Plains - Weapons", "THSword"),
            ("VAFist_Yagluth", "Goblin King Knuckles", "Plains - Weapons", "Fist"),
            ("VADual_MaceNeedle", "Dual Porcupine", "Plains - Weapons", "Mace"),
            ("VAblackmetal_sledge", "Skyshatter", "Plains - Weapons", "Sledge"),
            ("VASpearBlackmetal", "Blackmetal Spear", "Plains - Weapons", "Spear"),
            ("VADual_SwordBlackmetal", "Dual Black Metal Sword", "Plains - Weapons", "Sword"),
            ("VAJotunn_single_axe", "Jotuns Bane (Half)", "Mistlands - Weapons", "Axe"),
            ("VAJotunn_dualaxes", "Jotun Dualaxes", "Mistlands - Weapons", "Axe"),
            ("VAHeavy_Blood_Bone_Bow", "Carapace Blood Bow", "Mistlands - Weapons", "Bow"),
            ("VAQueen_bow", "Pincer Greatbow", "Mistlands - Weapons", "Bow"),
            ("VAdagger_queen", "Carapace Dagger", "Mistlands - Weapons", "Knife"),
            ("VAdagger_blackmetal_mistlands", "Hati Knife", "Mistlands - Weapons", "Knife"),
            ("VAJotunn_2h_axe", "Jotuns Greataxe", "Mistlands - Weapons", "Battleaxe"),
            ("VAQueen_greatsword", "Pincer Greatsword", "Mistlands - Weapons", "THSword"),
            ("VAmistland_mace", "Blackmarble Mace", "Mistlands - Weapons", "Mace"),
            ("VADual_VAmistland_mace", "Dual Blackmarble Mace", "Mistlands - Weapons", "Mace"),
            ("VADual_SwordMistwalker", "Dual Mistwalker", "Mistlands - Weapons", "Sword"),
            ("VADual_VASwordQueen", "Dual Carapace Sword", "Mistlands - Weapons", "Sword"),
            ("VASwordQueen", "Carapace Sword", "Mistlands - Weapons", "Sword"),
            ("VAdverger_tower", "Dverger Towershield", "Mistlands - Weapons", "Shield"),
            ("VAStaff_Poison", "Staff of Poison", "Mistlands - Staves", "Staff"),
            ("VAStaff_Spirit", "Staff of Spirit", "Mistlands - Staves", "Staff"),
            ("VAMeteorAtgeir", "Flametal Atgeir", "Ashlands - Weapons", "Atgeir"),
            ("VAFlametal_Axe", "Flametal Axe", "Ashlands - Weapons", "Axe"),
            ("VASoulStealer", "Soulstealer", "Ashlands - Weapons", "Knife"),
            ("VAdagger_meteor_2h", "Assassin's Knives", "Ashlands - Weapons", "Knife"),
            ("VAdagger_meteor", "Flametal Knife", "Ashlands - Weapons", "Knife"),
            ("VAFlametalAxe_2h", "Flametal Battleaxe", "Ashlands - Weapons", "Battleaxe"),
            ("VAGreatswordFader", "Dreadfire Greatsword", "Ashlands - Weapons", "THSword"),
            ("VADual_MaceEldner", "Dual Flametal Mace", "Ashlands - Weapons", "Mace"),
            ("VAflametal_sledge", "Flametal Sledge", "Ashlands - Weapons", "Sledge"),
            ("VASpearFader", "Dreadfire Spear", "Ashlands - Weapons", "Spear"),
            ("VASwordFader", "Dreadfire Sword", "Ashlands - Weapons", "Sword"),
            ("VADual_VASwordFader", "Dual Dreadfire Sword", "Ashlands - Weapons", "Sword"),
            ("VADual_SwordDyrnwyn", "Dual Dyrnwyn", "Ashlands - Weapons", "Sword"),
            ("VADual_SwordNiedhogg", "Dual NidhöGg", "Ashlands - Weapons", "Sword"),
            ("VAMeteorAtgeir_blood", "Flametal Blood Atgeir", "Ashlands - Blood", "Atgeir"),
            ("VAFlametal_Axe_Blood", "Flametal Blood Axe", "Ashlands - Blood", "Axe"),
            ("VAdagger_meteor_2h_blood", "Assassin's Blood Knives", "Ashlands - Blood", "Knife"),
            ("VAdagger_meteor_blood", "Flametal Blood Knife", "Ashlands - Blood", "Knife"),
            ("VAFlametalAxe_blood_2h", "Flametal Blood Battleaxe", "Ashlands - Blood", "Battleaxe"),
            ("VADual_MaceEldnerBlood", "Dual Bloodgeon", "Ashlands - Blood", "Mace"),
            ("VAflametal_sledge_blood", "Deathblow", "Ashlands - Blood", "Sledge"),
            ("VADual_SwordNiedhoggBlood", "Dual Nidhögg the Bleeding", "Ashlands - Blood", "Sword"),
            ("VAMeteorAtgeir_lightning", "Flametal Lightning Atgeir", "Ashlands - Lightning", "Atgeir"),
            ("VAFlametal_Axe_Lightning", "Flametal Lightning Axe", "Ashlands - Lightning", "Axe"),
            ("VAdagger_meteor_2h_lightning", "Assassin's Lightning Knives", "Ashlands - Lightning", "Knife"),
            ("VAdagger_meteor_lightning", "Flametal Lightning Knife", "Ashlands - Lightning", "Knife"),
            ("VAFlametalAxe_lightning_2h", "Flametal Lightning Battleaxe", "Ashlands - Lightning", "Battleaxe"),
            ("VADual_MaceEldnerLightning", "Dual Storm Star", "Ashlands - Lightning", "Mace"),
            ("VAflametal_sledge_lightning", "Thundermaker", "Ashlands - Lightning", "Sledge"),
            ("VADual_SwordNiedhoggLightning", "Dual Nidhögg the Thundering", "Ashlands - Lightning", "Sword"),
            ("VAMeteorAtgeir_nature", "Flametal Primal Atgeir", "Ashlands - Nature", "Atgeir"),
            ("VAFlametal_Axe_Primal", "Flametal Primal Axe", "Ashlands - Nature", "Axe"),
            ("VAdagger_meteor_2h_nature", "Assassin's Primal Knives", "Ashlands - Nature", "Knife"),
            ("VAdagger_meteor_nature", "Flametal Primal Knife", "Ashlands - Nature", "Knife"),
            ("VAFlametalAxe_primal_2h", "Flametal Primal Battleaxe", "Ashlands - Nature", "Battleaxe"),
            ("VADual_MaceEldnerNature", "Dual Klossen", "Ashlands - Nature", "Mace"),
            ("VAflametal_sledge_nature", "Earthshaker", "Ashlands - Nature", "Sledge"),
            ("VADual_SwordNiedhoggNature", "Dual Nidhögg the Primal", "Ashlands - Nature", "Sword"),
            ("VAGold_dualaxes", "Nord Dualaxes", "Deep North - Weapons", "Axe"),
            ("VAknife_gold", "Nord Daggers", "Deep North - Weapons", "Knife"),
            ("VADual_MaceGold", "Dual Nord Mace", "Deep North - Weapons", "Mace"),
            ("VADual_SwordGold", "Dual Nord Sword", "Deep North - Weapons", "Sword"),
            ("VAGold_dualaxes_thunderblood", "Thunderblood Dualaxes", "Deep North - Thunderblood", "Axe"),
            ("VAknife_gold_thunderblood", "Thunderblood Daggers", "Deep North - Thunderblood", "Knife"),
            ("VADual_MaceGold_BloodLightning", "Dual Thunderblood Mace", "Deep North - Thunderblood", "Mace"),
            ("VADual_SwordGold_BloodLightning", "Dual Thunderblood Sword", "Deep North - Thunderblood", "Sword"),
            ("VAGold_dualaxes_frostfire", "Frostfire Dualaxes", "Deep North - Frostfire", "Axe"),
            ("VAknife_gold_frostfire", "Frostfire Daggers", "Deep North - Frostfire", "Knife"),
            ("VADual_MaceGold_FrostFire", "Dual Frostfire Mace", "Deep North - Frostfire", "Mace"),
            ("VADual_SwordGold_FrostFire", "Dual Frostfire Sword", "Deep North - Frostfire", "Sword")
        };

        private void Awake()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null
                || !Chainloader.PluginInfos.TryGetValue(ArmoryGuid, out var armory)
                || armory.Instance == null) return;

            ItemManager.OnItemsRegistered += RegisterGear;
        }

        private void OnDestroy() => ItemManager.OnItemsRegistered -= RegisterGear;

        private void RegisterGear()
        {
            var added = 0;
            var eligible = 0;
            foreach (var weapon in Weapons)
            {
                var prefab = ObjectDB.instance?.GetItemPrefab(weapon.Prefab);
                if (prefab == null) prefab = PrefabManager.Instance.GetPrefab(weapon.Prefab);
                if (prefab == null)
                {
                    LogSkipped(weapon.Prefab, "prefab was not registered");
                    continue;
                }
                if (!prefab.TryGetComponent<ItemDrop>(out var item))
                {
                    LogSkipped(weapon.Prefab, "prefab has no ItemDrop component");
                    continue;
                }
                var result = NoGlowExpanded.RegisterArmoryGear(Config, weapon.Prefab, prefab,
                    item.m_itemData.m_shared.m_name, weapon.Name, weapon.Category, weapon.WeaponType);
                if (result == NoGlowExpanded.ArmoryRegistration.NoSupportedEffects)
                {
                    LogSkipped(weapon.Prefab, "no supported lights or particle renderers on the attached model");
                    continue;
                }
                eligible++;
                if (result == NoGlowExpanded.ArmoryRegistration.Added) added++;
            }
            if (NoGlowExpanded.BasicLoggingEnabled && (added > 0 || eligible != _lastLoggedEligible))
            {
                Logger.LogInfo($"ValheimArmory: {eligible}/{Weapons.Length} candidates have visual settings; {Weapons.Length - eligible} skipped.");
                _lastLoggedEligible = eligible;
            }
            if (eligible > 0) NoGlowExpanded.RefreshRegisteredGear();
        }

        private void LogSkipped(string prefab, string reason)
        {
            if (NoGlowExpanded.VerboseLoggingEnabled)
                Logger.LogInfo($"Skipped '{prefab}': {reason}.");
        }
    }
}
