using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScratchUpgrade : MonoBehaviour {

    ScratchOffCard card;
    Animator anim;
    public GearUpgrade upgrade;
    [SerializeField] Image icon;
    public GameObject selection;
    [HideInInspector] public UpgradeTooltipTrigger ttTrigger;

    public void Init(ScratchOffCard c, GearUpgrade _upgrade) {
        card = c;
        upgrade = _upgrade;
        icon.sprite = upgrade.icon;
        anim = GetComponent<Animator>();

        ttTrigger =  GetComponentInChildren<UpgradeTooltipTrigger>();
        ttTrigger.Initialize(_upgrade);
    }

    public void ScratchOff() {
        anim.SetTrigger("Scratch");
    }

    public void SelectUpgrade() {
        card.SelectUpgrade(this);
    }

}
