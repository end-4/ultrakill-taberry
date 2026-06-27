using System;
using NukeLib.UI;
using TMPro;
using UnityEngine;

namespace Taberry.NormalLevel;

public class LevelTitleController : MonoBehaviour {
    private GameObject SmallText;
    private GameObject BigText;

    internal void UpdateTitle() {
        string title = "";
        MapInfo mapInfo = MapInfo.Instance;
        StockMapInfo stockMapInfo = StockMapInfo.Instance;
        title = mapInfo?.levelName ?? stockMapInfo?.assets.LargeText ?? SceneHelper.CurrentScene ?? "???";
        string smallString = "";
        string bigString = "";
        if (title.Contains(":")) {
            var parts = title.Split(':');
            smallString = parts[0].Trim();
            bigString = parts[1].Trim();
        } else {
            smallString = SceneHelper.IsPlayingCustom ? "Custom level" : "Campaign";
            bigString = title.Length > 0 ? title : SceneHelper.CurrentScene;
        }

        if (ConfigManager.ShowLevelDifficulty.value) {
            SmallText.GetComponent<TextMeshProUGUI>().text =
                $"{DifficultyHelper.GetDifficultyName()} > {smallString}";
        } else {
            SmallText.GetComponent<TextMeshProUGUI>().text = smallString;
        }
        BigText.GetComponent<TextMeshProUGUI>().text = bigString;

        NormalLevelHandler.UnfuckLayouts();
    }

    internal void UpdateTitle(bool _) {
        UpdateTitle();
    }

    private void Awake() {
        SmallText = this.gameObject.FindRecursive("LevelInfoRow/Small");
        BigText = this.gameObject.FindRecursive("Main");
        ConfigManager.ShowLevelDifficulty.postValueChangeEvent += UpdateTitle;
    }

    private void Start() {
        UpdateTitle();
    }

    private void OnDestroy() {
        ConfigManager.ShowLevelDifficulty.postValueChangeEvent -= UpdateTitle;
    }
}
