using System;
using NukeLib.UI;
using TMPro;
using UnityEngine;

namespace Taberry.NormalLevel;

public class LevelTitleController : MonoBehaviour {
    private GameObject SmallText;
    private GameObject BigText;

    private TextMeshProUGUI? smallTextComp;
    private TextMeshProUGUI? bigTextComp;

    private string lastSmallText = string.Empty;
    private string lastBigText = string.Empty;

    internal void UpdateTitle() {
        if (smallTextComp == null || bigTextComp == null) return;

        MapInfo mapInfo = MapInfo.Instance;
        StockMapInfo stockMapInfo = StockMapInfo.Instance;
        string title = mapInfo?.levelName ?? stockMapInfo?.assets.LargeText ?? SceneHelper.CurrentScene ?? "???";
        
        string smallString;
        string bigString;

        int colonIndex = title.IndexOf(':');
        if (colonIndex != -1) {
            ReadOnlySpan<char> titleSpan = title.AsSpan();
            smallString = titleSpan.Slice(0, colonIndex).Trim().ToString();
            bigString = titleSpan.Slice(colonIndex + 1).Trim().ToString();
        } else {
            smallString = SceneHelper.IsPlayingCustom ? "Custom level" : "Campaign";
            bigString = title.Length > 0 ? title : (SceneHelper.CurrentScene ?? string.Empty);
        }

        string targetSmallText;
        if (ConfigManager.ShowLevelDifficulty.value) {
            targetSmallText = $"{DifficultyHelper.GetDifficultyName()} > {smallString}";
        } else {
            targetSmallText = smallString;
        }

        bool hasChanged = false;

        if (lastSmallText != targetSmallText) {
            smallTextComp.text = targetSmallText;
            lastSmallText = targetSmallText;
            hasChanged = true;
        }

        if (lastBigText != bigString) {
            bigTextComp.text = bigString;
            lastBigText = bigString;
            hasChanged = true;
        }

        if (hasChanged) {
            NormalLevelHandler.UnfuckLayouts();
        }
    }

    internal void UpdateTitle(bool _) {
        UpdateTitle();
    }

    private void Awake() {
        SmallText = this.gameObject.FindRecursive("LevelInfoRow/Small");
        BigText = this.gameObject.FindRecursive("Main");

        if (SmallText != null) smallTextComp = SmallText.GetComponent<TextMeshProUGUI>();
        if (BigText != null) bigTextComp = BigText.GetComponent<TextMeshProUGUI>();

        ConfigManager.ShowLevelDifficulty.postValueChangeEvent += UpdateTitle;
    }

    private void Start() {
        UpdateTitle();
    }

    private void OnDestroy() {
        ConfigManager.ShowLevelDifficulty.postValueChangeEvent -= UpdateTitle;
    }
}