using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitResults : MonoBehaviour
{

    [SerializeField] Image portrait;
    [SerializeField] TMP_Text nameTMP, hammerUsesTMP, equipUsesTMP, bulbsTMP;

    [SerializeField] Transform hpParent, emptyParent;
    [SerializeField] GameObject hpPip, emptyPip;


    public void Init(Unit u) {
        portrait.sprite = u.portrait;
        nameTMP.text = u.name;

        if (u is PlayerUnit pu) {
            hammerUsesTMP.text = pu.hammerUses.ToString();
            equipUsesTMP.text = pu.equipUses.ToString();
            bulbsTMP.text = pu.bulbPickups.ToString();
        } 

        for (int i = 1; i <= u.hpMax; i++) {
            Instantiate(emptyPip, emptyParent);
        }

        for (int i = 1; i <= u.hpCurrent; i++) {
            Instantiate(hpPip, hpParent);
        }
    }
}
