using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadoutManager : MonoBehaviour
{


    public Unit[] unitPrefabs;

    public List<EquipmentData> loadoutOptions;

    public IEnumerator Initialize() {
        yield return StartCoroutine(UIManager.instance.InitialLoadOutScreen());
        foreach(Unit u in unitPrefabs) {
            UIManager.instance.CreateLoadoutUI(u);
        }
    }

    public void UpdateLoadout(Unit u, EquipmentData equip) {
        foreach(EquipmentData e in u.equipment) {
            if (e is ConsumableEquipmentData) {
                u.equipment.Remove(e);
            }
        }
        u.equipment.Add(equip);
    }

}
