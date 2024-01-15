using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class UIManager : MonoBehaviour {
    
    ScenarioManager scenario;

    public Animator canvasAnim;

    [Header("Top UIs")]
    public MetaDisplay metaDisplay;
    [SerializeField] TurnOrderHover turnOrder;

    [Header("Floor Buttons")]
    [SerializeField] public Button peekButton;
    [SerializeField] public Button endTurnButton, undoButton;

    [Header("Game UIs")]
    [SerializeField] Transform unitGameUIParent;
    [SerializeField] GameObject unitGameUIPrefab;
    public List<GameUnitUI> unitGameUIs = new();

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
        turnOrder.OnHoverCallback += ToggleEnemyTurnOrder;
    }

    public void UpdatePortrait(Unit u = null, bool active = true) {
        foreach (GameUnitUI ui in unitGameUIs) ui.ToggleUnitPanel(false);
    
        GameUnitUI unitUI = unitGameUIs.Find(p => p.unit == u);
        if (unitUI == null && u != null) { unitUI = CreateUnitUI(u); }
        if (unitUI) unitUI.ToggleUnitPanel(active);
    }

    public GameUnitUI CreateUnitUI(Unit u) {

        GameUnitUI ui = Instantiate(unitGameUIPrefab, unitGameUIParent).GetComponent<GameUnitUI>();
        u.ui = (GameUnitUI)ui.Initialize(u, overviewParent, overviewLayoutParent);
        if (u is Nail) {
            u.ui.overview = nailOverview.Initialize(u, nailOverview.transform.parent);
        }
        unitGameUIs.Add(ui);

        return ui;
    }

    public IEnumerator LoadOutScreen(bool first = false) {
        // loadoutBG.gameObject.SetActive(true);
        // loadoutPanel.gameObject.SetActive(true);
        // loadoutManager.DisplayLoadout(first);
        // while (scenario.currentTurn == ScenarioManager.Turn.Loadout) {

             yield return null;
        // }
        // loadoutBG.gameObject.SetActive(false);
        // loadoutPanel.gameObject.SetActive(false);
        // foreach(Unit u in scenario.player.units)
        //     u.ui.UpdateEquipmentButtons();
    }

    public void LockPeekButton(bool state) {
        peekButton.GetComponent<Button>().interactable = state ? false : scenario.floorManager.floors.Count - 1 > scenario.floorManager.currentFloor.index;
    }

    public void LockHUDButtons(bool state) {
        LockPeekButton(state);
        endTurnButton.interactable = !state;
        undoButton.interactable = state ? false : scenario.player.undoOrder.Count > 0;
        turnOrder.EnableHover(!state);
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

    void ToggleEnemyTurnOrder(bool state) {
        foreach(Unit u in scenario.currentEnemy.units)
            u.elementCanvas.DisplayTurnOrder(state);
    }


}
