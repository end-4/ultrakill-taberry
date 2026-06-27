using System;
using System.Text.RegularExpressions;
using NukeLib.Debug;
using NukeLib.UI;
using Taberry.NormalLevel.Ranks;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Taberry.NormalLevel;

public class LevelScoresController : MonoBehaviour {
    private GameObject TimeObj;
    private GameObject KillsObj;
    private GameObject StyleObj;
    private Image CircProgMaskImage;
    private Image CircProgBorderImage;
    private TextMeshProUGUI ValueTextComp;
    private TextMeshProUGUI RequirementTextComp;
    private static StatsManager sman => MonoSingleton<StatsManager>.Instance;

    private Color RankColor(string formattedRankText) {
        Match match = Regex.Match(formattedRankText, @"#([A-Fa-f0-9]{6})");
        if (match.Success) {
            if (ColorUtility.TryParseHtmlString(match.Value, out Color parsedColor)) {
                return parsedColor;
            }
        }

        return Color.white;
    }

    private string FormatTime(float seconds) {
        float displayMinutes = (int)seconds / 60;
        float displaySeconds = seconds % 60f;
        return $"{displayMinutes}:{displaySeconds.ToString("00.000")}";
    }

    private void Awake() {
        TimeObj = this.gameObject.FindRecursive("Time");
        KillsObj = this.gameObject.FindRecursive("Kills");
        StyleObj = this.gameObject.FindRecursive("Style");
    }

    private void Start() {
        UpdateVisibilities();
        NormalLevelHandler.UnfuckLayouts();
    }

    internal void UpdateVisibilities() {
        TimeObj.FindRecursive("RequirementText")
            .SetActive(ConfigManager.ShowRequirements.value && !NormalLevelHandler.IsSecretLevel);
        KillsObj.FindRecursive("RequirementText")
            .SetActive(ConfigManager.ShowRequirements.value && !NormalLevelHandler.IsSecretLevel);
        StyleObj.FindRecursive("RequirementText")
            .SetActive(ConfigManager.ShowRequirements.value && !NormalLevelHandler.IsSecretLevel);
        if (NormalLevelHandler.IsSecretLevel) {
            TimeObj.FindRecursive("CircProg/CircProgMask").SetActive(false);
            KillsObj.SetActive(false);
            StyleObj.SetActive(false);
        }
    }

    private void UpdateRank(GameObject targetObject, int[] ranks, float currentValue, bool reverse,
        Func<float, string> valueToStringTransform = null) {
        // Assign default transform
        if (valueToStringTransform == null) valueToStringTransform = (f) => f.ToString();

        // Get target objects
        CircProgMaskImage = targetObject.FindRecursive("CircProg/CircProgMask").GetComponent<Image>();
        CircProgBorderImage =
            targetObject.FindRecursive("CircProg/CircProgMask/CircProgBorder").GetComponent<Image>();
        ValueTextComp = targetObject.FindRecursive("ValueText").GetComponent<TextMeshProUGUI>();
        RequirementTextComp =
            targetObject.FindRecursive("RequirementText").GetComponent<TextMeshProUGUI>();

        // Current value text update
        string currentValueString = valueToStringTransform(currentValue);
        ValueTextComp.text = currentValueString;

        // Circ prog update
        float maxValue = reverse ? ranks[0] : ranks[^1];
        float percentage = currentValue / maxValue;
        if (reverse) percentage = 1 - percentage;
        CircProgMaskImage.fillAmount = percentage;

        // Current rank color update
        int currentRank = SingularRankHelper.GetRank(ranks, currentValue, reverse);
        string hexRankColor = SingularRankHelper.GetRankForegroundColor(currentRank);
        ColorUtility.TryParseHtmlString(hexRankColor, out Color colorRankColor);
        CircProgBorderImage.color = colorRankColor;

        // Requirement text update
        int requirementRank = currentRank;
        switch (ConfigManager.RequirementStyle.value) {
            case ConfigManager.RequirementType.Next:
                requirementRank = SingularRankHelper.ClampRank(currentRank + (reverse ? 0 : +1), ranks);
                break;
            case ConfigManager.RequirementType.SRank:
                requirementRank = ranks.Length;
                break;
        }

        int requirementIndex = Math.Min(requirementRank - 1, ranks.Length - 1);
        float requirementValue = ranks[requirementIndex];
        string hexRequirementColor = SingularRankHelper.GetRankForegroundColor(requirementRank);
        string requirementValueString = valueToStringTransform(requirementValue);
        RequirementTextComp.text =
            $"<color=grey>/</color><color={hexRequirementColor}>{requirementValueString}</color>";

        // Plugin.Log.LogInfo($"Current rank {currentRank}, requirement rank {requirementRank}, requirement at index {requirementIndex}");
    }

    private void Update() {
        // Plugin.Log.LogInfo($"Time ranks {StatsManager.Instance.timeRanks.Stringify()}");
        // Plugin.Log.LogInfo($"Kill ranks {StatsManager.Instance.killRanks.Stringify()}");
        // Plugin.Log.LogInfo($"Style ranks {StatsManager.Instance.styleRanks.Stringify()}");
        UpdateRank(TimeObj, StatsManager.Instance.timeRanks, sman.seconds, true,
            valueToStringTransform: FormatTime);
        UpdateRank(KillsObj, StatsManager.Instance.killRanks, sman.kills, false);
        UpdateRank(StyleObj, StatsManager.Instance.styleRanks, sman.stylePoints, false);
    }
}
