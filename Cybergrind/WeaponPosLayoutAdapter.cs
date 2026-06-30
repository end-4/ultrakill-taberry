using System;
using UnityEngine;
using UnityEngine.UI;

namespace Taberry.Cybergrind;

public class WeaponPosLayoutAdapter : MonoBehaviour {
    private void OnEnable() {
        PrefsManager.onPrefChanged += OnPrefChanged;
    }

    private void OnDisable() {
        PrefsManager.onPrefChanged -= OnPrefChanged;
    }

    private void Start() {
        OnPrefChanged("weaponHoldPosition", MonoSingleton<PrefsManager>.Instance.GetInt("weaponHoldPosition"));
    }

    private void OnPrefChanged(string key, object value) {
        if (key == "weaponHoldPosition") {
            var v = (int)value;
            HorizontalLayoutGroup row = this.GetComponent<HorizontalLayoutGroup>();
            if (row != null) {
                row.reverseArrangement = (v == 2); // 2 = lefty -> reverse
                return;
            }
            GridLayoutGroup grid = this.GetComponent<GridLayoutGroup>();
            if (grid != null) {
                if (grid.startCorner == GridLayoutGroup.Corner.UpperLeft ||
                    grid.startCorner == GridLayoutGroup.Corner.UpperRight) {
                    grid.startCorner = (v == 2 ? GridLayoutGroup.Corner.UpperRight : GridLayoutGroup.Corner.UpperLeft);
                } else {
                    grid.startCorner = (v == 2 ? GridLayoutGroup.Corner.LowerRight : GridLayoutGroup.Corner.LowerLeft);
                }

                return;
            }
        }
    }
}
