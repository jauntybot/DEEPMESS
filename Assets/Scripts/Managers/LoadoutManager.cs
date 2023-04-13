using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadoutManager : MonoBehaviour
{


    public Unit[] unitPrefabs;

    public IEnumerator Initialize(List<Unit> units) {

        yield return null;

        foreach(Unit u in units) {
            UnitUI ui = UIManager.instance.CreateLoadoutUI(u);
            ui.ToggleUnitPanel(true);
            for(int i = ui.equipment.Count - 1; i >= 0; i--) {
                if (ui.equipment[i].data is not ConsumableEquipmentData) {
                    EquipmentButton b = ui.equipment[i];
                    ui.equipment.Remove(b);
                    Destroy(b.gameObject);
                }
            }
            ui.ToggleEquipmentPanel(true);
        }
        yield return StartCoroutine(UIManager.instance.InitialLoadOutScreen());
    }
}
