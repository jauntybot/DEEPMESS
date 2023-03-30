using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadoutManager : MonoBehaviour
{


    public Unit[] unitPrefabs;

    public List<EquipmentData> loadoutOptions;

    public IEnumerator Initialize(List<Unit> units) {

        yield return null;

        foreach(Unit u in units) {
            UnitUI ui = UIManager.instance.CreateLoadoutUI(u);
            ui.ToggleUnitPanel(true);
            for(int i = ui.equipment.Count - 2; i >= 0; i--) {
                EquipmentButton b = ui.equipment[i];
                ui.equipment.Remove(b);
                Destroy(b.gameObject);
            }
            ui.ToggleEquipmentPanel(true);
        }
        yield return StartCoroutine(UIManager.instance.InitialLoadOutScreen());
    }
}
