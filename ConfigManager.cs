using System;
using System.IO;
using PluginConfig.API;
using PluginConfig.API.Decorators;
using PluginConfig.API.Fields;
using UnityEngine;

namespace Taberry;

public static class ConfigManager {
    private static PluginConfigurator config;

    public static BoolField ShowWave;
    public static BoolField ShowEnemies;
    public static BoolField ShowTotalTime;
    public static BoolField ShowWaveTime;

    public static void Initialize() {}

    static ConfigManager() {
        config = PluginConfigurator.Create("Taberry", Plugin.PluginGUID);
        string iconPath = Path.Combine(Plugin.workingDir, "icon.png");
        if (File.Exists(iconPath)) config.SetIconWithURL(iconPath);

        new ConfigHeader(config.rootPanel, "", 10);
        new ConfigHeader(config.rootPanel, "-- TABERRY --", 24);
        ShowWave = new BoolField(config.rootPanel, "Show wave", "showWave", true);
        ShowEnemies = new BoolField(config.rootPanel, "Show enemy icons", "showEnemies", true);
        ShowTotalTime = new BoolField(config.rootPanel, "Show total time", "showTotalTime", true);
        ShowWaveTime = new BoolField(config.rootPanel, "Show wave time", "showWaveTime", true);
    }
}
