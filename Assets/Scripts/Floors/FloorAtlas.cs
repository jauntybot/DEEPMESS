using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Floors/Floor Atlas")]
[System.Serializable]
public class FloorAtlas : ScriptableObject
{

    public List<FloorAsset> assets;
    
}

[CreateAssetMenu(menuName = "Floors/Floor Asset")]
[System.Serializable]
public class FloorAsset : ScriptableObject {
    new public string name;
    public GridElement ge;
    public GameObject prefab;
    public Sprite icon;
}
