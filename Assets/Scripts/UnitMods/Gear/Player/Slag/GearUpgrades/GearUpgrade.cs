using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GearUpgrade : ScriptableObject {

    public Sprite icon;
    new public string name;
    public int ugpradeLevel = 1;
    public string description;
    public SlagGearData modifiedGear;

    public virtual void EquipDequip(bool equip) {}

}
