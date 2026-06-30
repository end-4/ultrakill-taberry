using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using NukeLib.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Taberry.Cybergrind;

internal static class CybergrindHandler {
    internal enum EnemyCategory {
        Common,
        Uncommon,
        Special,
        Mass
    }

    internal static GameObject EnemyIconPrefab;
    internal static GameObject CGStatsPrefab;

    //
    internal static GameObject? CGStatsObject;
    internal static GameObject? WaveProgress;
    internal static GameObject? CGEnemies;
    internal static GameObject? TimeGroup;
    internal static GameObject? TotalTime;
    internal static GameObject? ThisWaveTime;

    internal record struct EnemyIconRecord(
        EnemyIdentifier Enemy,
        EnemyCategory Type,
        bool IsRadiant,
        GameObject IconObject)
        : IComparable<EnemyIconRecord> {
        public bool Equals(EnemyIconRecord? other) => other.HasValue && other.Value.Enemy == Enemy;

        public int CompareTo(EnemyIconRecord other) {
            int typeComparison = Type.CompareTo(other.Type);
            if (typeComparison != 0) {
                return typeComparison;
            }

            int rankComparison = EnemyTracker.Instance.GetEnemyRank(Enemy)
                .CompareTo(EnemyTracker.Instance.GetEnemyRank(other.Enemy));
            if (rankComparison != 0) {
                return rankComparison;
            }

            int nameComparison = string.Compare(Enemy.FullName, other.Enemy.FullName,
                StringComparison.InvariantCultureIgnoreCase);
            return nameComparison != 0 ? nameComparison : Enemy.GetInstanceID().CompareTo(other.Enemy.GetInstanceID());
        }
    }

    internal static SortedSet<EnemyIconRecord> icons = [];

    static CybergrindHandler() {
        ConfigManager.ShowWave.postValueChangeEvent += UpdateVisibilities;
        ConfigManager.ShowEnemies.postValueChangeEvent += UpdateVisibilities;
        ConfigManager.ShowTotalTime.postValueChangeEvent += UpdateVisibilities;
        ConfigManager.ShowWaveTime.postValueChangeEvent += UpdateVisibilities;
        ConfigManager.ShowCGDifficulty.postValueChangeEvent += UpdateVisibilities;
    }

    private static void UpdateVisibilities() {
        if (WaveProgress != null) WaveProgress.SetActive(ConfigManager.ShowWave.value);
        if (CGEnemies != null) CGEnemies.SetActive(
            ConfigManager.ShowEnemies.value &&
            icons != null && icons.Any(icon => (icon.IconObject != null && icon.IconObject.activeSelf))
        );
        if (TotalTime != null) TotalTime.SetActive(ConfigManager.ShowTotalTime.value);
        if (ThisWaveTime != null) ThisWaveTime.SetActive(ConfigManager.ShowWaveTime.value);
        if (TimeGroup != null) TimeGroup.SetActive(ConfigManager.ShowTotalTime.value || ConfigManager.ShowWaveTime.value);
        if (CGStatsObject != null) CGStatsObject.SetActive(
            (WaveProgress != null && WaveProgress.activeSelf) ||
            (CGEnemies != null && CGEnemies.activeSelf) ||
            (TimeGroup != null && TimeGroup.activeSelf)
        );
        if (CGStatsObject != null) {
            GameObject wave = CGStatsObject.FindRecursive("Wave");
            WaveIndicatorController comp = null;
            if (wave != null) comp = wave.GetComponent<WaveIndicatorController>();
            if (comp != null) comp.UpdateVisibilities();
        }
    }

    private static void UpdateVisibilities(bool _) {
        UpdateVisibilities();
    }

    public static void Setup() {
        GameObject vanillaTabArea = UIUtils.FindRecursive("Canvas/Level Stats Controller");
        GameObject vanillaTabPanel = vanillaTabArea.FindRecursive("Level Stats (1)");
        if (CGStatsPrefab == null || EnemyIconPrefab == null) {
            AssetBundle bundle = AssetBundle.LoadFromFile(Plugin.BundlePath);
            if (CGStatsPrefab == null) CGStatsPrefab = bundle.LoadAsset<GameObject>("CGStats");
            if (EnemyIconPrefab == null) EnemyIconPrefab = bundle.LoadAsset<GameObject>("CGEnemy");
            bundle.Unload(false);
        }

        // Nuke vanilla panel
        Object.Destroy(vanillaTabPanel);

        // Add Taberry panel...
        CGStatsObject = Object.Instantiate(CGStatsPrefab, vanillaTabArea.transform);
        WaveProgress = CGStatsObject.FindRecursive("Wave");
        CGEnemies = CGStatsObject.FindRecursive("Enemies");
        TimeGroup = CGStatsObject.FindRecursive("Time");
        TotalTime = TimeGroup.FindRecursive("Total");
        ThisWaveTime = TimeGroup.FindRecursive("ThisWave");

        // ...and hook it to the Tab keybind
        Object.Destroy(vanillaTabArea.GetComponent<LevelStatsEnabler>());
        var lse = vanillaTabArea.AddComponent<TabEnabler>();
        lse.levelStats = CGStatsObject;

        // Add controllers
        CGStatsObject.FindRecursive("Wave").AddComponent<WaveIndicatorController>();
        CGStatsObject.AddComponent<WeaponPosLayoutAdapter>();
        CGStatsObject.FindRecursive("Enemies").AddComponent<WeaponPosLayoutAdapter>();
        UpdateVisibilities();
    }

    public static void UnfuckEnemiesLayout() {
        if (CGEnemies == null) return;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)CybergrindHandler.CGEnemies.transform);
    }
}

[HarmonyPatch]
internal static class CybergrindPatches {
    private static StatsManager sman => MonoSingleton<StatsManager>.Instance;
    private static float lastSeconds;

    [HarmonyPatch(typeof(EndlessGrid), "Start"), HarmonyPostfix]
    private static void EndlessGrid_Start() {
        CybergrindHandler.icons.Clear();
        lastSeconds = sman.seconds;
    }

    [HarmonyPatch(typeof(EndlessGrid), "SpawnOnGrid"), HarmonyPostfix]
    private static void EndlessGrid_SpawnOnGrid(GameObject obj, bool radiant, GameObject __result,
        PrefabDatabase ___prefabs) {
        if (__result?.GetComponentInChildren<EnemyIdentifier>(true) is not { } eid) return;

        GameObject icon = Object.Instantiate(CybergrindHandler.EnemyIconPrefab, CybergrindHandler.CGEnemies.transform);
        UIUtils.FindRecursive(icon, "TypeIcon").AddComponent<EnemyIconController>().enemyIdentifier = eid;
        if (radiant) UIUtils.FindRecursive(icon, "RadiantModifier").SetActive(true);

        CybergrindHandler.EnemyCategory type = CybergrindHandler.EnemyCategory.Common;
        if (___prefabs.uncommonEnemies.Any(prefab => prefab.prefab == obj)) {
            type = CybergrindHandler.EnemyCategory.Uncommon;
        } else if (___prefabs.specialEnemies.Any(prefab => prefab.prefab == obj)) {
            type = CybergrindHandler.EnemyCategory.Special;
        } else if (obj == ___prefabs.hideousMass) {
            type = CybergrindHandler.EnemyCategory.Mass;
        }

        CybergrindHandler.icons.Add(new CybergrindHandler.EnemyIconRecord(eid, type, radiant, icon));
        CybergrindHandler.icons.Do(icon2 => icon2.IconObject.transform.SetAsLastSibling()); // jade does this hmmmmm
        CybergrindHandler.UnfuckEnemiesLayout();
    }

    [HarmonyPatch(typeof(EndlessGrid), "Update"), HarmonyPostfix]
    private static void EndlessGrid_Update() {
        // Update enemies
        foreach ((EnemyIdentifier eid, CybergrindHandler.EnemyCategory type, bool radiant, GameObject icon) in
                 CybergrindHandler.icons) {
            if (icon == null) {
                CybergrindHandler.icons.RemoveWhere(record => record.IconObject == null);
                continue;
            }
            if (!eid) {
                icon.SetActive(false);
                CybergrindHandler.UnfuckEnemiesLayout();
                continue;
            }

            icon.FindRecursive("RadiantModifier").gameObject.SetActive(radiant);
            icon.FindRecursive("IdolModifier").gameObject.SetActive(eid.blessed);
            icon.FindRecursive("Eliminated").SetActive(eid.dead);
            icon.SetActive(!eid.dead || ConfigManager.DeadEnemyDisplayType.value ==
                ConfigManager.DeadEnemyIconDisplayType.Cross);
            CybergrindHandler.UnfuckEnemiesLayout();
        }

        if (CybergrindHandler.CGEnemies != null) CybergrindHandler.CGEnemies.SetActive(ConfigManager.ShowEnemies.value &&
                                              CybergrindHandler.icons.Any(icon => (icon.IconObject != null && icon.IconObject.activeSelf)));

        // Update time
        float seconds = sman.seconds;
        float secondsThisWave = sman.seconds - lastSeconds;
        float minutes = 0f;
        float minutesThisWave = 0f;
        minutes = (int)seconds / 60;
        minutesThisWave = (int)secondsThisWave / 60;
        seconds = seconds % 60f;
        secondsThisWave = secondsThisWave % 60f;

        if (CybergrindHandler.TotalTime != null) {
            CybergrindHandler.TotalTime.GetComponent<TMP_Text>().text = minutes + ":" + seconds.ToString("00.000");
        }
        if (CybergrindHandler.ThisWaveTime != null) {
            CybergrindHandler.ThisWaveTime.GetComponent<TMP_Text>().text =
                "+ " + minutesThisWave + ":" + secondsThisWave.ToString("00.000");
        }
    }

    [HarmonyPatch(typeof(EndlessGrid), "NextWave"), HarmonyPostfix]
    private static void EndlessGrid_NextWave() {
        CybergrindHandler.icons.Do(icon => Object.Destroy(icon.IconObject));
        CybergrindHandler.icons.Clear();
        lastSeconds = sman.seconds;
    }
}
