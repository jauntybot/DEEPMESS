using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearUpgradeManager : MonoBehaviour {

        public static GearUpgradeManager instance;
        private void Awake() {
            if (GearUpgradeManager.instance) {
                Debug.LogWarning("Warning! More than one instance of GearUpgradeManager found.");
                Destroy(gameObject);
            } 
            instance = this;
        }

        PlayerManager pManager;
        List<GearUpgrade> unitUpgrades;

        public void Init(PlayerManager _pManager) {

            pManager = _pManager;
            for (int i = pManager.units.Count - 1; i >= 0; i--) {
                if (pManager.units[i] is PlayerUnit pu) {
                    for (int x = pu.equipment.Count - 1; x >= 0; x--) {
                        if (pu.equipment[x] is SlagGearData d) {
                            for (int y = d.upgrades.Count - 1; y >= 0; y--) {
                                unitUpgrades.Add(d.upgrades[y]);
                            }
                        }
                    }
                }
            }

        }

}
