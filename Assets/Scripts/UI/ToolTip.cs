using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;

[System.Serializable]
public class PlayerInfo
{

}

public class ToolTip : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text itemName;
    [SerializeField] TMPro.TMP_Text itemDescription;

    private GameObject go;
    private int numOfChildren;
    private string goName;

    [SerializeField] List<string> _goList = new List<string>();

    void Start()
    {

        //create list from game object
        //use _goList
        CreateGameObjectList();


        //COMPARE GAMEOBJECT LIST

        //LOAD RELEVANT TEXT TO THE TOOL TIP

        Debug.Log(_goList[2]);
    }

    /// <summary>
    /// Create a list from the children of the game object
    /// </summary>
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

    private bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
