using NukeLib.UI;
using TMPro;
using UnityEngine;

namespace Taberry.NormalLevel;

public class LevelTitleController : MonoBehaviour {
    private GameObject SmallText;
    private GameObject BigText;

    private void Awake() {
        SmallText = this.gameObject.FindRecursive("LevelInfoRow/Small");
        BigText = this.gameObject.FindRecursive("Main");
    }

    private void Start() {
        string title = "";
        MapInfo mapInfo = MapInfo.Instance;
        StockMapInfo stockMapInfo = StockMapInfo.Instance;
        title = mapInfo?.levelName ?? stockMapInfo?.assets.LargeText ?? SceneHelper.CurrentScene ?? "???";

        if (title.Contains(":")) {
            var parts = title.Split(':');
            SmallText.GetComponent<TextMeshProUGUI>().text = parts[0].Trim();
            BigText.GetComponent<TextMeshProUGUI>().text = parts[1].Trim();
        } else {
            SmallText.GetComponent<TextMeshProUGUI>().text = SceneHelper.IsPlayingCustom ? "Custom level" : "Campaign";
            BigText.GetComponent<TextMeshProUGUI>().text = title.Length > 0 ? title : SceneHelper.CurrentScene;
        }

        NormalLevelHandler.UnfuckLayouts();
    }
}
