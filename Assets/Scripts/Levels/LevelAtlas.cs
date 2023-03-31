using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Grid/Level Atlas")]
[System.Serializable]
public class LevelAtlas : ScriptableObject
{

    public List<LevelAsset> assets;


}

[System.Serializable]
public class LevelAsset {
    public string name;
    public GridElement ge;
    public GameObject prefab;
    public Texture icon;
}
