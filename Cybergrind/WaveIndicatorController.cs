using NukeLib.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Taberry.Cybergrind;

public class WaveIndicatorController : MonoBehaviour {
    private TextMeshProUGUI WaveText;
    private GameObject? DifficultyTextObj;
    private Image CircProgMask;
    private ActivateNextWave NextWaveObj;
    
    private int lastWave = -1;
    private float lastFontSize = -1f;
    private float lastPercentage = -1f;
    private bool? lastDifficultyVisibility = null;

    internal void UpdateVisibilities() {
        bool targetVisibility = ConfigManager.ShowCGDifficulty.value;
        if (DifficultyTextObj != null && (!lastDifficultyVisibility.HasValue || lastDifficultyVisibility.Value != targetVisibility)) {
            DifficultyTextObj.SetActive(targetVisibility);
            lastDifficultyVisibility = targetVisibility;
        }
    }

    private void Start() {
        WaveText = UIUtils.FindRecursive(this.gameObject, "WaveNumber/NumberText").GetComponent<TextMeshProUGUI>();
        DifficultyTextObj = UIUtils.FindRecursive(this.gameObject, "WaveNumber/Difficulty");
        CircProgMask = UIUtils.FindRecursive(this.gameObject, "WaveCircularProgress").GetComponent<Image>();
        NextWaveObj = Object.FindObjectOfType<ActivateNextWave>();
        
        if (DifficultyTextObj != null) {
            var comp = DifficultyTextObj.GetComponent<TextMeshProUGUI>();
            if (comp != null) comp.text = DifficultyHelper.GetDifficultyName();
        }
        UpdateVisibilities();
    }

    private void Update() {
        var endlessGrid = MonoSingleton<EndlessGrid>.Instance;
        if (endlessGrid == null) return;

        // Wave number
        int currentWave = endlessGrid.currentWave;
        if (currentWave != lastWave) {
            string newText = currentWave.ToString();
            WaveText.text = newText;
            lastWave = currentWave;

            float targetFontSize = newText.Length > 2 ? 20f : 24f;
            if (!Mathf.Approximately(WaveText.fontSize, targetFontSize)) {
                WaveText.fontSize = targetFontSize;
            }
        }

        // Wave percentage
        if (NextWaveObj != null) {
            float percentage = endlessGrid.enemyAmount == 0 ? 0f : (float)NextWaveObj.deadEnemies / (float)endlessGrid.enemyAmount;
            percentage = Mathf.Clamp01(percentage);

            if (!Mathf.Approximately(lastPercentage, percentage)) {
                CircProgMask.fillAmount = percentage;
                lastPercentage = percentage;
            }
        }
    }
}