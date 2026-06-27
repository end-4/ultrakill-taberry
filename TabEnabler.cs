using UnityEngine;
using NukeLib.UI;

namespace Taberry;

/// <summary>
/// Basically LevelStatsEnabler but simpler because I can't figure out how to make that one work properly
/// </summary>
public class TabEnabler : MonoBehaviour {
    public GameObject levelStats;

    private void Start() {
        levelStats.SetActive(ConfigManager.ShowByDefault.value);
    }

    private void Update() {
        if (MonoSingleton<InputManager>.Instance.InputSource.Stats.WasPerformedThisFrame) {
            if (levelStats != null) {
                SlideFadeToggleEffect toggleEffect = levelStats.GetComponent<SlideFadeToggleEffect>();
                if (toggleEffect == null && ConfigManager.ToggleAnimation.value) {
                    toggleEffect = levelStats.AddComponent<SlideFadeToggleEffect>();
                    toggleEffect.hiddenOffset = new Vector2(0, 30);
                    toggleEffect.speed = 25;
                    toggleEffect.OnExitComplete += () => {
                        levelStats.SetActive(false);
                    };
                } else if (toggleEffect != null && !ConfigManager.ToggleAnimation.value) {
                    Destroy(toggleEffect);
                }

                if (!levelStats.activeSelf) {
                    levelStats.SetActive(true);
                } else if (toggleEffect != null) {
                    toggleEffect.StartExit();
                } else {
                    levelStats.SetActive(false);
                }
            }
        }
    }
}
