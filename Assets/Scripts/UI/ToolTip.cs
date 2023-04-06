using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using TMPro;

public class ToolTip : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text elementName;
    [SerializeField] TMPro.TMP_Text description;
    [SerializeField] TMPro.TMP_Text damage;

    private GameObject go;
    private int numOfChildren;
    private string goName;

    public GameObject testObject;

    [SerializeField] List<GameObject> _goList = new List<GameObject>();

    public static ToolTip _instance;


    //Singleton to make sure there is only one tool tip at a time
    /*public void Awake()
    {
        if (_instance != null && _instance == this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }*/

    void Start()
    {
        CreateCSVList(testObject);
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
            _goList.Add(go.transform.GetChild(i).gameObject);
        }
    }

    void CreateCSVList(GameObject goTest)
    {
        //get game object tag
        string goName =  goTest.name;
        string goTag = goTest.tag;
        
        TextAsset data = Resources.Load<TextAsset>("Database - " + goTag);

        //create rows
        string[] textData = data.text.Split(new char[] { '\n' });

        //create columns
        for (int i = 1; i < textData.Length; i++)
        {
            //create rows
            string[] row = textData[i].Split(new char[] { ',' });
            Debug.Log("index = " + Array.IndexOf(row, row[i]) + "name = " + row[i]);

        }

        Debug.Log(textData[0]);

        //find go name in array
        for (int i = 0; i < textData.Length; i ++)
        {
            if (textData[i] == goName)
            {
                Debug.Log(textData[i]);
            }
            else
            {
                Debug.Log("Can't find.");
            }
        }
        

    }

}
