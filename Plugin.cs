using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using NukeLib.UI;
using Taberry.Cybergrind;
using Taberry.NormalLevel;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Taberry;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency("com.eternalUnion.pluginConfigurator")]
public class Plugin : BaseUnityPlugin {
    // Logger
    internal static Plugin Instance;
    internal static ManualLogSource Log;

    // Misc info
    public static string workingPath = Assembly.GetExecutingAssembly().Location;
    public static string workingDir = Path.GetDirectoryName(workingPath);
    public const string PluginGUID = "com.github.end-4.taberry";
    public const string PluginName = "Taberry";
    public const string PluginVersion = "1.1.0";

    // Assets
    internal static readonly string BundlePath = Path.Combine(workingDir, "assets", "taberry.bundle");

    private void Awake() {
        if (Instance != null) return;
        Instance = this;

        Log = Logger;
        ConfigManager.Initialize();

        Harmony harmony = new Harmony("Taberry");
        harmony.PatchAll();

        SceneManager.sceneLoaded += OnSceneLoaded;

        Log.LogInfo($"{PluginName} loaded");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        GameObject tabPanelArea = UIUtils.FindRecursive("Canvas/Level Stats Controller/");
        if (tabPanelArea != null) tabPanelArea.transform.localScale = ConfigManager.PanelScale.value * Vector3.one;
        if (SceneHelper.CurrentScene == "Main Menu" || SceneHelper.CurrentScene == "uk_construct") return;
        if (SceneHelper.CurrentScene == "Endless") {
            CybergrindHandler.Setup();
        } else {
            var levelStatsController = UIUtils.FindRecursive("Canvas/Level Stats Controller");
            if (levelStatsController == null || !levelStatsController.activeSelf) return;
            NormalLevelHandler.Setup();
        }
    }
}
