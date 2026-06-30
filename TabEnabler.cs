using System;
using UnityEngine;
using NukeLib.UI;
using Object = UnityEngine.Object;

namespace Taberry;

/// <summary>
/// Basically LevelStatsEnabler but simpler because I can't figure out how to make that one work properly
/// </summary>
public class TabEnabler : MonoBehaviour {
    public GameObject levelStats;

    private void Start() {
        if (levelStats != null) {
            UpdateAnchors();
            levelStats.SetActive(ConfigManager.ShowByDefault.value);
        }
    }

    private void OnEnable() {
        PrefsManager.onPrefChanged += OnPrefChanged;
    }

    private void OnDisable() {
        PrefsManager.onPrefChanged -= OnPrefChanged;
    }

    private void OnPrefChanged(string key, object value) {
        if (key == "weaponHoldPosition") UpdateAnchors();
    }

    private void UpdateAnchors() {
        if (levelStats == null) return;
        int weapos = MonoSingleton<PrefsManager>.Instance.GetInt("weaponHoldPosition");
        // 0 = right, 1 = middle, 2 = left. HUD is on the side opposite to the weapon
        RectTransform rt = ((RectTransform)levelStats.transform);
        if (weapos == 2) {
            rt.anchoredPosition = Vector2.one;
            rt.pivot = Vector2.one;
        } else {
            rt.anchoredPosition = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
        }
        rt.localPosition = Vector3.zero; // Make sure anchor changes get committed
    }

    private void Update() {
        if (!MonoSingleton<InputManager>.Instance.InputSource.Stats.WasPerformedThisFrame || levelStats == null) {
            return;
        }

        levelStats.SetActiveAnimated(!levelStats.activeSelf, hiddenOffset: new Vector2(0, 30), speed: 25);
    }
}
