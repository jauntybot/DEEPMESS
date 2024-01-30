using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class GameUnitUI : UnitUI {

    [Header("Equipment")]
    public List<EquipmentButton> equipButtons = new();
    [SerializeField] GameObject equipmentParent, hammerParent, bulbParent, slagEquipmentButtonPrefab, hammerButtonPrefab, bulbButtonPrefab, gameUnitFramePrefab;
    [SerializeField] public SFX equipSelectSFX, hammerSelectSFX;

    [SerializeField] Animator emptyPips, hpPips;
    
    [Header("Overview")]
    [SerializeField] public UnitOverview overview;
    [SerializeField] protected GameObject overviewPrefab;

    public virtual UnitUI Initialize(Unit u, Transform overviewParent = null, Transform overviewLayoutParent = null) {
        UnitUI unitUI = base.Initialize(u);
// Hard-coded portrait sprite placement <<.<<
        if (u is PlayerUnit) {
            portrait.rectTransform.localPosition = new Vector2(-113, -95);    
            portrait.rectTransform.sizeDelta = new Vector2(600, 600);
        } else if (u is EnemyUnit) {
            portrait.rectTransform.localPosition = new Vector2(-135, -381);
            portrait.rectTransform.sizeDelta = new Vector2(600, 600);
             GameUnitFrame frame = Instantiate(gameUnitFramePrefab, equipmentParent.transform).GetComponent<GameUnitFrame>();
            frame.Init(u);
        } else if (u is Anvil) {
            portrait.rectTransform.localPosition = new Vector2(-17, -51);
            portrait.rectTransform.sizeDelta = new Vector2(400, 400);
            GameUnitFrame frame = Instantiate(gameUnitFramePrefab, equipmentParent.transform).GetComponent<GameUnitFrame>();
            frame.Init(u);
        } else if (u is Nail) {
            portrait.rectTransform.localPosition = new Vector2(-118, -170);
            portrait.rectTransform.sizeDelta = new Vector2(600, 600);
            GameUnitFrame frame = Instantiate(gameUnitFramePrefab, equipmentParent.transform).GetComponent<GameUnitFrame>();
            frame.Init(u);
        }

        if (u is PlayerUnit) {
            if (overviewParent != null) {
                UnitOverview view = Instantiate(overviewPrefab, overviewParent).GetComponent<UnitOverview>();
                overview = view.Initialize(u, overviewLayoutParent);
            }
            UpdateEquipmentButtons();
        } else {
            
        }

        ToggleUnitPanel(false);
        UpdatePips();

        return unitUI;
    }

    public override void ToggleUnitPanel(bool active) {
        base.ToggleUnitPanel(active);
        if (active) {
            UpdatePips();
            UpdateBulb();
        }
    }

    void UpdatePips() {
        emptyPips.SetInteger("Count", unit.hpMax);
        hpPips.SetInteger("Count", unit.hpCurrent);
    }

    void UpdateBulb() {
        //yield return new WaitForSecondsRealtime(0.25f);
        BulbEquipmentData b = (BulbEquipmentData)unit.equipment.Find(e => e is BulbEquipmentData);
        if (b) {
            int c = 0;
            if (b is SupportBulbData s) {
                if (s.supportType == SupportBulbData.SupportType.Heal)
                    c = 0;
                else c = 1;
            } else c = 2;

            equipButtons.Find(e => e.data is BulbEquipmentData).subAnim.SetInteger("Color", c);
        }
    }

    public void ToggleEquipmentButtons() {
        PlayerManager pManager = (PlayerManager)unit.manager;
        foreach (EquipmentButton b in equipButtons) {
            b.gameObject.GetComponentInChildren<Button>().interactable = unit.energyCurrent >= b.data.energyCost && 
                !unit.conditions.Contains(Unit.Status.Restricted) && !unit.conditions.Contains(Unit.Status.Disabled) && 
                !unit.conditions.Contains(Unit.Status.Stunned) && !pManager.unitActing;
        }      

    }

    public void UpdateEquipmentButtons() {

// Destroy buttons no longer owned by unit
        for (int i = equipButtons.Count - 1; i >= 0; i--) {
            if (unit.equipment.Find(equip => equip == equipButtons[i].data) == null) {
                EquipmentButton button = equipButtons[i];
                equipButtons.Remove(button);
                Destroy(button.gameObject);
            }
        }

// Add buttons unit owns but does not have
        for (int i = unit.equipment.Count - 1; i >= 0; i--) {
            if (unit.equipment[i] is not MoveData) {
                EquipmentButton b = equipButtons.Find(b => b.data == unit.equipment[i]);
                if (b == null) {
                    GameObject prefab = null; Transform parent = null; int index = 0;
                    if (unit.equipment[i] is SlagEquipmentData && unit.equipment[i] is not HammerData) {
                        prefab = slagEquipmentButtonPrefab;
                        parent = equipmentParent.transform;
                    } else if (unit.equipment[i] is HammerData) {
                        prefab = hammerButtonPrefab;
                        parent = hammerParent.transform;
                    } else if (unit.equipment[i] is BulbEquipmentData) {
                        prefab = bulbButtonPrefab;
                        parent = bulbParent.transform;
                        index = 1;
                    }
                    EquipmentButton newButt = Instantiate(prefab, parent).GetComponent<EquipmentButton>();
                    newButt.transform.localScale = Vector3.one;
                    newButt.Initialize(this, unit.equipment[i], unit);
                    equipButtons.Add(newButt);
                    newButt.transform.SetSiblingIndex(index);

                    if (unit.equipment[i] is BulbEquipmentData bulb) 
                        UpdateBulb();
                } else if (b.selected && b.data != unit.selectedEquipment)
                    b.DeselectEquipment();
            }
        }

        if (unit is PlayerUnit)
            ToggleEquipmentButtons();
    }

    public void DisarmButton() {
        if (unit.selectedEquipment) {
            unit.selectedEquipment.UntargetEquipment(unit);
            unit.selectedEquipment = null;
        }
        unit.grid.DisableGridHighlight();
        if (!unit.moved)
            unit.UpdateAction(unit.equipment[0]);
        else {
            unit.UpdateAction();
            PlayerManager pManager = (PlayerManager)unit.manager;
            pManager.contextuals.displaying = false;
        }
        //unit.ui.UpdateEquipmentButtons();
    }


    public void UpdateLoadout(EquipmentData equip) {
// Remove old equipment unless the same
        if (equip is SlagEquipmentData) {
            for (int i = unit.equipment.Count - 1; i >= 0; i--) {
                if (unit.equipment[i] is SlagEquipmentData e) {
                    if (equip == e) return;
                    unit.equipment.Remove(e);
                }
            }
        } 
// Add new equipment to unit
        unit.equipment.Insert(1, equip);
        PlayerUnit pu = (PlayerUnit)unit;

        UpdateEquipmentButtons(); 
        foreach (EquipmentButton button in equipButtons) button.gameObject.GetComponentInChildren<Button>().interactable = true;
    }

}