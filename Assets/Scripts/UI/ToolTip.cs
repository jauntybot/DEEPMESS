using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ToolTip : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text itemName;
    [SerializeField] TMPro.TMP_Text itemDescription;

    private GameObject go;
    private int numOfChildren;
    private string goName;

    List<string> _goList = new List<string>();
 

    private bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }


    [System.Serializable]
    public class Equipment
    {
        [SerializeField] string JSONpage;
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

        //create list from game object
        //use _goList
        CreateGameObjectList();


        //COMPARE GAMEOBJECT LIST 

        //LOAD RELEVANT TEXT TO THE TOOL TIP

        Debug.Log(_goList[2]);

        

        //myEquipmentList = JsonUtility.FromJson<EquipmentList>(textJSON.text);
    }

    void CreateGameObjectList()
    {
        //creates a list from the children of the game object
        goName = this.gameObject.name;
        go = this.gameObject;
        numOfChildren = go.transform.childCount;

        for (int i = 0; i < numOfChildren; i++)
        {
            _goList.Add(go.transform.GetChild(i).gameObject.name.ToLower());
        }
    }

    void CreateJSONList()
    {

    }
}
