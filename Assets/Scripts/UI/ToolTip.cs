using UnityEngine.EventSystems
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ToolTip : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text itemName;
    [SerializeField] TMPro.TMP_Text itemDescription;

    private bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }


    [System.Serializable]
    public class Equipment
    {
        public string name;
        public string description;
    }

    [System.Serializable]
    public class EquipmentList
    {
        public Equipment[] equipment;
    }

    public EquipmentList myEquipmentList = new EquipmentList();
    // Start is called before the first frame update
    void Start()
    {
        myEquipmentList = JsonUtility.FromJson<EquipmentList>(textJSON.text);
    }
}
