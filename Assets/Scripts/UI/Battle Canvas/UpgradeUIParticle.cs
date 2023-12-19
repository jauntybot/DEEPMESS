using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUIParticle : MonoBehaviour {

    public SlagEquipmentData.UpgradePath type;
    [SerializeField] List<Sprite> particleSprites;
    public void Init(SlagEquipmentData.UpgradePath _type) {
        name = "Upgrade UI Particle - " + type.ToString();
        type = _type;
        GetComponentInChildren<Image>().sprite = particleSprites[(int)type];
    }

}
