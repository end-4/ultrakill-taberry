using System;
using System.IO;
using NukeLib.UI;
using PluginConfig.API;
using PluginConfig.API.Decorators;
using PluginConfig.API.Fields;
using Taberry.NormalLevel;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Taberry;

public static class ConfigManager {
    private static PluginConfigurator config;

    public enum RequirementType {
        Next,
        SRank
    }

    public enum DeadEnemyIconDisplayType {
        Cross,
        Hide
    }

    public static BoolField ShowByDefault;
    public static FloatField PanelScale;
    public static BoolField ToggleAnimation;

    public static BoolField ShowLevelName;
    public static BoolField ShowRankStats;
    public static BoolField ShowRequirements;
    public static EnumField<RequirementType> RequirementStyle;
    public static BoolField ShowSecrets;
    public static BoolField ShowChallenge;

    public static BoolField ShowWave;
    public static BoolField ShowEnemies;
    public static BoolField ShowTotalTime;
    public static BoolField ShowWaveTime;
    public static EnumField<DeadEnemyIconDisplayType> DeadEnemyDisplayType;

    public static void Initialize() {
    }

    static ConfigManager() {
        config = PluginConfigurator.Create("Taberry", Plugin.PluginGUID);
        string iconPath = Path.Combine(Plugin.workingDir, "icon.png");
        if (File.Exists(iconPath)) config.SetIconWithURL(iconPath);

        new ConfigHeader(config.rootPanel, "", 10);
        new ConfigHeader(config.rootPanel, "-- <color=#12b4ff>GENERAL</color> --", 24);

        ShowByDefault = new BoolField(config.rootPanel, "Show by default", "showByDefault", true);
        PanelScale = new FloatField(config.rootPanel, "Panel scale", "panelScale", 1);
        PanelScale.postValueChangeEvent += (float f) => {
            UIUtils.FindRecursive("Canvas/Level Stats Controller/").transform.localScale = f * Vector3.one;
        };
        ToggleAnimation = new BoolField(config.rootPanel, "Toggle animation", "toggleAnimation", true);

        new ConfigHeader(config.rootPanel, "-- <color=#eb3b3b>NORMAL LEVELS</color> --", 24);
        ShowLevelName = new BoolField(config.rootPanel, "Show level name", "showLevelName", true);
        ShowRankStats = new BoolField(config.rootPanel, "Show rank stats", "showRankStats", true);
        ShowRequirements = new BoolField(config.rootPanel, "Show rank requirements", "showRequirements", true);

        RequirementStyle = new EnumField<RequirementType>(config.rootPanel, "Requirement style", "requirementStyle",
            RequirementType.Next);
        ShowSecrets = new BoolField(config.rootPanel, "Show secrets", "showSecrets", true);
        ShowChallenge = new BoolField(config.rootPanel, "Show challenge", "showChallenge", true);

        new ConfigHeader(config.rootPanel, "", 10);
        new ConfigHeader(config.rootPanel, "-- <color=#fbc94c>CYBER GRIND</color> --", 24);
        ShowWave = new BoolField(config.rootPanel, "Show wave", "showWave", true);
        ShowEnemies = new BoolField(config.rootPanel, "Show enemy icons", "showEnemies", true);
        ShowTotalTime = new BoolField(config.rootPanel, "Show total time", "showTotalTime", true);
        ShowWaveTime = new BoolField(config.rootPanel, "Show wave time", "showWaveTime", true);
        DeadEnemyDisplayType = new EnumField<DeadEnemyIconDisplayType>(config.rootPanel, "Dead enemy display type",
            "deadEnemyDisplayType", DeadEnemyIconDisplayType.Cross);
    }
}
