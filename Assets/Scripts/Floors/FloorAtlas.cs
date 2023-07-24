using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Floors/Floor Atlas")]
[System.Serializable]
public class FloorAtlas : ScriptableObject
{
    public FloorAsset baseTile;
    public List<FloorAsset> environmentAssets;
    public List<FloorAsset> unitAssets;
    
}


