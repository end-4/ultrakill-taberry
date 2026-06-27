using System;
using System.Collections.Generic;
using NukeLib.UI;
using UnityEngine;
using Object = System.Object;

namespace Taberry.NormalLevel;

public class LevelMiscController : MonoBehaviour {
    private GameObject SecretsGroup;
    private GameObject Challenge;
    private GameObject ChallengeDone;
    private List<GameObject> SecretOrbs = new List<GameObject>();
    private static StatsManager sman => MonoSingleton<StatsManager>.Instance;

    private void Awake() {
        SecretsGroup = this.gameObject.FindRecursive("Secrets/SecretOrbs");
        Challenge = this.gameObject.FindRecursive("Challenge");
        ChallengeDone = this.gameObject.FindRecursive("ChallengeDone");
    }

    private void Start() {
        for (int i = 0; i < sman.secretObjects.Length; i++) {
            var newOrb = Instantiate(NormalLevelHandler.SecretPrefab, SecretsGroup.transform);
            SecretOrbs.Add(newOrb);
        }

        UpdateVisibilities();
        NormalLevelHandler.UnfuckLayouts();
    }

    public void UpdateVisibilities() {
        GameObject SecretsObj = this.gameObject.FindRecursive("Secrets");
        this.gameObject.FindRecursive("Secrets/SecretIcon").SetActive(SecretOrbs.Count > 0);
        SecretsObj.SetActive(ConfigManager.ShowSecrets.value && SecretOrbs.Count > 0);
        this.gameObject.FindRecursive("padder").SetActive(!SecretsObj.activeSelf);
    }

    private void Update() {
        foreach (var secretNumber in sman.prevSecrets) {
            SecretOrbs[secretNumber].FindRecursive("DoneMark").SetActive(true);
        }

        foreach (var secretNumber in sman.newSecrets) {
            SecretOrbs[secretNumber].FindRecursive("DoneMark").SetActive(true);
        }


        if (MonoSingleton<ChallengeManager>.Instance != null &&
            MonoSingleton<ChallengeManager>.Instance.challengeDone &&
            !MonoSingleton<ChallengeManager>.Instance.challengeFailed) {
            Challenge.SetActive(false);
            ChallengeDone.SetActive(true && ConfigManager.ShowChallenge.value);
        } else {
            Challenge.SetActive(true && ConfigManager.ShowChallenge.value);
            ChallengeDone.SetActive(false);
        }
    }
}
