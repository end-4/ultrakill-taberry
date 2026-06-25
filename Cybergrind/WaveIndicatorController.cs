using NukeLib.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Taberry.Cybergrind;

public class WaveIndicatorController : MonoBehaviour {
    private TextMeshProUGUI WaveText;
    private Image CircProgMask;
    private ActivateNextWave NextWaveObj;

    private void Start() {
        WaveText = UIUtils.FindRecursive(this.gameObject, "WaveNumber").GetComponent<TextMeshProUGUI>();
        CircProgMask = UIUtils.FindRecursive(this.gameObject, "WaveCircularProgress").GetComponent<Image>();
        NextWaveObj = Object.FindObjectOfType<ActivateNextWave>();
    }

    private void Update() {
        WaveText.text = MonoSingleton<EndlessGrid>.Instance.currentWave.ToString();
        float percentage = (float)NextWaveObj.deadEnemies / (float)MonoSingleton<EndlessGrid>.Instance.enemyAmount;
        CircProgMask.fillAmount = percentage;
    }
}
