using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MetaDisplay : MonoBehaviour
{
    [SerializeField] ScenarioManager scenarioManager;

    private int turns;
    private int maxTurns;
    private int turnsLeft;
    private int turnsLeftClamp;
    public int currentFloorNum;
    private List<Grid> floorNumber;

    //UI text objects
    public GameObject goTurnsLeft;
    public GameObject goFloorNumber;

    //Text componenets
    TextMeshProUGUI turnsLeftText;
    TextMeshProUGUI floorNumberText;

    // Start is called before the first frame update
    void Start()
    {
        maxTurns = scenarioManager.turnsToDescend;
        turnsLeftText = goTurnsLeft.GetComponent<TextMeshProUGUI>();
        floorNumberText = goFloorNumber.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        TurnsLeft();
        FloorNumber();
    }

    //display how many turns player has left on floor
    public void TurnsLeft()
    {
        turns = scenarioManager.turnCount;
        turnsLeft = (maxTurns + 1) - turns;
        turnsLeftClamp = Mathf.Clamp(turnsLeft, 0, maxTurns);
        turnsLeftText.text = turnsLeftClamp.ToString();
    }

    //display what floor the player is currently on
    public void FloorNumber()
    {
        currentFloorNum = scenarioManager.GetComponent<FloorManager>().currentFloor.index + 1;
        floorNumberText.text = currentFloorNum.ToString();
    }
}
