using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[CreateAssetMenu(menuName = "Grid/Level Definition")]
[System.Serializable]
public class LevelDefinition : ScriptableObject
{

    public LevelAtlas atlas;
    public List<Content> initSpawns;

}

[System.Serializable]
public class Content {

    public Vector2 coord;
    public GridElement prefabToSpawn;
}

