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
    private class ScoreComponentCache {
        public GameObject Root;
        public Image CircProgMaskImage;
        public Image CircProgBorderImage;
        public TextMeshProUGUI ValueTextComp;
        public TextMeshProUGUI RequirementTextComp;
        
        public float LastValue = -1f;
        public Color LastBorderColor = Color.clear;
        public string LastValueText = string.Empty;
        public string LastRequirementText = string.Empty;
        public float LastFillAmount = -1f;
        public ConfigManager.RequirementType LastReqStyle = (ConfigManager.RequirementType)(-1);
    }

    private GameObject? TimeObj;
    private GameObject? KillsObj;
    private GameObject? StyleObj;

    private ScoreComponentCache? timeCache;
    private ScoreComponentCache? killsCache;
    private ScoreComponentCache? styleCache;

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
        int displayMinutes = (int)seconds / 60;
        float displaySeconds = seconds % 60f;
        return $"{displayMinutes}:{displaySeconds.ToString("00.000")}";
    }

    private void Awake() {
        TimeObj = this.gameObject.FindRecursive("Time");
        KillsObj = this.gameObject.FindRecursive("Kills");
        StyleObj = this.gameObject.FindRecursive("Style");

        if (TimeObj != null) timeCache = CreateCache(TimeObj);
        if (KillsObj != null) killsCache = CreateCache(KillsObj);
        if (StyleObj != null) styleCache = CreateCache(StyleObj);
    }

    private ScoreComponentCache CreateCache(GameObject target) {
        return new ScoreComponentCache {
            Root = target,
            CircProgMaskImage = target.FindRecursive("CircProg/CircProgMask").GetComponent<Image>(),
            CircProgBorderImage = target.FindRecursive("CircProg/CircProgMask/CircProgBorder").GetComponent<Image>(),
            ValueTextComp = target.FindRecursive("ValueText").GetComponent<TextMeshProUGUI>(),
            RequirementTextComp = target.FindRecursive("RequirementText").GetComponent<TextMeshProUGUI>()
        };
    }

    private void Start() {
        UpdateVisibilities();
        NormalLevelHandler.UnfuckLayouts();
    }

    internal void UpdateVisibilities() {
        if (TimeObj != null)
            TimeObj.FindRecursive("RequirementText")
                .SetActive(ConfigManager.ShowRequirements.value && !NormalLevelHandler.IsSecretLevel);
        if (KillsObj != null)
            KillsObj.FindRecursive("RequirementText")
                .SetActive(ConfigManager.ShowRequirements.value && !NormalLevelHandler.IsSecretLevel);
        if (StyleObj != null)
            StyleObj.FindRecursive("RequirementText")
                .SetActive(ConfigManager.ShowRequirements.value && !NormalLevelHandler.IsSecretLevel);

        if (NormalLevelHandler.IsSecretLevel) {
            if (TimeObj != null) TimeObj.FindRecursive("CircProg/CircProgMask").SetActive(false);
            if (KillsObj != null) KillsObj.SetActive(false);
            if (StyleObj != null) StyleObj.SetActive(false);
        }
    }

    private void UpdateRank(ScoreComponentCache cache, int[] ranks, float currentValue, bool reverse,
        Func<float, string>? valueToStringTransform = null) {
        
        if (valueToStringTransform == null) valueToStringTransform = (f) => f.ToString();

        // Current value text update
        string currentValueString = valueToStringTransform(currentValue);
        if (cache.LastValueText != currentValueString) {
            cache.ValueTextComp.text = currentValueString;
            cache.LastValueText = currentValueString;
        }

        // Circ prog update
        float maxValue = reverse ? ranks[0] : ranks[^1];
        float percentage = maxValue != 0 ? (currentValue / maxValue) : 0f;
        if (reverse) percentage = 1f - percentage;
        percentage = Mathf.Clamp01(percentage);

        if (!Mathf.Approximately(cache.LastFillAmount, percentage)) {
            cache.CircProgMaskImage.fillAmount = percentage;
            cache.LastFillAmount = percentage;
        }

        // Current rank color update
        int currentRank = SingularRankHelper.GetRank(ranks, currentValue, reverse);
        string hexRankColor = SingularRankHelper.GetRankForegroundColor(currentRank);
        ColorUtility.TryParseHtmlString(hexRankColor, out Color colorRankColor);
        
        if (cache.LastBorderColor != colorRankColor) {
            cache.CircProgBorderImage.color = colorRankColor;
            cache.LastBorderColor = colorRankColor;
        }

        // Requirement text update
        var currentReqStyle = ConfigManager.RequirementStyle.value;
        if (!Mathf.Approximately(cache.LastValue, currentValue) || cache.LastReqStyle != currentReqStyle) {
            int requirementRank = currentRank;
            switch (currentReqStyle) {
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
            
            string requirementText = $"<color=grey>/</color><color={hexRequirementColor}>{requirementValueString}</color>";
            if (cache.LastRequirementText != requirementText) {
                cache.RequirementTextComp.text = requirementText;
                cache.LastRequirementText = requirementText;
            }
            
            cache.LastReqStyle = currentReqStyle;
        }

        cache.LastValue = currentValue;
    }

    private void Update() {
        if (timeCache != null && TimeObj != null && TimeObj.activeSelf) {
            UpdateRank(timeCache, sman.timeRanks, sman.seconds, true, valueToStringTransform: FormatTime);
        }
        if (killsCache != null && KillsObj != null && KillsObj.activeSelf) {
            UpdateRank(killsCache, sman.killRanks, sman.kills, false);
        }
        if (styleCache != null && StyleObj != null && StyleObj.activeSelf) {
            UpdateRank(styleCache, sman.styleRanks, sman.stylePoints, false);
        }
    }
}