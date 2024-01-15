using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NuggetButton : MonoBehaviour {

    public SlagEquipmentData.UpgradePath type;
    Animator anim;
    public void Init(SlagEquipmentData.UpgradePath _type) {
        anim = GetComponent<Animator>();
        int i = 0;
        switch(_type) {
            case SlagEquipmentData.UpgradePath.Shunt: break;
            case SlagEquipmentData.UpgradePath.Scab: i = 1; break;
            case SlagEquipmentData.UpgradePath.Sludge: i = 2; break;
        }
        anim.SetInteger("Color", i);
        name = type.ToString() + "Nugget Button";
        type = _type;
    }

}
