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
    private List<GameObject> SecretOrbDoneMarks = new List<GameObject>();
    private HashSet<int> ActivatedSecretOrbs = new HashSet<int>();
    
    private bool? lastChallengeActiveState = null;
    private bool? lastChallengeDoneActiveState = null;

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
            var doneMark = newOrb.FindRecursive("DoneMark");
            SecretOrbDoneMarks.Add(doneMark);
        }

        UpdateVisibilities();
        NormalLevelHandler.UnfuckLayouts();
    }

    public void UpdateVisibilities() {
        GameObject SecretsObj = this.gameObject.FindRecursive("Secrets");
        if (SecretsObj == null) return;
        this.gameObject.FindRecursive("Secrets/SecretIcon").SetActive(SecretOrbs.Count > 0);
        SecretsObj.SetActive(ConfigManager.ShowSecrets.value && SecretOrbs.Count > 0);
        this.gameObject.FindRecursive("padder").SetActive(!SecretsObj.activeSelf);
    }

    private void Update() {
        foreach (var secretNumber in sman.prevSecrets) {
            if (ActivatedSecretOrbs.Add(secretNumber)) {
                if (secretNumber < SecretOrbDoneMarks.Count && SecretOrbDoneMarks[secretNumber] != null) {
                    SecretOrbDoneMarks[secretNumber].SetActive(true);
                }
            }
        }

        foreach (var secretNumber in sman.newSecrets) {
            if (ActivatedSecretOrbs.Add(secretNumber)) {
                if (secretNumber < SecretOrbDoneMarks.Count && SecretOrbDoneMarks[secretNumber] != null) {
                    SecretOrbDoneMarks[secretNumber].SetActive(true);
                }
            }
        }

        bool targetChallengeActive = false;
        bool targetChallengeDoneActive = false;

        var challengeMgr = MonoSingleton<ChallengeManager>.Instance;
        if (challengeMgr != null && challengeMgr.challengeDone && !challengeMgr.challengeFailed) {
            targetChallengeActive = false;
            targetChallengeDoneActive = ConfigManager.ShowChallenge.value;
        } else {
            targetChallengeActive = ConfigManager.ShowChallenge.value;
            targetChallengeDoneActive = false;
        }

        if (Challenge != null && (!lastChallengeActiveState.HasValue || lastChallengeActiveState.Value != targetChallengeActive)) {
            Challenge.SetActive(targetChallengeActive);
            lastChallengeActiveState = targetChallengeActive;
        }

        if (ChallengeDone != null && (!lastChallengeDoneActiveState.HasValue || lastChallengeDoneActiveState.Value != targetChallengeDoneActive)) {
            ChallengeDone.SetActive(targetChallengeDoneActive);
            lastChallengeDoneActiveState = targetChallengeDoneActive;
        }
    }
}
