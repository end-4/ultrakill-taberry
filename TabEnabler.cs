using UnityEngine;

namespace Taberry;

/// <summary>
/// Basically LevelStatsEnabler but simpler because I can't figure out how to make that one work properly
/// </summary>
public class TabEnabler : MonoBehaviour {
    public GameObject levelStats;

    private void Update() {
        if (MonoSingleton<InputManager>.Instance.InputSource.Stats.WasPerformedThisFrame) {
            if (levelStats != null) {
                levelStats.SetActive(!levelStats.activeSelf);
            }
        }
    }
}
