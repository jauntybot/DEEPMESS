using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using TMPro;

public class ToolTipData : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text elementName;
    [SerializeField] TMPro.TMP_Text description;
    [SerializeField] TMPro.TMP_Text damage;

    public GridElement testGE;
    public static ToolTipData _instance;

    //Singleton to make sure there is only one tool tip at a time
    public void Awake()
    {
        if (_instance != null && _instance == this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    void Start()
    {
        CreateCSVList(testGE);
    }

    void CreateCSVList(GridElement geTest)
    {
        //get game object tag
        string goName = geTest.name;
        TextAsset data = Resources.Load<TextAsset>("Database - " + geTest.GetType().Name);

        //create rows
        string[] textData = data.text.Split(new char[] { '\n' });

        Debug.Log(textData[2]);

        int nameLength = goName.Length;
        int index = 0;

        //create columns
        for (int i = 1; i < textData.Length; i++)
        {
            string refName = "";
            for (int l = 0; l < nameLength; l++)
                refName += textData[i][l];

            if (goName == refName)
            {
                index = i;
                Debug.Log(goName + " is at index " + i);

                string[] column = textData[i].Split(new char[] { ',' });
                elementName.text = column[0];
                description.text = column[1];
                damage.text = column[2];
            }
        }
    }
}
