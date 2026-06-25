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
    internal static GameObject CGStatsObject;
    internal static GameObject WaveProgress;
    internal static GameObject CGEnemies;
    internal static GameObject TotalTime;
    internal static GameObject ThisWaveTime;

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
        ConfigManager.ShowEnemies.postValueChangeEvent += UpdateVisibilities;
        ConfigManager.ShowWave.postValueChangeEvent += UpdateVisibilities;
        ConfigManager.ShowTotalTime.postValueChangeEvent += UpdateVisibilities;
        ConfigManager.ShowWaveTime.postValueChangeEvent += UpdateVisibilities;
    }

    private static void UpdateVisibilities() {
        WaveProgress.SetActive(ConfigManager.ShowWave.value);
        CGEnemies.SetActive(ConfigManager.ShowEnemies.value);
        TotalTime.SetActive(ConfigManager.ShowTotalTime.value);
        ThisWaveTime.SetActive(ConfigManager.ShowWaveTime.value);
    }

    private static void UpdateVisibilities(bool _) {
        UpdateVisibilities();
    }

    public static void Setup() {
        GameObject vanillaTabArea = UIUtils.FindRecursive("Canvas/Level Stats Controller");
        GameObject vanillaTabPanel = UIUtils.FindRecursive(vanillaTabArea, "Level Stats (1)");
        AssetBundle bundle = AssetBundle.LoadFromFile(Plugin.BundlePath);
        if (CGStatsPrefab == null) CGStatsPrefab = bundle.LoadAsset<GameObject>("CGStats");
        if (EnemyIconPrefab == null) EnemyIconPrefab = bundle.LoadAsset<GameObject>("CGEnemy");
        bundle.Unload(false);

        // Nuke vanilla panel
        Object.Destroy(vanillaTabPanel);

        // Add Taberry panel...
        CGStatsObject = Object.Instantiate(CGStatsPrefab, vanillaTabArea.transform);
        WaveProgress = UIUtils.FindRecursive(CGStatsObject, "Wave");
        CGEnemies = UIUtils.FindRecursive(CGStatsObject, "Enemies");
        TotalTime = UIUtils.FindRecursive(CGStatsObject, "Time/Total");
        ThisWaveTime = UIUtils.FindRecursive(CGStatsObject, "Time/ThisWave");

        // ...and hook it to the Tab keybind
        Object.Destroy(vanillaTabArea.GetComponent<LevelStatsEnabler>());
        var lse = vanillaTabArea.AddComponent<TabEnabler>();
        lse.levelStats = CGStatsObject;

        // Add controllers
        UIUtils.FindRecursive(CGStatsObject, "Wave").AddComponent<WaveIndicatorController>();
        UpdateVisibilities();
    }

    public static void UnfuckEnemiesLayout() {
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)CybergrindHandler.CGEnemies.transform);
    }
}

[HarmonyPatch]
internal static class CybergrindPatches {
    private static StatsManager sman => MonoSingleton<StatsManager>.Instance;
    private static float lastSeconds;

    [HarmonyPatch(typeof(EndlessGrid), "SpawnOnGrid"), HarmonyPostfix]
    private static void EndlessGrid_SpawnOnGrid(GameObject obj, bool radiant, GameObject __result,
        PrefabDatabase ___prefabs) {
        if (__result?.GetComponentInChildren<EnemyIdentifier>(true) is not { } eid) return;

        GameObject icon = Object.Instantiate(CybergrindHandler.EnemyIconPrefab, CybergrindHandler.CGEnemies.transform);
        UIUtils.FindRecursive(icon, "TypeIcon").AddComponent<EnemyIconController>().enemyIdentifier = eid;
        if (radiant) {
            UIUtils.FindRecursive(icon, "RadiantModifier").SetActive(true);
        }

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
            if (!eid) {
                icon.SetActive(false);
                CybergrindHandler.UnfuckEnemiesLayout();
                continue;
            }

            icon.transform.Find("RadiantModifier").gameObject.SetActive(radiant);
            icon.transform.Find("IdolModifier").gameObject.SetActive(eid.blessed);
            icon.SetActive(!eid.dead);
            CybergrindHandler.UnfuckEnemiesLayout();
        }

        CybergrindHandler.CGEnemies.SetActive(ConfigManager.ShowEnemies.value && CybergrindHandler.icons.Any(icon => icon.IconObject.activeSelf));

        // Update time
        float seconds = sman.seconds;
        float secondsThisWave = sman.seconds - lastSeconds;
        float minutes = 0f;
        float minutesThisWave = 0f;
        minutes = (int)seconds / 60;
        minutesThisWave = (int)secondsThisWave / 60;
        seconds = seconds % 60f;
        secondsThisWave = secondsThisWave % 60f;

        CybergrindHandler.TotalTime.GetComponent<TMP_Text>().text = minutes + ":" + seconds.ToString("00.000");
        CybergrindHandler.ThisWaveTime.GetComponent<TMP_Text>().text =
            "+ " + minutesThisWave + ":" + secondsThisWave.ToString("00.000");
    }

    [HarmonyPatch(typeof(EndlessGrid), "NextWave"), HarmonyPostfix]
    private static void EndlessGrid_NextWave() {
        CybergrindHandler.icons.Do(icon => Object.Destroy(icon.IconObject));
        CybergrindHandler.icons.Clear();
        lastSeconds = sman.seconds;
    }
}
