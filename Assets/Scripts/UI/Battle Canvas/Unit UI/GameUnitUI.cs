using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUnitUI : UnitUI {

    public override UnitUI Initialize(Unit u, Transform overviewParent = null, Transform overviewLayoutParent = null) {
        UnitUI unitUI = base.Initialize(u, overviewParent, overviewLayoutParent);
        
        if (u is PlayerUnit) {
            if (overviewParent != null) {
                UnitOverview view = Instantiate(overviewPrefab, overviewParent).GetComponent<UnitOverview>();
                overview = view.Initialize(u, overviewLayoutParent);
            }
            UpdateEquipmentButtons();
        }

        ToggleUnitPanel(false);

        return unitUI;
    }
}