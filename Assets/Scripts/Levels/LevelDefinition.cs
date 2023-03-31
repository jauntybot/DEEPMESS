using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[CreateAssetMenu(menuName = "Grid/Level Definition")]
[System.Serializable]
public class LevelDefinition : ScriptableObject
{

    public LevelAtlas atlas;
    public int genPool;
    public List<Spawn> initSpawns;

}

[System.Serializable]
public class Spawn {

    public LevelAsset asset;
    public Vector2 coord;

}

