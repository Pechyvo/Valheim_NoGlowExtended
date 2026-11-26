using BepInEx;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace A2.NoGlow
{
    [BepInPlugin(PluginInfo.PluginGUID, PluginInfo.PluginName, PluginInfo.PluginSemanticVersion)]
    [BepInDependency(Jotunn.Main.ModGuid, BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.NotEnforced, VersionStrictness.None)]
    internal class Plugin : BaseUnityPlugin
    {
        private static readonly Harmony _harmony = new(PluginInfo.PluginGUID);
        private static bool _isInitialized = false;

        public void Awake()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                // do not run on dedicated server
                return;
            }
            if (_isInitialized)
            {
                return;
            }
            _isInitialized = true;

            _harmony.PatchAll();
            NoGlowExpanded.Bind(Config, Logger);
            PrefabManager.OnVanillaPrefabsAvailable += OnVanillaPrefabsAvailable;
        }

        public void OnDestroy()
        {
            if (!_isInitialized)
            {
                return;
            }
            _isInitialized = false;

            PrefabManager.OnVanillaPrefabsAvailable -= OnVanillaPrefabsAvailable;
            NoGlowExpanded.Shutdown();
            _harmony.UnpatchSelf();
        }

        private static void OnVanillaPrefabsAvailable()
        {
            NoGlowExpanded.Apply();
        }
    }
}
