using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    
    public static SaveDataManager instance;
    void Awake() {
        if (SaveDataManager.instance)
            return;
        SaveDataManager.instance = this;
    }


    ScenarioManager scenario;
    public FloorChunk savedPacket;

    void LoadSavedState() {

        
    }




}
