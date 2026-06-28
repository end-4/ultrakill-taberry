using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using NukeLib.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Taberry.NormalLevel;

internal static class NormalLevelHandler {
    internal static GameObject LevelStatsPrefab;
    internal static GameObject SecretPrefab;
    internal static bool IsSecretLevel;

    private static GameObject LevelStats;
    internal static GameObject LevelName;
    internal static GameObject LevelStatsColumn;
    internal static GameObject LevelMiscRow;

    static NormalLevelHandler() {
        ConfigManager.ShowLevelName.postValueChangeEvent += UpdateVisibilities;
        ConfigManager.ShowRankStats.postValueChangeEvent += UpdateVisibilities;
        ConfigManager.ShowRequirements.postValueChangeEvent += UpdateVisibilities;
        ConfigManager.ShowSecrets.postValueChangeEvent += UpdateVisibilities;
        ConfigManager.ShowChallenge.postValueChangeEvent += UpdateVisibilities;
    }

    private static void UpdateVisibilities(bool _) {
        UpdateVisibilities();
    }
    private static void UpdateVisibilities() {
        if (LevelName != null) LevelName.SetActive(ConfigManager.ShowLevelName.value);
        if (LevelStatsColumn != null) {
            LevelStatsColumn.SetActive(ConfigManager.ShowRankStats.value);
            LevelStatsColumn.GetComponent<LevelScoresController>().UpdateVisibilities();
        }

        if (LevelMiscRow != null) {
            LevelMiscRow.SetActive((ConfigManager.ShowSecrets.value || ConfigManager.ShowChallenge.value) && !NormalLevelHandler.IsSecretLevel);
            LevelMiscRow.GetComponent<LevelMiscController>().UpdateVisibilities();
        }

        if (LevelStats != null) 
            LevelStats.SetActive(ConfigManager.ShowLevelName.value || ConfigManager.ShowRankStats.value 
            || ConfigManager.ShowSecrets.value || ConfigManager.ShowChallenge.value);
        
    }

    public static void UnfuckLayouts() {
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)LevelStats.transform);
    }

    internal static void Setup() {
        GameObject vanillaTabArea = UIUtils.FindRecursive("Canvas/Level Stats Controller");
        GameObject vanillaTabPanel = UIUtils.FindRecursive(vanillaTabArea, "Level Stats (1)");
        if (LevelStatsPrefab == null || SecretPrefab == null) {
            AssetBundle bundle = AssetBundle.LoadFromFile(Plugin.BundlePath);
            if (LevelStatsPrefab == null) LevelStatsPrefab = bundle.LoadAsset<GameObject>("LevelStats");
            if (SecretPrefab == null) SecretPrefab = bundle.LoadAsset<GameObject>("Secret");
            bundle.Unload(false);
        }

        // Nuke vanilla panel
        IsSecretLevel = Object.FindObjectOfType<LevelStats>().secretLevel;
        Object.Destroy(vanillaTabPanel);

        // Add Taberry panel...
        LevelStats = Object.Instantiate(LevelStatsPrefab, vanillaTabArea.transform);
        LevelName = LevelStats.FindRecursive("Name");
        LevelStatsColumn = LevelStats.FindRecursive("StatsColumn");
        LevelMiscRow = LevelStats.FindRecursive("MiscRow");
        UnfuckLayouts();

        // ...and hook it to the Tab keybind
        Object.Destroy(vanillaTabArea.GetComponent<LevelStatsEnabler>());
        var lse = vanillaTabArea.AddComponent<TabEnabler>();
        lse.levelStats = LevelStats;

        // Add controllers
        LevelName.AddComponent<LevelTitleController>();
        LevelStatsColumn.AddComponent<LevelScoresController>();
        LevelMiscRow.AddComponent<LevelMiscController>();

        UpdateVisibilities();
    }
}
