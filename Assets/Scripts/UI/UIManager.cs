using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    
    [SerializeField] Transform portraitParent;
    [SerializeField] GameObject portraitPrefab;
    List<UnitUI> unitPortraits = new List<UnitUI>();
    bool unitDisplayed;



    public static UIManager instance;
    private void Awake() {
        if (UIManager.instance) return;
        UIManager.instance = this;
    }

    public void UpdatePortrait(Unit u = null, bool active = true) {
        if (unitDisplayed) {
            foreach (UnitUI ui in unitPortraits) ui.ToggleUnitPanel(false);
        }
        UnitUI unitUI = unitPortraits.Find(p => p.unit == u);
        if (unitUI == null) { unitUI = CreateUnitUI(u); }
        unitUI.ToggleUnitPanel(active);        
    }

    public UnitUI CreateUnitUI(Unit u) {

        UnitUI ui = Instantiate(portraitPrefab, portraitParent).GetComponent<UnitUI>();
        ui.Initialize(u);
        unitPortraits.Add(ui);
        return ui;

    }

    public void UpdateFloorStats() {


    }

}
