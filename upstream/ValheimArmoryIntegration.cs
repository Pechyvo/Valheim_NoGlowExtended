using BepInEx;
using BepInEx.Bootstrap;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.Rendering;

namespace A2.NoGlow
{
    [BepInPlugin(PluginInfo.PluginGUID + ".ValheimArmory", PluginInfo.PluginName + " - ValheimArmory", PluginInfo.PluginSemanticVersion)]
    [BepInDependency(PluginInfo.PluginGUID)]
    [BepInDependency(ArmoryGuid, BepInDependency.DependencyFlags.SoftDependency)]
    internal sealed class ValheimArmoryIntegration : BaseUnityPlugin
    {
        private const string ArmoryGuid = "MidnightsFX.ValheimArmory";
        private const string TestPrefab = "VADual_SwordGold_BloodLightning";

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
            var prefab = ObjectDB.instance?.GetItemPrefab(TestPrefab) ?? PrefabManager.Instance.GetPrefab(TestPrefab);
            if (prefab == null || !prefab.TryGetComponent<ItemDrop>(out var item)) return;

            NoGlowExpanded.RegisterArmoryGear(Config, TestPrefab, item.m_itemData.m_shared.m_name,
                "Dual Thunderblood Sword", "Deep North");
        }
    }
}
