using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class UIManager : MonoBehaviour
{
    
    ScenarioManager scenario;

    [SerializeField] Animator canvasAnim;

    [Header("Top UIs")]
    public MetaDisplay metaDisplay;

    [Header("Floor Buttons")]
    [SerializeField] public Button upButton;
    [SerializeField] public Button downButton, endTurnButton, undoButton;

    [Header("Portraits")]
    [SerializeField] Transform portraitParent;
    [SerializeField] GameObject portraitPrefab;
    public List<UnitUI> unitPortraits = new List<UnitUI>();

    [Header("Overviews")]
    [SerializeField] Transform overviewParent;
    [SerializeField] Transform overviewLayoutParent;
    [SerializeField] UnitOverview nailOverview;

    [Header("Loadouts")]
    [SerializeField] LoadoutManager loadoutManager;

    [SerializeField] Transform loadoutBG, loadoutPanel;

    [Header("UI AUDIO")]
    [SerializeField] public SFX peekBelowSFX;
    [SerializeField] public SFX peekAboveSFX, genSelectSFX, genDeselectSFX;

    
    private AudioSource audioSource;

    public static UIManager instance;
    private void Awake() {
        if (UIManager.instance) return;
        UIManager.instance = this;
    }

    void Start() {
        scenario = ScenarioManager.instance;
        ToggleUndoButton(false);
        audioSource = GetComponent<AudioSource>();
    }

    public void UpdatePortrait(Unit u = null, bool active = true) {
        foreach (UnitUI ui in unitPortraits) ui.ToggleUnitPanel(false);
    
        UnitUI unitUI = unitPortraits.Find(p => p.unit == u);
        if (unitUI == null && u != null) { unitUI = CreateUnitUI(u); }
        if (unitUI) unitUI.ToggleUnitPanel(active);        
    }

    public UnitUI CreateUnitUI(Unit u) {

        UnitUI ui = Instantiate(portraitPrefab, portraitParent).GetComponent<UnitUI>();
        u.ui = ui.Initialize(u, overviewParent, overviewLayoutParent);
        if (u is Nail) {
            u.ui.overview = nailOverview.Initialize(u, nailOverview.transform.parent);
        }
        unitPortraits.Add(ui);

        return ui;
    }

    public IEnumerator LoadOutScreen(bool first = false) {
        loadoutBG.gameObject.SetActive(true);
        loadoutPanel.gameObject.SetActive(true);
        loadoutManager.DisplayLoadout(first);
        while (scenario.currentTurn == ScenarioManager.Turn.Loadout) {

            yield return null;
        }
        loadoutBG.gameObject.SetActive(false);
        loadoutPanel.gameObject.SetActive(false);
        foreach(Unit u in scenario.player.units)
            u.ui.UpdateEquipmentButtons();
    }

    public void LockFloorButtons(bool state) {
        upButton.GetComponent<Button>().interactable = state ? false : scenario.floorManager.floors.Count - 1 > scenario.floorManager.currentFloor.index;
        downButton.GetComponent<Button>().interactable = state ? false : scenario.floorManager.floors.Count - 1 > scenario.floorManager.currentFloor.index;
    }

    public void LockHUDButtons(bool state) {
        LockFloorButtons(state);
        endTurnButton.interactable = !state;
        undoButton.interactable = state ? false : scenario.player.undoOrder.Count > 0;
    }

    public void ToggleUndoButton(bool state) {
        if (state)
            state = scenario.player.undoableMoves.Count > 0;
        undoButton.interactable = state;
        Color c = state ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1);
    }

    
    public virtual void PlaySound(AudioClip clip) {
        if (clip)
            audioSource.PlayOneShot(clip);
    }

    public virtual void GenericMenu(bool positive) {
        audioSource.PlayOneShot(positive ? genSelectSFX.Get() : genDeselectSFX.Get());
    }

    public void ToggleBattleCanvas(bool state) {
        canvasAnim.SetBool("Active", state);
    }


}
