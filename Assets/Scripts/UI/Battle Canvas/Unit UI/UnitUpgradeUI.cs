using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitUpgradeUI : UnitUI {

    [SerializeField] TMP_Text modifierTMP;

    public void UpdateModifier(string mod) {
        modifierTMP.text = mod;
    }




}
