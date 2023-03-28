using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    
    ScenarioManager scenario;

    [Header("Meta Data")]
    public MetaDisplay metaDisplay;

    [Header("Portraits")]
    [SerializeField] Transform portraitParent;
    [SerializeField] GameObject portraitPrefab;
    List<UnitUI> unitPortraits = new List<UnitUI>();
    bool unitDisplayed;
    [Header("Loadouts")]
    [SerializeField] GameObject loadoutPrefab;
    [SerializeField] Transform loadoutPanel, loadoutUIParent;
    List<UnitUI> loadoutUIs = new List<UnitUI>();


    public static UIManager instance;
    private void Awake() {
        if (UIManager.instance) return;
        UIManager.instance = this;
    }

    void Start() {
        scenario = ScenarioManager.instance;
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

    public UnitUI CreateLoadoutUI(Unit u) {
        UnitUI ui = Instantiate(loadoutPrefab, loadoutUIParent).GetComponent<UnitUI>();
        ui.Initialize(u);
        loadoutUIs.Add(ui);
        return ui;
    }
    

    public IEnumerator InitialLoadOutScreen() {
        loadoutPanel.gameObject.SetActive(true);
        while (scenario.currentTurn != ScenarioManager.Turn.Descent) {
            Vector2 pos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            RaycastHit2D hitInfo = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(pos), Vector2.zero);
            if (hitInfo != default(RaycastHit2D)) {
                UnitUI ui = hitInfo.transform.GetComponent<UnitUI>();
                if (ui != null) {
                    ui.ToggleEquipmentPanel(true);
                }
            }

            yield return null;
        }
        loadoutPanel.gameObject.SetActive(false);
    }

    public void UpdateDropChance(int chance) {
        metaDisplay.UpdateDropChance(chance);
    }

}
