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

    internal static GameObject? CGStatsObject;
    internal static GameObject? WaveProgress;
    internal static GameObject? CGEnemies;
    internal static GameObject? TimeGroup;
    internal static GameObject? TotalTime;
    internal static GameObject? ThisWaveTime;


    internal static TextMeshProUGUI? TotalTimeTextComp;
    internal static TextMeshProUGUI? ThisWaveTimeTextComp;
    internal static WaveIndicatorController? WaveIndicatorComp;


    private static bool? lastWaveVisible;
    private static bool? lastEnemiesVisible;
    private static bool? lastTotalTimeVisible;
    private static bool? lastWaveTimeVisible;
    private static bool? lastTimeGroupVisible;
    private static bool? lastStatsObjectVisible;

    internal class EnemyIconRecord : IComparable<EnemyIconRecord> {
        public readonly EnemyIdentifier Enemy;
        public readonly EnemyCategory Type;
        public readonly bool IsRadiant;
        public readonly GameObject IconObject;

        // UI elements
        public readonly GameObject RadiantModifier;
        public readonly GameObject IdolModifier;
        public readonly GameObject Eliminated;

        // States
        public bool LastActiveState = true;
        public bool LastBlessedState = false;
        public bool LastDeadState = false;

        public EnemyIconRecord(EnemyIdentifier enemy, EnemyCategory type, bool isRadiant, GameObject iconObject) {
            Enemy = enemy;
            Type = type;
            IsRadiant = isRadiant;
            IconObject = iconObject;

            RadiantModifier = iconObject.FindRecursive("RadiantModifier");
            IdolModifier = iconObject.FindRecursive("IdolModifier");
            Eliminated = iconObject.FindRecursive("Eliminated");
        }

        public int CompareTo(EnemyIconRecord other) {
            if (other == null) return 1;
            int typeComparison = Type.CompareTo(other.Type);
            if (typeComparison != 0) return typeComparison;

            int rankComparison = EnemyTracker.Instance.GetEnemyRank(Enemy)
                .CompareTo(EnemyTracker.Instance.GetEnemyRank(other.Enemy));
            if (rankComparison != 0) return rankComparison;

            int nameComparison = string.Compare(Enemy.FullName, other.Enemy.FullName, StringComparison.InvariantCultureIgnoreCase);
            return nameComparison != 0 ? nameComparison : Enemy.GetInstanceID().CompareTo(other.Enemy.GetInstanceID());
        }
    }

    internal static List<EnemyIconRecord> icons = new List<EnemyIconRecord>();

    static CybergrindHandler() {
        ConfigManager.ShowWave.postValueChangeEvent += UpdateVisibilities;
        ConfigManager.ShowEnemies.postValueChangeEvent += UpdateVisibilities;
        ConfigManager.ShowTotalTime.postValueChangeEvent += UpdateVisibilities;
        ConfigManager.ShowWaveTime.postValueChangeEvent += UpdateVisibilities;
        ConfigManager.ShowCGDifficulty.postValueChangeEvent += UpdateVisibilities;
    }

    private static bool AreAnyIconsActive() {
        for (int i = 0; i < icons.Count; i++) {
            var icon = icons[i];
            if (icon?.IconObject != null && icon.IconObject.activeSelf) return true;
        }
        return false;
    }

    internal static void UpdateVisibilities() {
        bool targetWave = ConfigManager.ShowWave.value;
        if (WaveProgress != null && (!lastWaveVisible.HasValue || lastWaveVisible.Value != targetWave)) {
            WaveProgress.SetActive(targetWave);
            lastWaveVisible = targetWave;
        }

        bool targetEnemies = ConfigManager.ShowEnemies.value && AreAnyIconsActive();
        if (CGEnemies != null && (!lastEnemiesVisible.HasValue || lastEnemiesVisible.Value != targetEnemies)) {
            CGEnemies.SetActive(targetEnemies);
            lastEnemiesVisible = targetEnemies;
        }

        bool targetTotalTime = ConfigManager.ShowTotalTime.value;
        if (TotalTime != null && (!lastTotalTimeVisible.HasValue || lastTotalTimeVisible.Value != targetTotalTime)) {
            TotalTime.SetActive(targetTotalTime);
            lastTotalTimeVisible = targetTotalTime;
        }

        bool targetWaveTime = ConfigManager.ShowWaveTime.value;
        if (ThisWaveTime != null && (!lastWaveTimeVisible.HasValue || lastWaveTimeVisible.Value != targetWaveTime)) {
            ThisWaveTime.SetActive(targetWaveTime);
            lastWaveTimeVisible = targetWaveTime;
        }

        bool targetTimeGroup = ConfigManager.ShowTotalTime.value || ConfigManager.ShowWaveTime.value;
        if (TimeGroup != null && (!lastTimeGroupVisible.HasValue || lastTimeGroupVisible.Value != targetTimeGroup)) {
            TimeGroup.SetActive(targetTimeGroup);
            lastTimeGroupVisible = targetTimeGroup;
        }

        bool targetStatsObj = (WaveProgress != null && WaveProgress.activeSelf) ||
                               (CGEnemies != null && CGEnemies.activeSelf) ||
                               (TimeGroup != null && TimeGroup.activeSelf);
        if (CGStatsObject != null && (!lastStatsObjectVisible.HasValue || lastStatsObjectVisible.Value != targetStatsObj)) {
            CGStatsObject.SetActive(targetStatsObj);
            lastStatsObjectVisible = targetStatsObj;
        }

        if (WaveIndicatorComp != null) {
            WaveIndicatorComp.UpdateVisibilities();
        }
    }

    private static void UpdateVisibilities(bool _) => UpdateVisibilities();

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

        var waveObj = CGStatsObject.FindRecursive("Wave");
        WaveProgress = waveObj;
        CGEnemies = CGStatsObject.FindRecursive("Enemies");
        TimeGroup = CGStatsObject.FindRecursive("Time");
        TotalTime = TimeGroup.FindRecursive("Total");
        ThisWaveTime = TimeGroup.FindRecursive("ThisWave");

        if (TotalTime != null) TotalTimeTextComp = TotalTime.GetComponent<TextMeshProUGUI>();
        if (ThisWaveTime != null) ThisWaveTimeTextComp = ThisWaveTime.GetComponent<TextMeshProUGUI>();

        Object.Destroy(vanillaTabArea.GetComponent<LevelStatsEnabler>());
        var lse = vanillaTabArea.AddComponent<TabEnabler>();
        lse.levelStats = CGStatsObject;

        // Add controllers
        if (waveObj != null) WaveIndicatorComp = waveObj.AddComponent<WaveIndicatorController>();
        CGStatsObject.AddComponent<WeaponPosLayoutAdapter>();
        if (CGEnemies != null) CGEnemies.AddComponent<WeaponPosLayoutAdapter>();

        lastWaveVisible = null; lastEnemiesVisible = null; lastTotalTimeVisible = null;
        lastWaveTimeVisible = null; lastTimeGroupVisible = null; lastStatsObjectVisible = null;

        UpdateVisibilities();
    }

    public static void UnfuckEnemiesLayout() {
        if (CGEnemies == null) return;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)CGEnemies.transform);
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
    private static void EndlessGrid_SpawnOnGrid(GameObject obj, bool radiant, GameObject __result, PrefabDatabase ___prefabs) {
        if (__result?.GetComponentInChildren<EnemyIdentifier>(true) is not { } eid) return;

        GameObject icon = Object.Instantiate(CybergrindHandler.EnemyIconPrefab, CybergrindHandler.CGEnemies.transform);
        var eic = UIUtils.FindRecursive(icon, "TypeIcon").AddComponent<EnemyIconController>();
        eic.enemyIdentifier = eid;
        eic.style = ConfigManager.IconStyle.value;


        CybergrindHandler.EnemyCategory type = CybergrindHandler.EnemyCategory.Common;
        if (___prefabs.uncommonEnemies.Any(prefab => prefab.prefab == obj)) {
            type = CybergrindHandler.EnemyCategory.Uncommon;
        } else if (___prefabs.specialEnemies.Any(prefab => prefab.prefab == obj)) {
            type = CybergrindHandler.EnemyCategory.Special;
        } else if (obj == ___prefabs.hideousMass) {
            type = CybergrindHandler.EnemyCategory.Mass;
        }

        var record = new CybergrindHandler.EnemyIconRecord(eid, type, radiant, icon);

        if (radiant && record.RadiantModifier != null) record.RadiantModifier.SetActive(true);
        if (record.IdolModifier != null) record.IdolModifier.SetActive(eid.blessed);
        record.LastBlessedState = eid.blessed;

        CybergrindHandler.icons.Add(record);
        CybergrindHandler.icons.Sort(); // icons was originally SortedList and it looks nice that way

        for (int i = 0; i < CybergrindHandler.icons.Count; i++) {
            CybergrindHandler.icons[i].IconObject.transform.SetAsLastSibling();
        }

        CybergrindHandler.UnfuckEnemiesLayout();
        CybergrindHandler.UpdateVisibilities();
    }

    [HarmonyPatch(typeof(EndlessGrid), "Update"), HarmonyPostfix]
    private static void EndlessGrid_Update() {
        bool layoutDirty = false;
        bool structuralVisibilityCheckNeeded = false;

        // Loop backwards for safe deletions
        for (int i = CybergrindHandler.icons.Count - 1; i >= 0; i--) {
            var record = CybergrindHandler.icons[i];

            if (record.IconObject == null) {
                CybergrindHandler.icons.RemoveAt(i);
                layoutDirty = true;
                structuralVisibilityCheckNeeded = true;
                continue;
            }

            if (record.Enemy == null) {
                if (record.LastActiveState) {
                    record.IconObject.SetActive(false);
                    record.LastActiveState = false;
                    layoutDirty = true;
                    structuralVisibilityCheckNeeded = true;
                }
                continue;
            }

            // Check if Blessed state switched
            bool isBlessed = record.Enemy.blessed;
            if (isBlessed != record.LastBlessedState) {
                if (record.IdolModifier != null) record.IdolModifier.SetActive(isBlessed);
                record.LastBlessedState = isBlessed;
            }

            // Check if Dead state switched
            bool isDead = record.Enemy.dead;
            if (isDead != record.LastDeadState) {
                if (record.Eliminated != null) record.Eliminated.SetActive(isDead);
                record.LastDeadState = isDead;

                bool targetActive = !isDead || ConfigManager.DeadEnemyDisplayType.value == ConfigManager.DeadEnemyIconDisplayType.Cross;
                if (record.LastActiveState != targetActive) {
                    record.IconObject.SetActive(targetActive);
                    record.LastActiveState = targetActive;
                    layoutDirty = true;
                    structuralVisibilityCheckNeeded = true;
                }
            }
        }

        if (layoutDirty) {
            CybergrindHandler.UnfuckEnemiesLayout();
        }

        if (structuralVisibilityCheckNeeded) {
            CybergrindHandler.UpdateVisibilities();
        }

        // Update time
        float seconds = sman.seconds;
        float secondsThisWave = Mathf.Max(0f, seconds - lastSeconds);

        int totalMin = (int)(seconds / 60f);
        float totalSec = (seconds % 60f);
        int waveMin = (int)(secondsThisWave / 60f);
        float waveSec = (secondsThisWave % 60f);

        if (CybergrindHandler.TotalTimeTextComp != null) {
            CybergrindHandler.TotalTimeTextComp.text = $"{totalMin}:{totalSec.ToString("00.000")}";
        }

        if (CybergrindHandler.ThisWaveTimeTextComp != null) {
            CybergrindHandler.ThisWaveTimeTextComp.text = $"+ {waveMin}:{waveSec.ToString("00.000")}";
        }
    }

    [HarmonyPatch(typeof(EndlessGrid), "NextWave"), HarmonyPostfix]
    private static void EndlessGrid_NextWave() {
        for (int i = 0; i < CybergrindHandler.icons.Count; i++) {
            if (CybergrindHandler.icons[i].IconObject != null) {
                Object.Destroy(CybergrindHandler.icons[i].IconObject);
            }
        }
        CybergrindHandler.icons.Clear();
        lastSeconds = sman.seconds;
        CybergrindHandler.UnfuckEnemiesLayout();
        CybergrindHandler.UpdateVisibilities();
    }
}

