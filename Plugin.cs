using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Taberry.Cybergrind;
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
    public const string PluginVersion = "1.0.0";

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
        if (SceneHelper.CurrentScene == "Endless") {
            CybergrindHandler.Setup();
        }
    }
}
