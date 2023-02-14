using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HammerChargeDisplay : MonoBehaviour
{

    public Color emptyCharge, fullCharge;
    public List<Image> charges;


    public void UpdateCharges(int num) {
        foreach(Image charge in charges) charge.color = emptyCharge;
        for (int i = 0; i < num; i++) {
            charges[i].color = fullCharge;
        }
    }
}
