using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Floors/Floor Atlas")]
[System.Serializable]
public class FloorAtlas : ScriptableObject
{

    public List<FloorAsset> assets;


}

[System.Serializable]
public class FloorAsset {
    public string name;
    public GridElement ge;
    public GameObject prefab;
    public Texture icon;
}
