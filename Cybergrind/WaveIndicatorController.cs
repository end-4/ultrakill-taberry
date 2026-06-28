using NukeLib.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Taberry.Cybergrind;

public class WaveIndicatorController : MonoBehaviour {
    private TextMeshProUGUI WaveText;
    private GameObject DifficultyTextObj;
    private Image CircProgMask;
    private ActivateNextWave NextWaveObj;

    internal void UpdateVisibilities() {
        if (DifficultyTextObj != null) DifficultyTextObj.SetActive(ConfigManager.ShowCGDifficulty.value);
    }

    private void Start() {
        WaveText = UIUtils.FindRecursive(this.gameObject, "WaveNumber/NumberText").GetComponent<TextMeshProUGUI>();
        DifficultyTextObj = UIUtils.FindRecursive(this.gameObject, "WaveNumber/Difficulty");
        CircProgMask = UIUtils.FindRecursive(this.gameObject, "WaveCircularProgress").GetComponent<Image>();
        NextWaveObj = Object.FindObjectOfType<ActivateNextWave>();
        DifficultyTextObj.GetComponent<TextMeshProUGUI>().text = DifficultyHelper.GetDifficultyName();
        UpdateVisibilities();
    }


    private void Update() {
        WaveText.text = MonoSingleton<EndlessGrid>.Instance.currentWave.ToString();
        float percentage = (float)NextWaveObj.deadEnemies / (float)MonoSingleton<EndlessGrid>.Instance.enemyAmount;
        CircProgMask.fillAmount = percentage;
    }
}
