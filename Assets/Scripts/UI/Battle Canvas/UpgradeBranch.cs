using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeBranch : MonoBehaviour {

    UpgradeManager upgradeManager;
    [SerializeField] SlagEquipmentData.UpgradePath upgradePath;
    [SerializeField] int tier = 0;
    public UpgradeButtonHoldHandler tierI, tierII, tierIII;

    public void Init(UpgradeManager _upgradeManager) {
        upgradeManager = _upgradeManager;
        tier = 0;
        UpdateBranch();
    }
    
    public void ProgressBranch() {
        tier++;
        
        UpdateBranch();
    }

    public void UpdateBranch(SlagEquipmentData data = null) {
        if (data != null) tier = data.upgrades[upgradePath];
        switch(tier) {
            case 0:
                tierI.upgradeButton.interactable = true;
                tierII.upgradeButton.interactable = false;
                tierIII.upgradeButton.interactable = false;
            break;
            case 1:
                tierI.upgradeButton.interactable = false;
                tierII.upgradeButton.interactable = true;
                tierIII.upgradeButton.interactable = false;
            break;
            case 2:
                tierI.upgradeButton.interactable = false;
                tierII.upgradeButton.interactable = false;
                tierIII.upgradeButton.interactable = true;
            break;
            case 3:
                tierI.upgradeButton.interactable = false;
                tierII.upgradeButton.interactable = false;
                tierIII.upgradeButton.interactable = false;
            break;
        }
        tierI.ResetProgress();
        tierII.ResetProgress();
        tierIII.ResetProgress();
    }
}
