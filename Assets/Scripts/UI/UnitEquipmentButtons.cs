using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitEquipmentButtons : MonoBehaviour
{
    
    public List<List<EquipmentButton>> buttons;

    public void UpdateButtonsDisplayed(int index) {


    }


    public void UpdateUnitEquipmentButtons() {



    }

}


class ButtonContainer {

    public Unit owner;
    public List<EquipmentButton> equipmentButtons;

}