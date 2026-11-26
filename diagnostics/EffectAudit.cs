using System;
using System.IO;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace NoGlowDiagnostics
{
    [BepInPlugin("NoGlowExpanded.EffectAudit", "No Glow Effect Audit", "1.0.0")]
    internal sealed class EffectAudit : BaseUnityPlugin
    {
        private ConfigEntry<KeyboardShortcut> _shortcut = null!;

        private void Awake()
        {
            _shortcut = Config.Bind("Diagnostics", "Write report", new KeyboardShortcut(KeyCode.F9),
                "Write a read-only item effect report while in a world.");
        }

        private void Update()
        {
            if (_shortcut.Value.IsDown() && Player.m_localPlayer != null && ObjectDB.instance != null)
            {
                try { WriteReport(); }
                catch (Exception exception) { Logger.LogError(exception); }
            }
        }

        private void WriteReport()
        {
            var report = new StringBuilder();
            report.AppendLine("Registered item candidates. Prefab component presence does not prove passive visibility.");
            foreach (var prefab in ObjectDB.instance.m_items)
            {
                if (prefab == null || !prefab.TryGetComponent<ItemDrop>(out var item)) continue;
                var effects = new StringBuilder();
                Describe(prefab, effects);
                if (effects.Length == 0) continue;
                var token = item.m_itemData.m_shared.m_name;
                var label = Localization.instance?.Localize(token) ?? token;
                report.AppendLine($"\n[{prefab.name}] {label} ({token})");
                report.Append(effects);
            }

            report.AppendLine("\n[Local player equipment snapshot] Candidate effects can include unrelated player visuals.");
            var equipment = Player.m_localPlayer.GetComponentInChildren<VisEquipment>(true);
            if (equipment != null) Describe(equipment.gameObject, report);
            var filename = $"NoGlowEffectAudit-{DateTime.Now:yyyyMMdd-HHmmss-fff}.txt";
            var path = Path.Combine(Paths.BepInExRootPath, filename);
            File.WriteAllText(path, report.ToString(), Encoding.UTF8);
            Logger.LogInfo($"Effect audit written to {path}");
        }

        private static void Describe(GameObject root, StringBuilder report)
        {
            foreach (var light in root.GetComponentsInChildren<Light>(true))
                Line(root, light, report, $"Light enabled={light.enabled} intensity={light.intensity} range={light.range}");
            foreach (var controller in root.GetComponentsInChildren<LightLod>(true))
                Line(root, controller, report, $"LightLod enabled={controller.enabled}");
            foreach (var controller in root.GetComponentsInChildren<LightFlicker>(true))
                Line(root, controller, report, $"LightFlicker enabled={controller.enabled}");
            foreach (var particle in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                var renderer = particle.GetComponent<ParticleSystemRenderer>();
                Line(root, particle, report, $"Particles rendererEnabled={renderer != null && renderer.enabled} " +
                    $"playOnAwake={particle.main.playOnAwake} looping={particle.main.loop} " +
                    $"playing={particle.isPlaying} count={particle.particleCount} particleLights={particle.lights.enabled}");
            }
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null || !material.HasProperty("_EmissionColor")) continue;
                    var emission = material.GetColor("_EmissionColor");
                    var keyword = material.IsKeywordEnabled("_EMISSION");
                    if (!keyword && emission.maxColorComponent <= 0f) continue;
                    Line(root, renderer, report, $"Emission candidate material={material.name} shader={material.shader.name} " +
                        $"keyword={keyword} color={emission}");
                }
            }
            foreach (var audio in root.GetComponentsInChildren<AudioSource>(true))
            {
                if (!audio.loop) continue;
                Line(root, audio, report, $"Looping audio clip={audio.clip?.name ?? "(none assigned)"} " +
                    $"enabled={audio.enabled} playOnAwake={audio.playOnAwake} playing={audio.isPlaying}");
            }
        }

        private static void Line(GameObject root, Component component, StringBuilder report, string detail)
        {
            var child = component.transform;
            var path = child.name;
            while (child != root.transform && child.parent != null)
            {
                child = child.parent;
                path = child.name + "/" + path;
            }
            report.AppendLine($"  {path} [activeSelf={component.gameObject.activeSelf}, activeInHierarchy={component.gameObject.activeInHierarchy}] {detail}");
        }
    }
}
